using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// OpenRouter-backed translation using the same prompt contract as the local
/// Ollama translation service. Returns null whenever the cloud path must be
/// skipped (offline, no API key, HTTP/parse failure) so callers fall back to
/// the local Ollama provider.
/// </summary>
public sealed class CloudTranslateService
{
    private const string CompletionsUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string ProviderName = "openrouter";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ICloudChatUsageService _usageService;
    private readonly IInternetAvailabilityService _internet;
    private readonly OpenRouterCatalogService _catalogService;
    private readonly IProgressPublisher _progressPublisher;
    private readonly ILogger<CloudTranslateService> _logger;

    public CloudTranslateService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        OpenRouterCatalogService catalogService,
        IProgressPublisher progressPublisher,
        ILogger<CloudTranslateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _prompts = prompts;
        _usageService = usageService;
        _internet = internet;
        _catalogService = catalogService;
        _progressPublisher = progressPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Translates the given text via OpenRouter. Returns null when the cloud
    /// path is unavailable/failed so callers fall back to the local Ollama.
    /// </summary>
    public async Task<TranslateResponseDto?> TranslateAsync(
        TranslateRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        if (!await _internet.IsReachableAsync(cancellationToken))
        {
            _logger.LogWarning("[CloudTranslate] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return null;
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudTranslate] Chave OpenRouter não configurada. A usar o fallback local (Ollama).");
            return null;
        }

        var model = await ResolveModelAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(model))
            model = CloudChatSettingsKeys.DefaultModelValue;

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, cancellationToken),
            out var paid) ? paid : false;
        model = CloudChatSettingsKeys.ApplyPaidModelGate(model, allowPaid);

        var template = await _prompts.GetTemplateAsync("translate-text.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["sourceLanguage"] = ResolveLanguageName(request.SourceLanguage),
            ["targetLanguage"] = ResolveLanguageName(request.TargetLanguage),
            ["text"] = request.Text
        });

        var candidates = await BuildCandidateModelsAsync(model, allowPaid, cancellationToken);
        TranslateResponseDto? lastWeak = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var candidate = candidates[i];
            await PublishCloudProgressAsync(
                request, "cloud_try", candidate, 40 + (i * 5), cancellationToken);

            var (content, servedModel) = await TryRawCompletionAsync(
                candidate, apiKey, userPrompt, maxTokens: 1024, cancellationToken);
            if (content is null)
            {
                await PublishCloudProgressAsync(
                    request, "cloud_failed", candidate, 40 + (i * 5), cancellationToken);
                continue;
            }

            // The response body reports the model that ACTUALLY served the request, which
            // differs from the requested one when the openrouter/free router is in play.
            var effectiveModel = string.IsNullOrWhiteSpace(servedModel) ? candidate : servedModel;
            var attempt = Stamp(ParseTranslateResponse(content), effectiveModel);

            if (!string.IsNullOrWhiteSpace(attempt.TranslatedText)
                && attempt.Confidence >= 0.5)
            {
                await PublishCloudProgressAsync(
                    request, "cloud_success", effectiveModel, 100, cancellationToken);
                return attempt;
            }

            await PublishCloudProgressAsync(
                request, "cloud_empty", effectiveModel, 40 + (i * 5), cancellationToken);
            lastWeak = attempt;
        }

        if (lastWeak is not null)
            return lastWeak;

        _logger.LogWarning("[CloudTranslate] OpenRouter falhou a tradução; a usar fallback local (Ollama).");
        return null;
    }

    private static TranslateResponseDto Stamp(TranslateResponseDto? dto, string model)
    {
        var result = dto ?? new TranslateResponseDto();
        result.Provider = ProviderName;
        result.Model = model;
        return result;
    }

    /// <summary>
    /// Publishes the model in flight on the progress channel so a cancelled translation can
    /// still report which model was being attempted (a cancellation never returns a body).
    /// Never throws: progress is diagnostics, not part of the translation contract.
    /// </summary>
    private async Task PublishCloudProgressAsync(
        TranslateRequestDto request, string stage, string model, int percent, CancellationToken ct)
    {
        try
        {
            await _progressPublisher.PublishAsync(
                ProgressChannel.BooksInfo,
                new ProgressUpdateDto
                {
                    CorrelationId = request.CorrelationId,
                    Stage = stage,
                    Percent = percent,
                    Message = model,
                    Level = "info",
                    Service = nameof(CloudTranslateService),
                    Operation = "TranslateAsync",
                    Provider = ProviderName,
                    Model = model
                },
                request.ConnectionId,
                ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[CloudTranslate] Falha ao publicar progresso {Stage}.", stage);
        }
    }

    private TranslateResponseDto? ParseTranslateResponse(string content)
    {
        var cleaned = CleanResponseText(content);
        var extracted = OllamaResponseHelper.TryExtractFirstJsonObject(cleaned);
        if (!string.IsNullOrWhiteSpace(extracted))
        {
            var parsed = TryDeserializeTranslateResponse(extracted);
            if (parsed is not null)
                return parsed;
        }

        return new TranslateResponseDto { TranslatedText = cleaned.Trim(), Confidence = 0.2 };
    }

    /// <summary>
    /// Runs one raw completion and returns the cleaned content together with the model
    /// reported by the response body (the one that actually served the request, which for
    /// the <c>openrouter/free</c> router is never the requested id). Content is null when the
    /// attempt failed, in which case the caller should try the next candidate.
    /// </summary>
    private async Task<(string? Content, string? ServedModel)> TryRawCompletionAsync(
        string model, string apiKey, string userPrompt, int maxTokens, CancellationToken ct)
    {
        if (CloudChatSettingsKeys.IsFreeModel(model))
        {
            if (await _usageService.GetFreeRequestsRemainingAsync(ct) <= 0)
            {
                _logger.LogWarning("[CloudTranslate] Teto diário de modelos gratuitos do OpenRouter atingido; a usar fallback local (Ollama).");
                return (null, null);
            }

            try { await _usageService.RegisterFreeModelUsageAsync(ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CloudTranslate] Falha a registar requisição gratuita."); }
        }

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = "You are a professional translator. Return only the translation, without explanations or code fences." },
                new { role = "user", content = userPrompt }
            },
            temperature = 0,
            max_tokens = maxTokens
        };

        var client = _httpClientFactory.CreateClient("OpenRouterChat");
        using var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer-Translate");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudTranslate] Erro ao contactar OpenRouter com o modelo {Model}.", model);
            return (null, null);
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                _logger.LogWarning("[CloudTranslate] 429 Too Many Requests do OpenRouter ({Model}).", model);
                return (null, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudTranslate] HTTP {Status} ({Model}): {Body}",
                    (int)response.StatusCode, model, errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return (null, null);
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[CloudTranslate] Resposta sem conteúdo message.content ({Model}).", model);
                return (null, null);
            }

            return (CleanResponseText(content), ExtractServedModel(fullBody));
        }
    }

    private async Task<string?> ResolveModelAsync(CancellationToken ct)
    {
        var overrideModels = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.ExtractionModels, null, ct);
        if (!string.IsNullOrWhiteSpace(overrideModels))
        {
            var first = overrideModels
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first)) return first;
        }

        var defaultModel = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.DefaultModel, null, ct);
        if (!string.IsNullOrWhiteSpace(defaultModel) && RecommendedExtractionModels.IsRecommended(defaultModel))
            return defaultModel;

        return CloudChatSettingsKeys.DefaultModelValue;
    }

    /// <summary>
    /// When the resolved model is free, a weak/empty answer is treated as a poor
    /// result and the sweep continues. Paid models are only swept when paid models
    /// are enabled; otherwise the candidates come from the LIVE OpenRouter catalog
    /// of actual free models (recommended ones first), never from fabricated
    /// ":free" names that may not exist. Non-free/recommended models run alone.
    /// </summary>
    private async Task<List<string>> BuildCandidateModelsAsync(string first, bool allowPaid, CancellationToken ct)
    {
        if (!CloudChatSettingsKeys.IsFreeModel(first))
            return new List<string> { first };

        var models = new List<string> { first };
        if (allowPaid)
        {
            foreach (var prefix in RecommendedExtractionModels.Prefixes)
                AddUniqueModel(models, prefix);
        }
        else
        {
            var catalog = await _catalogService.GetFreeModelsAsync(ct);
            var recommended = new List<string>();
            var others = new List<string>();
            foreach (var model in catalog)
            {
                if (string.IsNullOrWhiteSpace(model.Id)
                    || models.Contains(model.Id, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (RecommendedExtractionModels.IsRecommended(model.Id))
                    recommended.Add(model.Id);
                else
                    others.Add(model.Id);
            }

            models.AddRange(recommended);
            var remaining = Math.Max(0, 12 - models.Count);
            models.AddRange(others.Take(remaining));
        }
        return models;
    }

    private static void AddUniqueModel(List<string> models, string candidate)
    {
        if (!models.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            models.Add(candidate);
    }

    private TranslateResponseDto? TryDeserializeTranslateResponse(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<TranslateResponseDto>(text, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string CleanResponseText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var cleaned = text.Trim();
        const string fence = "```";
        var firstFence = cleaned.IndexOf(fence, StringComparison.Ordinal);
        var lastFence = cleaned.LastIndexOf(fence, StringComparison.Ordinal);
        if (firstFence >= 0 && lastFence > firstFence)
            cleaned = cleaned.Substring(firstFence + fence.Length, lastFence - (firstFence + fence.Length)).Trim();

        while (cleaned.Length > 0 && cleaned[0] == '`')
            cleaned = cleaned[1..].TrimStart();

        return cleaned;
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

    private async Task SyncFreeUsageFromRateLimitHeadersAsync(HttpResponseMessage response, bool isFreeModel, CancellationToken ct)
    {
        if (!isFreeModel) return;
        var limitFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Limit", out var limit)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Limit", out limit);
        var remainingFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", out var remaining)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Remaining", out remaining);
        if (!limitFound || !remainingFound) return;
        try { await _usageService.SyncFreeUsageFromHeadersAsync(limit, remaining, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "[CloudTranslate] Falha ao sincronizar contadores 429."); }
    }

    private static bool TryGetHeaderValue(System.Net.Http.Headers.HttpHeaders headers, string name, out int value)
    {
        value = 0;
        if (!headers.TryGetValues(name, out var vals)) return false;
        var raw = vals.FirstOrDefault();
        return int.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static string? ExtractMessageContent(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                    return content.GetString();
            }
        }
        catch { /* fall through */ }
        return null;
    }

    /// <summary>
    /// Reads the model the provider actually served from the response body. OpenRouter echoes
    /// the upstream model there, which is the only honest way to report which model answered
    /// when the request went through the <c>openrouter/free</c> router.
    /// </summary>
    private static string? ExtractServedModel(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("model", out var model)
                && model.ValueKind == JsonValueKind.String)
            {
                var value = model.GetString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
        catch { /* fall through */ }
        return null;
    }
}