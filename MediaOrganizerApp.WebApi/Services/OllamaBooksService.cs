using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaBooksService : IAiBooksService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaBooksService> _logger;
        private readonly IConfiguration _config;
        private readonly IPromptTemplateProvider _prompts;

        public OllamaBooksService(HttpClient httpClient, ILogger<OllamaBooksService> logger, IConfiguration config, IPromptTemplateProvider prompts)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            _prompts = prompts;
        }

        public async Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author, string language, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();

            try
            {
                // internal default/cap for results when AI generates a list
                var defaultMaxResults = 20;
                var template = await _prompts.GetTemplateAsync("author-books.prompt.txt", cancellationToken);
                var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
                {
                    ["author"] = author,
                    ["max_results"] = defaultMaxResults.ToString(),
                    ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
                });

                var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
                var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

                var payload = new { model, prompt, stream = false };

                using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
                {
                    Content = JsonContent.Create(payload)
                };

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Ollama returned non-success status {Status} when requesting books for {Author}", response.StatusCode, author);
                    return new List<GoogleBookInfo>();
                }

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var raw = Encoding.UTF8.GetString(bytes);
                var text = TryExtractOllamaResponseText(raw);

                if (string.IsNullOrWhiteSpace(text))
                    return new List<GoogleBookInfo>();

                try
                {
                    var parsed = JsonSerializer.Deserialize<List<GoogleBookInfo>>(text, JsonOptions);
                    if (parsed is null) return new List<GoogleBookInfo>();

                    var result = new List<GoogleBookInfo>();
                    foreach (var item in parsed)
                    {
                        if (item == null) continue;
                        if (string.IsNullOrWhiteSpace(item.Title)) continue;
                        item.Thumbnail = string.IsNullOrWhiteSpace(item.Thumbnail) ? "images/NoPhoto.png" : item.Thumbnail;
                        // Clamp confidence if present
                        if (item.Confidence is not null)
                        {
                            if (item.Confidence < 0) item.Confidence = 0;
                            if (item.Confidence > 1) item.Confidence = 1;
                        }

                        result.Add(item);
                        if (result.Count >= defaultMaxResults) break;
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Ollama books response for {Author}. Raw: {Raw}", author, text);
                    return new List<GoogleBookInfo>();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("OllamaBooksService request canceled for author {Author}", author);
                return new List<GoogleBookInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Ollama for books by {Author}", author);
                return new List<GoogleBookInfo>();
            }
        }

        private static string? TryExtractOllamaResponseText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
                {
                    var responseText = response.GetString();
                    if (!string.IsNullOrWhiteSpace(responseText))
                    {
                        try
                        {
                            using var inner = JsonDocument.Parse(responseText);
                            return inner.RootElement.ToString();
                        }
                        catch
                        {
                            return responseText;
                        }
                    }
                }
            }
            catch { /* fall back to NDJSON */ }

            var sb = new StringBuilder();
            foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
                        sb.Append(response.GetString());
                    else
                        sb.Append(line);
                }
                catch
                {
                    sb.Append(line);
                }
            }

            var merged = sb.ToString();
            return string.IsNullOrWhiteSpace(merged) ? null : merged;
        }
    }
}
