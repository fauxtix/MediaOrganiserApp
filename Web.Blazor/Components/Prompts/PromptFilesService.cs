using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Components.Prompts
{
    public class PromptFilesService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PromptFilesService> _logger;
        private readonly IConfiguration _env;
        private readonly string _Uri;

        public PromptFilesService(HttpClient http, ILogger<PromptFilesService> logger, IConfiguration env)
        {
            _http = http;
            _logger = logger;
            _env = env;
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var apiKey = _env["ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    _http.DefaultRequestHeaders.Add("ApiKey", apiKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to add ApiKey header (invalid value)");
                }
            }

            var baseUrl = _env["BaseUrl"] ?? string.Empty;
            _Uri = string.IsNullOrWhiteSpace(baseUrl) ? "api" : $"{baseUrl.TrimEnd('/')}/AI";
        }

        public async Task<List<string>> GetPromptFilesAsync(CancellationToken cancellationToken = default)
        {
            var files = await _http.GetFromJsonAsync<List<string>>($"{_Uri}/prompts", cancellationToken);
            return files ?? new List<string>();
        }

        public async Task<string> GetPromptFileContentAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return await _http.GetStringAsync($"{_Uri}/prompts/{fileName}", cancellationToken);
        }

        public async Task<string> GetReadmeContentAsync(CancellationToken cancellationToken = default)
        {
            return await _http.GetStringAsync($"{_Uri}/prompts/readme", cancellationToken);
        }

        public async Task SavePromptFileAsync(string fileName, string content, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName required", nameof(fileName));
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (content.Length > 200_000) throw new InvalidOperationException("Prompt file too large.");

            _logger.LogInformation("Sending PUT request to save file: {FileName} with content length: {ContentLength}", fileName, content.Length);
            _logger.LogInformation("Request URI: {RequestUri}", $"{_Uri}/prompts/{fileName}");

            // Disambiguate the PutAsJsonAsync call by invoking the static method explicitly
            var response = await System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync(
                _http,
                $"{_Uri}/prompts/{fileName}",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("File saved to runtime folder successfully.");
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to save file to runtime folder. Status: {StatusCode}. Body: {Body}", response.StatusCode, body);
                throw new HttpRequestException($"Failed to save file to runtime folder. Status: {response.StatusCode}");
            }
        }
    }
}