using MediaOrganizerApp.Application.DTOs.Api.WikiData;
using MediaOrganizerApp.Application.Interfaces.Services.WikiData;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.WikiData
{
    public class WikidataService : IWikidataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WikidataService> _logger;

        private const string SparqlEndpoint = "https://query.wikidata.org/sparql";
        private const string UserAgentValue = "MediaOrganizerApp/1.0 (+dev@yourdomain.com)";

        public WikidataService(HttpClient httpClient, ILogger<WikidataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // =========================================================
        // PUBLIC API
        // =========================================================
        public async Task<List<BookResult>> GetBooksByAuthorAsync(string author, string lang)
        {
            try
            {
                var qid = await GetAuthorQid(author, lang);

                if (string.IsNullOrWhiteSpace(qid))
                    return new List<BookResult>();

                var query = $@"
                SELECT ?book ?bookLabel ?image ?date WHERE {{
                  ?book wdt:P50 wd:{qid}.
                  ?book wdt:P31 wd:Q571.

                  OPTIONAL {{ ?book wdt:P18 ?image. }}
                  OPTIONAL {{ ?book wdt:P577 ?date. }}

                  SERVICE wikibase:label {{
                    bd:serviceParam wikibase:language ""{lang},en"".
                  }}
                }}";

                using var request = new HttpRequestMessage(HttpMethod.Post, SparqlEndpoint);
                request.Headers.UserAgent.ParseAdd(UserAgentValue);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sparql-results+json"));

                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("query", query)
                });

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("SPARQL error {Status}: {Body}", (int)response.StatusCode, body);
                    return new List<BookResult>();
                }

                var data = JsonSerializer.Deserialize<SparqlResult>(body);

                return data?.results?.bindings?
                    .Select(b => new BookResult
                    {
                        Title = b.bookLabel?.value,
                        CoverImage = b.image?.value,
                        PublicationYear = ParseYear(b.date?.value)
                    })
                    .ToList()
                    ?? new List<BookResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBooksByAuthorAsync");
                return new List<BookResult>();
            }
        }

        // =========================================================
        // AUTHOR RESOLVER (FIXED + TYPE SAFE)
        // =========================================================
        private async Task<string?> GetAuthorQid(string author, string lang)
        {
            try
            {
                var authorDecoupled = author
                    .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(authorDecoupled))
                    return null;

                var url =
                    $"https://www.wikidata.org/w/api.php" +
                    $"?action=wbsearchentities" +
                    $"&search={Uri.EscapeDataString(authorDecoupled)}" +
                    $"&language={Uri.EscapeDataString(lang)}" +
                    $"&type=item" +
                    $"&limit=10" +
                    $"&format=json";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd(UserAgentValue);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Wikidata search error {Status}: {Body}",
                        (int)response.StatusCode, body);
                    return null;
                }

                var result = JsonSerializer.Deserialize<WikidataSearchResult>(body);

                var candidates = result?.search ?? new List<WikidataSearchItem>();

                var best = candidates
                    .Select(c => new
                    {
                        c.id,
                        Score = ScoreAuthorCandidate(c)
                    })
                    .Where(x => x.Score > int.MinValue)
                    .OrderByDescending(x => x.Score)
                    .FirstOrDefault();

                return best?.id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving author QID for {Author}", author);
                return null;
            }
        }

        // =========================================================
        // AUTHOR FILTER + SCORING (STRICT WHITELIST)
        // =========================================================
        private int ScoreAuthorCandidate(WikidataSearchItem c)
        {
            var desc = c.description?.ToLowerInvariant() ?? "";

            // ONLY allow author-like entities
            bool isAuthor =
                desc.Contains("writer") ||
                desc.Contains("author") ||
                desc.Contains("novelist") ||
                desc.Contains("poet") ||
                desc.Contains("playwright") ||
                desc.Contains("essayist") ||
                desc.Contains("literary");

            if (!isAuthor)
                return int.MinValue;

            int score = 0;

            if (desc.Contains("novelist")) score += 12;
            if (desc.Contains("writer")) score += 10;
            if (desc.Contains("author")) score += 10;
            if (desc.Contains("poet")) score += 8;
            if (desc.Contains("playwright")) score += 7;
            if (desc.Contains("essayist")) score += 6;

            if (!string.IsNullOrWhiteSpace(c.label)) score += 2;
            if (!string.IsNullOrWhiteSpace(c.description)) score += 1;

            return score;
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private int? ParseYear(string? date)
        {
            if (DateTime.TryParse(date, out var dt))
                return dt.Year;

            return null;
        }
    }
}