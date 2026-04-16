using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Local AI provider implementation using Ollama.
/// This is intended to make AI fallback testable without paid hosted APIs.
/// </summary>
public sealed class OllamaAuthorProfileService : IAiAuthorProfileService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl;
    private string _model;
    private readonly IAppSettingsRepository _appSettingsRepository;

    private readonly IPromptTemplateProvider _prompts;
    private readonly ILogger<OllamaAuthorProfileService> _logger;
    public OllamaAuthorProfileService(HttpClient httpClient,
                                      IAppSettingsRepository appSettingsRepository,
                                      IPromptTemplateProvider prompts,
                                      ILogger<OllamaAuthorProfileService> logger)
    {
        _httpClient = httpClient;
        _appSettingsRepository = appSettingsRepository;
        _prompts = prompts;
        _logger = logger;
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, string language, CancellationToken cancellationToken = default)
    {
        var template = await _prompts.GetTemplateAsync("author-profile.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["name"] = name,
            ["language"] = language
        });
        
        var (modelBaseUrl, modelName) = await GetAiSettings();
        _baseUrl = modelBaseUrl;
        _model = modelName;

        var payload = new OllamaGenerateRequest
        {
            Model = _model,
            Prompt = prompt,
            Stream = false
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/api/generate")
        {
            Content = JsonContent.Create(payload)
        };

        using var resp = await _httpClient.SendAsync(req, cancellationToken);

        resp.EnsureSuccessStatusCode();

        // Ollama may return either a single JSON object or NDJSON (one JSON object per line).
        // Reading bytes + decoding defensively avoids DecoderFallbackException when the response contains
        // unexpected encodings.
        var rawBytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);
        var raw = DecodeUtf8Lossy(rawBytes);
        var text = TryExtractOllamaResponseText(raw);

        // Use repairer to robustly parse or attempt to repair malformed JSON responses.
        var parsed = await AuthorProfileJsonRepairer.ParseOrRepairAsync(
            text,
            async (repairPrompt) =>
            {
                var payload = new OllamaGenerateRequest
                {
                    Model = _model,
                    Prompt = repairPrompt,
                    Stream = false
                };

                using var req2 = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/api/generate")
                {
                    Content = JsonContent.Create(payload)
                };

                using var resp2 = await _httpClient.SendAsync(req2, cancellationToken);
                resp2.EnsureSuccessStatusCode();
                var rawBytes2 = await resp2.Content.ReadAsByteArrayAsync(cancellationToken);
                var raw2 = DecodeUtf8Lossy(rawBytes2);
                var repairedText = TryExtractOllamaResponseText(raw2);
                return repairedText;
            },
            _logger,
            language);

        return parsed;
    }

    private sealed class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; }
    }

 private static string TryExtractOllamaResponseText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Fast path: single JSON object.
        try
        {
            var single = JsonSerializer.Deserialize<OllamaGenerateResponse>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrWhiteSpace(single?.Response))
                return single.Response!;
        }
        catch
        {
            // Ignore and attempt NDJSON parsing.
        }

        // NDJSON path: parse each line as JSON and concatenate the response fields.
        var sb = new StringBuilder();
        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var item = JsonSerializer.Deserialize<OllamaGenerateResponse>(
                    line,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrWhiteSpace(item?.Response))
                    sb.Append(item.Response);
            }
            catch
            {
                // Ignore non-JSON lines.
            }
        }

        return sb.ToString();
    }

    private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return (settings.ModelBaseUrl, settings.AiModelName);
    }

    private static string DecodeUtf8Lossy(byte[] bytes)
        => Encoding.UTF8.GetString(bytes);

    private sealed class OllamaGenerateResponse
    {
        public string? Response { get; set; }
    }
}
