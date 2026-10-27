using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaTranslateService : OllamaBaseClient, ITranslateService
{
    private const string ProviderName = "ollama";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> KnownCodeFenceMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        "json",
        "javascript",
        "js",
        "csharp",
        "cs"
    };

    private readonly ILogger<OllamaTranslateService> _logger;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaTranslateService(
        HttpClient httpClient,
        ILogger<OllamaTranslateService> logger,
        IPromptTemplateProvider prompts,
        IProgressPublisher progressPublisher,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService)
        : base(httpClient, aiSettingsProvider, systemPromptService, prompts)
    {
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public async Task<TranslateResponseDto> TranslateAsync(TranslateRequestDto request, CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var model = await ResolveModelNameAsync();
        await PublishAsync(request.ConnectionId, request.CorrelationId, "preparing_prompt", 5, "Preparing translation prompt...", cancellationToken, model: model);
        var template = await Prompts.GetTemplateAsync("translate-text.prompt.txt", cancellationToken);

        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["sourceLanguage"] = ResolveLanguageName(request.SourceLanguage),
            ["targetLanguage"] = ResolveLanguageName(request.TargetLanguage),
            ["text"] = request.Text
        });

        try
        {
            await PublishAsync(request.ConnectionId, request.CorrelationId, "request_sent", 40, "Sending translation prompt to AI model...", cancellationToken, model: model);
            await PublishAsync(request.ConnectionId, request.CorrelationId, "waiting_response", 65, "Waiting for AI response...", cancellationToken, model: model);
            var responseText = await ExecuteGenerateAsync(prompt, cancellationToken);
            await PublishAsync(request.ConnectionId, request.CorrelationId, "response_received", 75, "AI response received.", cancellationToken, model: model);
            await PublishAsync(request.ConnectionId, request.CorrelationId, "parsing", 80, "Parsing translation response...", cancellationToken, model: model);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return Stamp(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "No response received from translation backend."
                }, model);
            }

            var text = CleanResponseText(responseText);
            var extracted = OllamaResponseHelper.TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extracted))
            {
                var parsed = TryDeserializeTranslateResponse(extracted);
                if (parsed is not null)
                {
                    await PublishAsync(request.ConnectionId, request.CorrelationId, "completed", 100, "Translation completed.", cancellationToken, model: model);
                    return Stamp(parsed, model);
                }
            }

            await PublishAsync(request.ConnectionId, request.CorrelationId, "completed", 100, "Translation completed.", cancellationToken, model: model);
            return Stamp(new TranslateResponseDto { TranslatedText = text.Trim(), Confidence = 0.2 }, model);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // User cancelled (dialog closed / Cancel button): propagate so the caller stops the
            // request instead of reporting an AI failure.
            _logger.LogInformation("Ollama translate cancelled by the caller.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama for translate");
            await PublishAsync(request.ConnectionId, request.CorrelationId, "failed", 100, ex.Message ?? "Translation failed.", cancellationToken, "error", model);
            return Stamp(new TranslateResponseDto
            {
                TranslatedText = string.Empty,
                Confidence = 0,
                ErrorMessage = "Translation service error: " + (ex.Message ?? "Unexpected error")
            }, model);
        }
    }

    /// <summary>
    /// Overwrites the provider/model with the ones this service actually used. Required because
    /// the response DTO is deserialized straight from the model's own JSON output, which must
    /// never be trusted to report which provider answered.
    /// </summary>
    private static TranslateResponseDto Stamp(TranslateResponseDto dto, string? model)
    {
        dto.Provider = ProviderName;
        dto.Model = model;
        return dto;
    }

    private async Task<string?> ResolveModelNameAsync()
    {
        try
        {
            var (_, modelName) = await GetAiSettingsAsync();
            return string.IsNullOrWhiteSpace(modelName) ? null : modelName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OllamaTranslate] Falha ao resolver o nome do modelo local.");
            return null;
        }
    }

    private TranslateResponseDto? TryDeserializeTranslateResponse(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<TranslateResponseDto>(text, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Translation response JSON parse failed. Format={Format}. Length={Length}. Preview={Preview}",
                DescribeResponseFormat(text),
                text.Length,
                CreateSafePreview(text));

            return null;
        }
    }

    private static string CleanResponseText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var cleaned = text.Trim();

        var fenceStart = cleaned.IndexOf("```", StringComparison.Ordinal);
        if (fenceStart >= 0)
        {
            var fenceEnd = cleaned.IndexOf("```", fenceStart + 3, StringComparison.Ordinal);
            if (fenceEnd > fenceStart)
            {
                cleaned = cleaned.Substring(fenceStart + 3, fenceEnd - fenceStart - 3).Trim();

                var firstLineBreak = cleaned.IndexOfAny(new[] { '\r', '\n' });
                if (firstLineBreak >= 0)
                {
                    var firstLine = cleaned[..firstLineBreak].Trim();
                    if (KnownCodeFenceMarkers.Contains(firstLine))
                        cleaned = cleaned[(firstLineBreak + 1)..].Trim();
                }
            }
        }

        cleaned = cleaned.Trim().Trim('`');

        var embeddedJson = OllamaResponseHelper.TryExtractFirstJsonObject(cleaned);
        if (!string.IsNullOrWhiteSpace(embeddedJson))
            return embeddedJson;

        return cleaned.Contains('`', StringComparison.Ordinal)
            ? cleaned.Replace("`", string.Empty, StringComparison.Ordinal).Trim()
            : cleaned;
    }

    private static string DescribeResponseFormat(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "empty";

        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
            return "markdown_fence";

        if (trimmed.StartsWith('`'))
            return "backtick_wrapped_text";

        if (trimmed.StartsWith('{'))
            return "json_object";

        return trimmed.IndexOf('{') >= 0 ? "prose_with_embedded_json" : "plain_text";
    }

    private static string CreateSafePreview(string text, int maxLength = 160)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Trim();

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
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
        string level = "info",
        string? model = null)
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
                Operation = "translate",
                Provider = ProviderName,
                Model = model
            },
            connectionId,
            cancellationToken);
}
