using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaTranslateService : ITranslateService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaTranslateService> _logger;
    private string _ollamaBaseUrl;
    private string _model;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaTranslateService(HttpClient httpClient,
                                  ILogger<OllamaTranslateService> logger,
                                  IPromptTemplateProvider prompts,
                                  IAppSettingsRepository appSettingsRepository,
                                  IProgressPublisher progressPublisher)
    {
        _httpClient = httpClient;
        _logger = logger;
        _prompts = prompts;
        _appSettingsRepository = appSettingsRepository;
        _progressPublisher = progressPublisher;
    }

    public async Task<TranslateResponseDto> TranslateAsync(TranslateRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        await PublishAsync(request.ConnectionId, request.CorrelationId, "preparing_prompt", 5, "Preparing translation prompt...", cancellationToken);
        var template = await _prompts.GetTemplateAsync("translate-text.prompt.txt", cancellationToken);

        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["sourceLanguage"] = ResolveLanguageName(request.SourceLanguage),
            ["targetLanguage"] = ResolveLanguageName(request.TargetLanguage),
            ["text"] = request.Text
        });

        var (modelBaseUrl, modelName) = await GetAiSettings();
        _ollamaBaseUrl = modelBaseUrl;
        _model = modelName;

        var payload = new
        {
            model = _model,
            prompt,
            stream = false
        };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_ollamaBaseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            await PublishAsync(request.ConnectionId, request.CorrelationId, "request_sent", 40, "Sending translation prompt to AI model...", cancellationToken);
            using var resp = await _httpClient.SendAsync(req, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Translate: Ollama request failed. Status={Status}", resp.StatusCode);
                return new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = $"Translation backend returned status {(int)resp.StatusCode} ({resp.StatusCode})."
                };
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes);

            var text = TryExtractOllamaResponseText(json, _logger);
            await PublishAsync(request.ConnectionId, request.CorrelationId, "parsing", 80, "Parsing translation response...", cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
                return new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "No response received from translation backend."
                };

            // Attempt to parse direct JSON
            try
            {
                var parsed = JsonSerializer.Deserialize<TranslateResponseDto>(text, JsonOptions);
                if (parsed is not null)
                {
                    await PublishAsync(request.ConnectionId, request.CorrelationId, "completed", 100, "Translation completed.", cancellationToken);
                    return parsed;
                }
            }
            catch { }

            // Fallback: try extract first JSON object
            var extracted = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<TranslateResponseDto>(extracted, JsonOptions);
                    if (parsed is not null)
                    {
                        await PublishAsync(request.ConnectionId, request.CorrelationId, "completed", 100, "Translation completed.", cancellationToken);
                        return parsed;
                    }
                }
                catch { }
            }

            // If we reach here return raw text as translatedText with low confidence
            await PublishAsync(request.ConnectionId, request.CorrelationId, "completed", 100, "Translation completed.", cancellationToken);
            return new TranslateResponseDto { TranslatedText = text.Trim(), Confidence = 0.2 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for translate");
            await PublishAsync(request.ConnectionId, request.CorrelationId, "failed", 100, ex.Message ?? "Translation failed.", cancellationToken, "error");
            // Return a concise user-facing error message; full exception remains in logs
            return new TranslateResponseDto
            {
                TranslatedText = string.Empty,
                Confidence = 0,
                ErrorMessage = "Translation service error: " + (ex.Message ?? "Unexpected error")
            };
        }
    }

    // Reuse helpers from OllamaBookSummaryService via copying minimal implementations
    private static string? TryExtractFirstJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var start = text.IndexOf('{');
        if (start < 0)
            return null;

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var ch = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
                depth++;
            else if (ch == '}')
                depth--;

            if (depth == 0)
                return text.Substring(start, i - start + 1);
        }

        return null;
    }

    private static string? TryExtractOllamaResponseText(string json, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
            {
                var responseText = response.GetString();
                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    // Clean common markdown / code-fence wrappers and leading noise
                    static string CleanResponse(string s)
                    {
                        if (string.IsNullOrWhiteSpace(s)) return s;
                        s = s.Trim();

                        // Triple backtick fences ```json ... ```
                        if (s.StartsWith("```"))
                        {
                            var last = s.LastIndexOf("```", StringComparison.Ordinal);
                            if (last > 2)
                            {
                                var inner = s.Substring(3, last - 3);
                                return inner.Trim();
                            }
                        }

                        // Single backticks `...`
                        if (s.Length >= 2 && s[0] == '`' && s[^1] == '`')
                            return s.Substring(1, s.Length - 2).Trim();

                        // If there is leading text before the first JSON object/array, strip it
                        var firstJsonIndex = s.IndexOfAny(new[] { '{', '[' });
                        if (firstJsonIndex > 0)
                            return s.Substring(firstJsonIndex).Trim();

                        return s;
                    }

                    var cleaned = CleanResponse(responseText);

                    if (!string.IsNullOrWhiteSpace(cleaned) && (cleaned[0] == '{' || cleaned[0] == '['))
                    {
                        try
                        {
                            using var innerDoc = JsonDocument.Parse(cleaned);
                            return innerDoc.RootElement.ToString();
                        }
                        catch (JsonException ex)
                        {
                            logger.LogWarning(ex, "Malformed stringified JSON in response after cleaning; returning raw responseText");
                            return responseText;
                        }
                    }

                    return responseText;
                }
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Malformed JSON encountered: {Json}", json);
            return null;
        }

        // NDJSON fallback
        var sb = new StringBuilder();
        foreach (var line in json.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
                    sb.Append(response.GetString());
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Malformed NDJSON line encountered: {Line}", line);
            }
        }

        var merged = sb.ToString();
        return string.IsNullOrWhiteSpace(merged) ? null : merged;
    }

    private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return (settings.ModelBaseUrl, settings.AiModelName);
    }

    private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pt"] = "Portuguese",
        ["en"] = "English",
        ["fr"] = "French",
        ["es"] = "Spanish",
        ["de"] = "German",
        ["it"] = "Italian",
    };

    private static string ResolveLanguageName(string code)
        => LanguageNames.TryGetValue(code, out var name) ? name : code;

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
                Service = nameof(OllamaTranslateService),
                Operation = "translate"
            },
            connectionId,
            cancellationToken);

}
