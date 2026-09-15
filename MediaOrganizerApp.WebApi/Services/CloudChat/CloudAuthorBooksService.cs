using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// Fetches an author's bibliography from OpenRouter (structured JSON), as the
/// cloud step of the author-books chain (OpenRouter -> Ollama). Returns null
/// when the cloud path is unavailable/failed so callers fall back to Ollama.
/// </summary>
public sealed class CloudAuthorBooksService
{
    private const string CompletionsUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string PromptTemplate = "author-works.prompt.txt";
    private static readonly JsonSerializerOptions SnakeCaseJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ICloudChatUsageService _usageService;
    private readonly IInternetAvailabilityService _internet;
    private readonly IProgressPublisher _progressPublisher;
    private readonly ILogger<CloudAuthorBooksService> _logger;

    public CloudAuthorBooksService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        IProgressPublisher progressPublisher,
        ILogger<CloudAuthorBooksService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _prompts = prompts;
        _usageService = usageService;
        _internet = internet;
        _progressPublisher = progressPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Returns the author's books from OpenRouter, or null when the cloud path
    /// must be skipped (missing key, toggle off, network error, parse failure).
    /// When the resolved model is free and the author-less result comes back
    /// empty, the curated recommended models are tried in order before giving up.
    /// </summary>
    public async Task<List<GoogleBookInfo>?> GetAuthorBooksAsync(
        string author,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new List<GoogleBookInfo>();

        var useCloud = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AuthorBooksUseCloud, null, ct),
            out var v) ? v : true;
        if (!useCloud)
        {
            _logger.LogDebug("[CloudAuthorBooks] Cloud desativado pelo toggle.");
            return null;
        }

        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogWarning("[CloudAuthorBooks] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return null;
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudAuthorBooks] Chave OpenRouter não configurada.");
            return null;
        }

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, ct),
            out var paid) ? paid : false;

        var firstModel = await ResolveModelAsync(ct);
        if (string.IsNullOrWhiteSpace(firstModel))
            firstModel = CloudChatSettingsKeys.DefaultModelValue;

        var template = await _prompts.GetTemplateAsync(PromptTemplate, ct);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["author"] = author,
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
        });

        var candidates = BuildCandidateModels(firstModel, allowPaid);
        List<GoogleBookInfo>? lastEmpty = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var model = candidates[i];
            var percent = Math.Min(10 + i * 60 / candidates.Count, 70);
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_try", model, percent, ct);

            var attempt = await TryGetAuthorBooksWithModelAsync(model, apiKey, userPrompt, ct);
            if (attempt is null)
            {
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_failed", model, percent, ct);
                continue;
            }

            if (attempt.Count > 0)
            {
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_success", $"{model}|{attempt.Count}", 100, ct);
                return attempt;
            }

            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_empty", model, percent, ct);
            lastEmpty = attempt;
        }

        if (candidates.Count > 1)
        {
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_fallback", string.Empty, 75, ct);
        }

        return lastEmpty;
    }

    /// <summary>
    /// When the resolved model is free (e.g. openrouter/free), an empty author
    /// result is treated as a weak answer and the curated recommended models are
    /// tried next, strongest first. Paid models are only swept when paid models
    /// are enabled; otherwise the corresponding ":free" variants are used so the
    /// sweep never spends credits. Non-free/recommended models run alone.
    /// </summary>
    private static List<string> BuildCandidateModels(string first, bool allowPaid)
    {
        if (!CloudChatSettingsKeys.IsFreeModel(first))
            return new List<string> { first };

        var models = new List<string> { first };
        foreach (var prefix in RecommendedExtractionModels.Prefixes)
        {
            var candidate = allowPaid ? prefix : prefix + ":free";
            if (models.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                continue;
            models.Add(candidate);
        }
        return models;
    }

    private async Task PublishCloudProgressAsync(
        string? connectionId, string? correlationId,
        string stage, string message, int? percent, CancellationToken ct)
    {
        try
        {
            await _progressPublisher.PublishAsync(
                ProgressChannel.BooksInfo,
                new ProgressUpdateDto
                {
                    Stage = stage,
                    Percent = percent,
                    Message = message,
                    Level = "info",
                    Service = nameof(CloudAuthorBooksService),
                    Operation = "GetAuthorBooksAsync"
                },
                connectionId,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudAuthorBooks] Falha a publicar progresso SignalR.");
        }
    }

    private async Task<List<GoogleBookInfo>?> TryGetAuthorBooksWithModelAsync(
        string model, string apiKey, string userPrompt, CancellationToken ct)
    {
        if (CloudChatSettingsKeys.IsFreeModel(model))
        {
            try { await _usageService.RegisterFreeModelUsageAsync(ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CloudAuthorBooks] Falha a registar requisição gratuita."); }
        }

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = "You return only valid JSON. Do not add prose or markdown fences." },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.2,
            max_tokens = 4096
        };

        var client = _httpClientFactory.CreateClient("OpenRouterCatalog");
        using var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer-AuthorBooks");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, SnakeCaseJson), Encoding.UTF8, "application/json");

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
            _logger.LogWarning(ex, "[CloudAuthorBooks] Erro ao contactar OpenRouter com o modelo {Model}.", model);
            return null;
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                _logger.LogWarning("[CloudAuthorBooks] 429 Too Many Requests do OpenRouter ({Model}).", model);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudAuthorBooks] HTTP {Status} ({Model}): {Body}",
                    (int)response.StatusCode, model, errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return null;
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[CloudAuthorBooks] Resposta sem conteúdo message.content ({Model}).", model);
                return null;
            }

            return ParseBooksJson(StripFences(content));
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

    private async Task SyncFreeUsageFromRateLimitHeadersAsync(HttpResponseMessage response, bool isFreeModel, CancellationToken ct)
    {
        if (!isFreeModel) return;
        var limitFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Limit", out var limit)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Limit", out limit);
        var remainingFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", out var remaining)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Remaining", out remaining);
        if (!limitFound || !remainingFound) return;
        try { await _usageService.SyncFreeUsageFromHeadersAsync(limit, remaining, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "[CloudAuthorBooks] Falha ao sincronizar contadores 429."); }
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

    private static List<GoogleBookInfo>? ParseBooksJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                var start = raw.IndexOf('[');
                var end = raw.LastIndexOf(']');
                if (start < 0 || end <= start) return null;
                var arr = raw.Substring(start, end - start + 1);
                using var doc2 = JsonDocument.Parse(arr);
                if (doc2.RootElement.ValueKind != JsonValueKind.Array) return null;
                return MapArray(doc2.RootElement);
            }
            return MapArray(doc.RootElement);
        }
        catch { return null; }
    }

    private static List<GoogleBookInfo> MapArray(JsonElement arr)
    {
        var result = new List<GoogleBookInfo>();
        foreach (var item in arr.EnumerateArray())
        {
            var title = item.TryGetProperty("title", out var t) ? t.GetString() : null;
            if (string.IsNullOrWhiteSpace(title)) continue;
            var year = item.TryGetProperty("year", out var y) ? y.GetString() : null;
            var thumbnail = item.TryGetProperty("thumbnail", out var th) ? th.GetString() : null;
            result.Add(new GoogleBookInfo
            {
                Title = title,
                PublishedDate = year,
                Thumbnail = string.IsNullOrWhiteSpace(thumbnail) ? "/images/no-image-available.png" : thumbnail,
                Confidence = 0.7,
                Synthesized = true
            });
        }
        return result;
    }
}
