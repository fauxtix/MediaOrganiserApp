using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// Cloud (OpenRouter) backed generation of media notes, media metadata and
/// media edition year, mirroring the same prompt contracts used by the local
/// Ollama services. Returns null whenever the cloud path must be skipped
/// (offline, no API key, HTTP/parse failure) so callers fall back to Ollama.
/// </summary>
public sealed class CloudMediaService
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
    private readonly ILogger<CloudMediaService> _logger;

    public CloudMediaService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        IProgressPublisher progressPublisher,
        OpenRouterCatalogService catalogService,
        ILogger<CloudMediaService> logger)
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
    /// Generates media notes via OpenRouter. Returns null when the cloud path
    /// is unavailable/failed so callers fall back to the local Ollama provider.
    /// </summary>
    public async Task<AiMediaNotesResponseDto?> GetMediaNotesAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync("media-notes.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language,
            ["artist"] = artist,
            ["title"] = title
        });

        return await ExecuteExtractionAsync(
            userPrompt,
            connectionId,
            correlationId,
            operation: "media-notes",
            maxTokens: 2048,
            isWeak: dto => string.IsNullOrWhiteSpace(dto.Notes) || (dto.Confidence ?? 0) < 0.5,
            parse: content => ParseDto<AiMediaNotesResponseDto>(content),
            cancellationToken);
    }

    /// <summary>
    /// Generates full media metadata via OpenRouter. Returns null when the
    /// cloud path is unavailable/failed so callers fall back to Ollama.
    /// </summary>
    public async Task<AiMediaMetadataResponseDto?> GetMediaMetadataAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync("media-metadata.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language,
            ["artist"] = artist,
            ["title"] = title
        });

        return await ExecuteExtractionAsync(
            userPrompt,
            connectionId,
            correlationId,
            operation: "media-metadata",
            maxTokens: 4096,
            isWeak: dto => (dto.Confidence ?? 0) < 0.5,
            parse: content => ParseDto<AiMediaMetadataResponseDto>(content),
            cancellationToken);
    }

    /// <summary>
    /// Resolves the media edition year via OpenRouter. Returns null when the
    /// cloud path is unavailable; returns "N/A" when the model cannot find a year.
    /// </summary>
    public async Task<string?> GetMediaEditionYearAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var additionalInfo = $"Release year not found in external databases for '{title}' by '{artist}'. Only return the year if certain.";
        var template = await _prompts.GetTemplateAsync("media-edition-year.prompt.txt", cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language,
            ["artist"] = artist,
            ["title"] = title,
            ["additionalInfo"] = additionalInfo
        });

        var year = await ExecuteExtractionRawAsync(
            userPrompt,
            connectionId,
            correlationId,
            operation: "media-edition-year",
            maxTokens: 32,
            isWeak: value => string.IsNullOrWhiteSpace(value),
            normalize: NormalizeYear,
            cancellationToken);
        if (year is null)
        {
            _logger.LogWarning("[CloudMedia] OpenRouter falhou a resolver o ano de edição de '{Title}' - '{Artist}'. A usar fallback Ollama.", title, artist);
            return null;
        }

        return year;
    }

    /// <summary>
    /// Runs the extraction with the resolved model; when the resolved model is
    /// free and the answer is weak/empty, the curated recommended models are
    /// tried next, strongest first (same strategy as the author-books cloud).
    /// </summary>
    private async Task<T?> ExecuteExtractionAsync<T>(
        string userPrompt,
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

        return lastWeak;
    }

    /// <summary>
    /// Runs a plain-string (non-JSON) extraction with the resolved model,
    /// mirroring <see cref="ExecuteExtractionAsync{T}"/>: when the first
    /// candidate returns a null/weak answer the sweep continues to the next
    /// model, strongest first, honouring the <c>AllowPaidModels</c> toggle.
    /// Used for single-value extractions such as the media edition year.
    /// </summary>
    private async Task<string?> ExecuteExtractionRawAsync(
        string userPrompt,
        string? connectionId,
        string? correlationId,
        string operation,
        int maxTokens,
        Func<string?, bool> isWeak,
        Func<string?, string?> normalize,
        CancellationToken cancellationToken)
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
        string? lastWeak = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var model = candidates[i];
            var percent = Math.Min(10 + i * 60 / candidates.Count, 70);
            await PublishCloudProgressAsync(connectionId, correlationId, "cloud_try", model, percent, operation, cancellationToken);

            var raw = await TryRawCompletionAsync(model, apiKey, userPrompt, maxTokens, isJsonOnly: false, cancellationToken);
            if (raw is null)
            {
                await PublishCloudProgressAsync(connectionId, correlationId, "cloud_failed", model, percent, operation, cancellationToken);
                continue;
            }

            var attempt = normalize(raw);
            if (attempt is null || isWeak(attempt))
            {
                await PublishCloudProgressAsync(connectionId, correlationId, "cloud_empty", model, percent, operation, cancellationToken);
                lastWeak = attempt;
                continue;
            }

            await PublishCloudProgressAsync(connectionId, correlationId, "cloud_success", $"{model}|{attempt}", 100, operation, cancellationToken);
            return attempt;
        }

        if (candidates.Count > 1)
        {
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_fallback", string.Empty, 75, operation, cancellationToken);
        }

        return lastWeak;
    }

    private async Task<(bool Ok, string ApiKey)> TryBootstrapAsync(CancellationToken ct)
    {
        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogWarning("[CloudMedia] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return (false, string.Empty);
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudMedia] Chave OpenRouter não configurada. A usar o fallback local (Ollama).");
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
            try { await _usageService.RegisterFreeModelUsageAsync(ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CloudMedia] Falha a registar requisição gratuita."); }
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
        request.Headers.Add("X-Title", "MediaOrganizer-MediaInfo");
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
            _logger.LogWarning(ex, "[CloudMedia] Erro ao contactar OpenRouter com o modelo {Model}.", model);
            return null;
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                _logger.LogWarning("[CloudMedia] 429 Too Many Requests do OpenRouter ({Model}).", model);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudMedia] HTTP {Status} ({Model}): {Body}",
                    (int)response.StatusCode, model, errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return null;
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[CloudMedia] Resposta sem conteúdo message.content ({Model}).", model);
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

    private static string? NormalizeYear(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var cleaned = raw.Trim().Trim('"');
        var lines = cleaned.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Any(l => l.Contains("N/A", StringComparison.OrdinalIgnoreCase)))
            return "N/A";

        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"\b(\d{4})\b");
            if (match.Success)
                return match.Value;
        }

        return null;
    }

    private Task PublishCloudProgressAsync(
        string? connectionId, string? correlationId, string stage,
        string message, int? percent, string operation, CancellationToken ct)
    {
        try
        {
            return _progressPublisher.PublishAsync(
                ProgressChannel.MediaInfo,
                new ProgressUpdateDto
                {
                    CorrelationId = correlationId,
                    Stage = stage,
                    Percent = percent,
                    Message = message,
                    Level = "info",
                    Service = nameof(CloudMediaService),
                    Operation = operation
                },
                connectionId,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudMedia] Falha a publicar progresso SignalR.");
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
        catch (Exception ex) { _logger.LogWarning(ex, "[CloudMedia] Falha ao sincronizar contadores 429."); }
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