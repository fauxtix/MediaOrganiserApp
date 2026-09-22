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
/// Cloud (OpenRouter) backed generation of book summaries/metadata, mirroring the
/// same prompt contracts used by the local Ollama book-summary services. Returns
/// null whenever the cloud path must be skipped (offline, no API key, HTTP/parse
/// failure) so callers fall back to Ollama. When the resolved model is free the
/// candidates sweep comes from the LIVE OpenRouter free-model catalog, matching
/// the strategy used by <see cref="CloudMediaService"/>.
/// </summary>
public sealed class CloudBookSummaryService
{
    private const string CompletionsUrl = "https://openrouter.ai/api/v1/chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ICloudChatUsageService _usageService;
    private readonly IInternetAvailabilityService _internet;
    private readonly IProgressPublisher _progressPublisher;
    private readonly OpenRouterCatalogService _catalogService;
    private readonly ILogger<CloudBookSummaryService> _logger;

    public CloudBookSummaryService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        IProgressPublisher progressPublisher,
        OpenRouterCatalogService catalogService,
        ILogger<CloudBookSummaryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _prompts = prompts;
        _usageService = usageService;
        _internet = internet;
        _progressPublisher = progressPublisher;
        _catalogService = catalogService;
        _logger = logger;
    }

    /// <summary>
    /// Generates full book metadata + summary via OpenRouter. Returns null when
    /// the cloud path is unavailable/failed so callers fall back to Ollama.
    /// </summary>
    public async Task<AiBookSummaryResponseDto?> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync("book-summary-full.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        return await ExecuteExtractionAsync(
            userPrompt,
            title,
            author,
            connectionId,
            correlationId,
            operation: "book-summary",
            maxTokens: 4096,
            isWeak: dto => IsWeakSummary(dto),
            parse: content => NormalizeSummary(ParseDto<AiBookSummaryResponseDto>(content), title, author),
            cancellationToken);
    }

    /// <summary>
    /// Generates a book summary only (no extra metadata) via OpenRouter. Returns
    /// null when the cloud path is unavailable/failed so callers fall back to Ollama.
    /// </summary>
    public async Task<AiBookSummaryResponseDto?> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync("book-summary-only.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        return await ExecuteExtractionAsync(
            userPrompt,
            title,
            author,
            connectionId,
            correlationId,
            operation: "book-summary",
            maxTokens: 2048,
            isWeak: dto => IsWeakSummary(dto),
            parse: content => NormalizeSummary(ParseDto<AiBookSummaryResponseDto>(content), title, author),
            cancellationToken);
    }

    private static bool IsWeakSummary(AiBookSummaryResponseDto dto)
        => string.IsNullOrWhiteSpace(dto.Summary) || dto.Confidence < 0.5;

    private static AiBookSummaryResponseDto? NormalizeSummary(AiBookSummaryResponseDto? dto, string title, string? author)
    {
        if (dto is null)
            return null;

        var summaryValid = !string.IsNullOrWhiteSpace(dto.Summary)
            && dto.Summary.IndexOf(dto.Author ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
            && dto.Summary.IndexOf(dto.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

        var confidenceLow = dto.Confidence < 0.5;
        dto.Warnings = summaryValid && !confidenceLow
            ? string.Empty
            : $"AI Summary may not be accurate for '{dto.Author ?? author}' - '{dto.Title ?? title}'.";

        return dto;
    }

    /// <summary>
    /// Runs the extraction with the resolved model; when the resolved model is
    /// free and the answer is weak/empty, the curated recommended models are
    /// tried next, strongest first (same strategy as the media cloud services).
    /// </summary>
    private async Task<T?> ExecuteExtractionAsync<T>(
        string userPrompt,
        string title,
        string? author,
        string? connectionId,
        string? correlationId,
        string operation,
        int maxTokens,
        Func<T, bool> isWeak,
        Func<string?, T?> parse,
        CancellationToken cancellationToken) where T : class
    {
        var (ok, apiKey) = await TryBootstrapAsync(cancellationToken);
        if (!ok)
            return null;

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, cancellationToken),
            out var paid) ? paid : false;

        var firstModel = await ResolveModelAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(firstModel))
            firstModel = CloudChatSettingsKeys.DefaultModelValue;
        firstModel = CloudChatSettingsKeys.ApplyPaidModelGate(firstModel, allowPaid);

        var candidates = await BuildCandidateModelsAsync(firstModel, allowPaid, cancellationToken);
        _logger.LogInformation(
            "[CloudBookSummary] A gerar {Operation} via OpenRouter para '{Title}' - '{Author}'; modelos: {Models}",
            operation, title, author, string.Join(", ", candidates));

        T? lastWeak = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var model = candidates[i];
            var percent = Math.Min(10 + i * 60 / candidates.Count, 70);
            await PublishCloudProgressAsync(connectionId, correlationId, "cloud_try", model, percent, operation, cancellationToken);

            var content = await TryRawCompletionAsync(model, apiKey, userPrompt, maxTokens, isJsonOnly: true, cancellationToken);
            if (content is null)
            {
                await PublishCloudProgressAsync(connectionId, correlationId, "cloud_failed", model, percent, operation, cancellationToken);
                continue;
            }

            var attempt = parse(content);
            if (attempt is null)
            {
                await PublishCloudProgressAsync(connectionId, correlationId, "cloud_failed", model, percent, operation, cancellationToken);
                continue;
            }

            if (!isWeak(attempt))
            {
                await PublishCloudProgressAsync(connectionId, correlationId, "cloud_success", $"{model}", 100, operation, cancellationToken);
                return attempt;
            }

            await PublishCloudProgressAsync(connectionId, correlationId, "cloud_empty", model, percent, operation, cancellationToken);
            lastWeak = attempt;
        }

        if (candidates.Count > 1)
        {
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_fallback", string.Empty, 75, operation, cancellationToken);
        }

        _logger.LogWarning(
            "[CloudBookSummary] Sweep terminou sem um resultado forte para '{Title}' - '{Author}' após {Count} modelos. A usar fallback local (Ollama).",
            title, author, candidates.Count);

        return lastWeak;
    }

    private async Task<(bool Ok, string ApiKey)> TryBootstrapAsync(CancellationToken ct)
    {
        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogWarning("[CloudBookSummary] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return (false, string.Empty);
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudBookSummary] Chave OpenRouter não configurada. A usar o fallback local (Ollama).");
            return (false, string.Empty);
        }

        return (true, apiKey);
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

    private async Task<string?> TryRawCompletionAsync(
        string model,
        string apiKey,
        string userPrompt,
        int maxTokens,
        bool isJsonOnly,
        CancellationToken ct)
    {
        if (CloudChatSettingsKeys.IsFreeModel(model))
        {
            if (await _usageService.GetFreeRequestsRemainingAsync(ct) <= 0)
            {
                _logger.LogWarning("[CloudBookSummary] Teto diário de modelos gratuitos do OpenRouter atingido; a usar fallback local (Ollama).");
                return null;
            }

            try { await _usageService.RegisterFreeModelUsageAsync(ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CloudBookSummary] Falha a registar requisição gratuita."); }
        }

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = isJsonOnly
                    ? "You return only valid JSON. Do not add prose or markdown fences."
                    : "You return only a plain string. Do not add prose, explanation, markdown or code fences." },
                new { role = "user", content = userPrompt }
            },
            temperature = 0,
            max_tokens = maxTokens
        };

        var client = _httpClientFactory.CreateClient("OpenRouterChat");
        using var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer-BookSummary");
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
            _logger.LogWarning(ex, "[CloudBookSummary] Erro ao contactar OpenRouter com o modelo {Model}.", model);
            return null;
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                _logger.LogWarning("[CloudBookSummary] 429 Too Many Requests do OpenRouter ({Model}).", model);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudBookSummary] HTTP {Status} ({Model}): {Body}",
                    (int)response.StatusCode, model, errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return null;
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[CloudBookSummary] Resposta sem conteúdo message.content ({Model}).", model);
                return null;
            }

            return StripFences(content);
        }
    }

    private static T? ParseDto<T>(string? content) where T : class
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch
        {
            var embeddedJson = OllamaResponseHelper.TryExtractFirstJsonObject(content);
            if (string.IsNullOrWhiteSpace(embeddedJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(embeddedJson, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }

    private Task PublishCloudProgressAsync(
        string? connectionId, string? correlationId, string stage,
        string message, int? percent, string operation, CancellationToken ct)
    {
        try
        {
            return _progressPublisher.PublishAsync(
                ProgressChannel.BooksInfo,
                new ProgressUpdateDto
                {
                    CorrelationId = correlationId,
                    Stage = stage,
                    Percent = percent,
                    Message = message,
                    Level = "info",
                    Service = nameof(CloudBookSummaryService),
                    Operation = operation
                },
                connectionId,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudBookSummary] Falha a publicar progresso SignalR.");
            return Task.CompletedTask;
        }
    }

    private async Task SyncFreeUsageFromRateLimitHeadersAsync(HttpResponseMessage response, bool isFreeModel, CancellationToken ct)
    {
        if (!isFreeModel) return;
        var limitFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Limit", out var limit)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Limit", out limit);
        var remainingFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", out var remaining)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Remaining", out remaining);
        if (!limitFound || !remainingFound) return;
        try { await _usageService.SyncFreeUsageFromHeadersAsync(limit, remaining, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "[CloudBookSummary] Falha ao sincronizar contadores 429."); }
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

    private static string? StripFences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = text.Trim();
        const string fence = "```";
        var firstFence = cleaned.IndexOf(fence, StringComparison.Ordinal);
        var lastFence = cleaned.LastIndexOf(fence, StringComparison.Ordinal);
        if (firstFence >= 0 && lastFence > firstFence)
            cleaned = cleaned.Substring(firstFence + fence.Length, lastFence - (firstFence + fence.Length)).TrimStart();
        while (cleaned.Length > 0 && (cleaned[0] == '`' || cleaned[0] == '\''))
            cleaned = cleaned[1..].TrimStart();
        return cleaned;
    }
}