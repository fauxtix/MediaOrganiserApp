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
    private string _baseUrl;
    private string _model;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IPromptTemplateProvider _prompts;

    public OllamaTranslateService(HttpClient httpClient, ILogger<OllamaTranslateService> logger, IPromptTemplateProvider prompts, IAppSettingsRepository appSettingsRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _prompts = prompts;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<TranslateResponseDto> TranslateAsync(TranslateRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var template = await _prompts.GetTemplateAsync("translate-text.prompt.txt", cancellationToken);

        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["sourceLanguage"] = ResolveLanguageName(request.SourceLanguage),
            ["targetLanguage"] = ResolveLanguageName(request.TargetLanguage),
            ["text"] = request.Text
        });
        _logger.LogDebug("Rendered translate prompt:\n{Prompt}", prompt);

        var (modelBaseUrl, modelName) = await GetAiSettings();
        _baseUrl = modelBaseUrl;
        _model = modelName;

        var payload = new
        {
            model = _model,
            prompt,
            stream = false
        };

            try
            {
            using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

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
            _logger.LogDebug("Raw Ollama response JSON: {Json}", json);
            _logger.LogDebug("Extracted response text: {Text}", text);
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
                if (parsed is not null) return parsed;
            }
            catch { }

            // Fallback: try extract first JSON object
            var extracted = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<TranslateResponseDto>(extracted, JsonOptions);
                    if (parsed is not null) return parsed;
                }
                catch { }
            }

            // If we reach here return raw text as translatedText with low confidence
            return new TranslateResponseDto { TranslatedText = text.Trim(), Confidence = 0.2 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for translate");
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
                    try
                    {
                        using var innerDoc = JsonDocument.Parse(responseText);
                        return innerDoc.RootElement.ToString();
                    }
                    catch (JsonException ex)
                    {
                        logger.LogWarning(ex, "Malformed stringified JSON in response");
                        return responseText;
                    }
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

}
