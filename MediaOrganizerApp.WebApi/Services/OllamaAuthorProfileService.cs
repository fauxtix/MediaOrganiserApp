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
    private readonly IProgressPublisher _progressPublisher;
    
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaAuthorProfileService(HttpClient httpClient,
                                      IAppSettingsRepository appSettingsRepository,
                                      IPromptTemplateProvider prompts,
                                      ILogger<OllamaAuthorProfileService> logger,
                                      IProgressPublisher progressPublisher)
    {
        _httpClient = httpClient;
        _appSettingsRepository = appSettingsRepository;
        _prompts = prompts;
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Preparing author profile prompt...", cancellationToken);
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

        await PublishAsync(connectionId, correlationId, "request_sent", 40, "Sending author profile prompt to AI model...", cancellationToken);
        using var resp = await _httpClient.SendAsync(req, cancellationToken);

        resp.EnsureSuccessStatusCode();

        // Ollama may return either a single JSON object or NDJSON (one JSON object per line).
        // Reading bytes + decoding defensively avoids DecoderFallbackException when the response contains
        // unexpected encodings.
        var rawBytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);
        var raw = DecodeUtf8Lossy(rawBytes);
        var text = TryExtractOllamaResponseText(raw);
        await PublishAsync(connectionId, correlationId, "parsing", 75, "Parsing author profile response...", cancellationToken);

        // Use repairer to robustly parse or attempt to repair malformed JSON responses.
        var parsed = await AuthorProfileJsonRepairer.ParseOrRepairAsync(
            text,
            async (repairPrompt) =>
            {
                await PublishAsync(connectionId, correlationId, "repairing", 85, "Repairing malformed author profile JSON...", cancellationToken);
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

        await PublishAsync(connectionId, correlationId, "completed", 100, "Author profile completed.", cancellationToken);
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
            var single = JsonSerializer.Deserialize<OllamaGenerateResponse>(raw, s_jsonOptions);

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
                var item = JsonSerializer.Deserialize<OllamaGenerateResponse>(line, s_jsonOptions);

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

    private Task PublishAsync(
        string? connectionId,
        string? correlationId,
        string stage,
        int percent,
        string message,
        CancellationToken cancellationToken,
        string level = "info")
        => _progressPublisher.PublishAsync(
            ProgressChannel.BooksInfo,
            new ProgressUpdateDto
            {
                CorrelationId = correlationId,
                Stage = stage,
                Percent = percent,
                Message = message,
                Level = level,
                Service = nameof(OllamaAuthorProfileService),
                Operation = "author-profile"
            },
            connectionId,
            cancellationToken);
}
