using MediaOrganizerApp.Application.DTOs.Api.WikiData;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Services.DocumentHelpers;

namespace Web.Blazor.Services.ApiWrappers
{
    public class WikidataClientService : IWikidataClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<WikidataClientService> _logger;
        private readonly string _uri;
        private readonly IConfiguration _env;


        public WikidataClientService(HttpClient http,
            ILogger<WikidataClientService> logger,
            IConfiguration env)
        {
            _http = http;
            _logger = logger;
            _env = env;
            _http.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _uri = $"{_env["BaseUrl"]}/api/wikidata";
        }

        public async Task<List<BookResult>?> GetBooksByAuthorAsync(string author, string lang = "en")
        {
            if (string.IsNullOrWhiteSpace(author))
                return new List<BookResult>();

            try
            {
                var url = $"{_uri}/books-by-author/{Uri.EscapeDataString(author)}/{Uri.EscapeDataString(lang)}";
                _logger.LogDebug("WikidataClientService: GET {Url}", url);

                using var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("WikidataClientService: Response {StatusCode} Body: {Body}", (int)response.StatusCode, body);

                if (!response.IsSuccessStatusCode)
                {
                    // Return null to preserve previous behavior and let caller/logging decide;
                    // we already logged the status and full body for diagnosis.
                    return null;
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<List<BookResult>>(body);
                return result ?? new List<BookResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Wikidata API");
                return null;
            }
        }

        public async Task<(string bio, string photo, string birthDate)> GetWikiDataAuthorDetails(string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return (string.Empty, string.Empty, string.Empty);

            try
            {
                var url = $"{_uri}/author-bio/{Uri.EscapeDataString(author)}";
                _logger.LogDebug("WikidataClientService: GET {Url}", url);

                using var response = await _http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("WikidataClientService: Response {StatusCode} Body: {Body}", (int)response.StatusCode, body);

                if (!response.IsSuccessStatusCode)
                {
                    // Return empty tuple to preserve previous behavior and let caller/logging decide;
                    // we already logged the status and full body for diagnosis.
                    return (string.Empty, string.Empty, string.Empty);
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<(string bio, string photo, string birthDate)?>(body);
                return result ?? (string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Wikidata API");
                return (string.Empty, string.Empty, string.Empty);
            }
        }

    }
}