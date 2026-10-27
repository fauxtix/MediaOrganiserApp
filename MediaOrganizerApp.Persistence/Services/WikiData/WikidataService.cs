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
using static System.Net.WebRequestMethods;

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
  
                  VALUES ?classes {{ wd:Q571 wd:Q7725634 }}
                  ?book wdt:P31/wdt:P279* ?classes.

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


                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<SparqlResult>(body, options);


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
                    $"&uselang=en" +
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

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<WikidataSearchResult>(body, options);

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
        // =========================================================
        // AUTHOR FILTER + SCORING (CORRIGIDO)
        // =========================================================
        private int ScoreAuthorCandidate(WikidataSearchItem c)
        {
            var desc = c.description?.ToLowerInvariant() ?? "";
            int score = 0;

            // 1. Pontuação para termos em Inglês
            if (desc.Contains("novelist")) score += 12;
            if (desc.Contains("writer")) score += 10;
            if (desc.Contains("author")) score += 10;
            if (desc.Contains("poet")) score += 8;
            if (desc.Contains("playwright")) score += 7;
            if (desc.Contains("essayist")) score += 6;
            if (desc.Contains("biographer")) score += 5;

            // 2. Pontuação para termos em Português (essencial se lang = "pt")
            if (desc.Contains("romancista")) score += 12;
            if (desc.Contains("escritor") || desc.Contains("escritora")) score += 10;
            if (desc.Contains("autor") || desc.Contains("autora")) score += 10;
            if (desc.Contains("poeta")) score += 8;
            if (desc.Contains("dramaturgo") || desc.Contains("dramaturga")) score += 7;
            if (desc.Contains("ensaísta")) score += 6;
            if (desc.Contains("biógrafo") || desc.Contains("biógrafa")) score += 5;

            // 3. Critérios gerais de integridade do resultado
            if (desc.Contains("literary") || desc.Contains("literário") || desc.Contains("literária")) score += 4;
            if (!string.IsNullOrWhiteSpace(c.label)) score += 2;
            if (!string.IsNullOrWhiteSpace(c.description)) score += 1;

            // Retorna a pontuação acumulada. Se não fizer correspondência com nenhum termo, 
            // a pontuação será baixa, mas o candidato NÃO será descartado com int.MinValue.
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


        //
        // WikiDataPUBLIC API: Get author details (BIO, photo, birth year)
        /*
         * This method demonstrates how to fetch multiple properties in one SPARQL query.
         * It also includes basic BIO construction logic based on available data.
         */

        public async Task<(string bio, string photo, string birthDate)> GetWikiDataAuthorDetails(string authorName)
        {
            var qid = await GetAuthorQid(authorName);

            if (qid == null)
                return ("", "images/NoPhoto.png", "");

            var query = $@"
        SELECT ?description ?occupationLabel ?image ?birthDate WHERE {{
          BIND(wd:{qid} AS ?author)

          OPTIONAL {{
            ?author schema:description ?description
            FILTER (lang(?description) = ""en"")
          }}

          OPTIONAL {{ ?author wdt:P106 ?occupation. }}
          OPTIONAL {{ ?author wdt:P18 ?image. }}
          OPTIONAL {{ ?author wdt:P569 ?birthDate. }}

          SERVICE wikibase:label {{
            bd:serviceParam wikibase:language ""en"".
          }}
        }}";

            var request = new HttpRequestMessage(HttpMethod.Post, "https://query.wikidata.org/sparql");
            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("query", query)
        });

            request.Headers.Add("Accept", "application/sparql-results+json");
            request.Headers.Add("User-Agent", "MyApp/1.0");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var bindings = doc.RootElement
                .GetProperty("results")
                .GetProperty("bindings");

            if (bindings.GetArrayLength() == 0)
                return ("", "images/NoPhoto.png", "");

            // ------------------------
            // Extract data
            // ------------------------

            string? description = null;
            string? imageUrl = null;
            string? birthDate = null;

            var occupations = new List<string>();

            foreach (var b in bindings.EnumerateArray())
            {
                if (description == null && b.TryGetProperty("description", out var desc))
                    description = desc.GetProperty("value").GetString();

                if (imageUrl == null && b.TryGetProperty("image", out var img))
                    imageUrl = img.GetProperty("value").GetString();

                if (birthDate == null && b.TryGetProperty("birthDate", out var bd))
                {
                    var raw = bd.GetProperty("value").GetString();
                    if (DateTime.TryParse(raw, out var dt))
                        birthDate = dt.Year.ToString();
                }

                if (b.TryGetProperty("occupationLabel", out var occ))
                {
                    var val = occ.GetProperty("value").GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        occupations.Add(val);
                }
            }

            // ------------------------
            // Build BIO sentence
            // ------------------------

            var bio = BuildBio(authorName, description, occupations);

            // ------------------------
            // Fix image URL (Wikidata → Wikimedia)
            // ------------------------

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = imageUrl
                    .Replace(" ", "_"); // basic fix for spaces
            }
            else
            {
                imageUrl = "images/NoPhoto.png";
            }

            return (bio, imageUrl, birthDate ?? "");
        }

        // -----------------------------------
        // QID resolver (simple version)
        // -----------------------------------
        private async Task<string?> GetAuthorQid(string name)
        {
            var url =
                $"https://www.wikidata.org/w/api.php" +
                $"?action=wbsearchentities" +
                $"&search={Uri.EscapeDataString(name)}" +
                $"&language=en&format=json";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd(UserAgentValue);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);

                var results = doc.RootElement.GetProperty("search");

                if (results.GetArrayLength() == 0)
                    return null;

                return results[0].GetProperty("id").GetString();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        // -----------------------------------
        // BIO builder
        // -----------------------------------
        private string BuildBio(string name, string? description, List<string> occupations)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return Capitalize(description) + ".";
            }

            var distinct = occupations.Distinct().ToList();

            if (distinct.Count == 0)
                return $"{name} is an author.";

            string occText;

            if (distinct.Count == 1)
                occText = distinct[0];
            else
                occText = string.Join(", ", distinct.Take(distinct.Count - 1))
                          + " and " + distinct.Last();

            return $"{name} was a {occText}.";
        }

        private string Capitalize(string s)
            => char.ToUpper(s[0]) + s.Substring(1);
    }
}