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
/// OpenRouter-backed author profile using the same prompt contract as the local
/// Ollama/OpenAI-SDK author-profile service. Returns null whenever the cloud path
/// must be skipped (offline, no API key, HTTP/parse failure) or no usable bio was
/// produced, so callers fall back to the local provider.
/// </summary>
public sealed class CloudAuthorProfileService
{
    private const string CompletionsUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string PromptTemplate = "author-profile.prompt.txt";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ICloudChatUsageService _usageService;
    private readonly IInternetAvailabilityService _internet;
    private readonly OpenRouterCatalogService _catalogService;
    private readonly ILogger<CloudAuthorProfileService> _logger;

    public CloudAuthorProfileService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        OpenRouterCatalogService catalogService,
        ILogger<CloudAuthorProfileService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _prompts = prompts;
        _usageService = usageService;
        _internet = internet;
        _catalogService = catalogService;
        _logger = logger;
    }

    /// <summary>
    /// Generates an author profile via OpenRouter. Returns null when the cloud
    /// path is unavailable/failed so callers fall back to the local provider.
    /// </summary>
    public async Task<AiAuthorProfileResponseDto?> GetAuthorProfileAsync(
        string name,
        string language,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        if (!await _internet.IsReachableAsync(cancellationToken))
        {
            _logger.LogWarning("[CloudAuthorProfile] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return null;
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudAuthorProfile] Chave OpenRouter não configurada. A usar o fallback local.");
            return null;
        }

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, cancellationToken),
            out var paid) ? paid : false;

        var model = await ResolveModelAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(model))
            model = CloudChatSettingsKeys.DefaultModelValue;
        model = CloudChatSettingsKeys.ApplyPaidModelGate(model, allowPaid);

        var template = await _prompts.GetTemplateAsync(PromptTemplate, cancellationToken);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["name"] = name,
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
        });

        var candidates = await BuildCandidateModelsAsync(model, allowPaid, cancellationToken);
        for (var i = 0; i < candidates.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var content = await TryRawCompletionAsync(candidates[i], apiKey, userPrompt, maxTokens: 2048, cancellationToken);
            if (content is null)
                continue;

            var parsed = await AuthorProfileJsonRepairer.ParseOrRepairAsync(
                content,
                repairWithModelAsync: null,
                _logger,
                string.IsNullOrWhiteSpace(language) ? "pt" : language);

            // A profile only counts as usable when the model actually returned a bio;
            // otherwise treat this candidate as a poor result and keep sweeping.
            if (parsed is not null && !string.IsNullOrWhiteSpace(parsed.Bio))
                return parsed;
        }

        _logger.LogWarning("[CloudAuthorProfile] OpenRouter não produziu perfil para '{Name}'; a usar fallback local.", name);
        return null;
    }

    private async Task<string?> TryRawCompletionAsync(
        string model, string apiKey, string userPrompt, int maxTokens, CancellationToken ct)
    {
        if (CloudChatSettingsKeys.IsFreeModel(model))
        {
            if (await _usageService.GetFreeRequestsRemainingAsync(ct) <= 0)
            {
                _logger.LogWarning("[CloudAuthorProfile] Teto diário de modelos gratuitos do OpenRouter atingido; a usar fallback local.");
                return null;
            }

            try { await _usageService.RegisterFreeModelUsageAsync(ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "[CloudAuthorProfile] Falha a registar requisição gratuita."); }
        }

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = "You return only valid JSON. Do not add prose or markdown fences." },
                new { role = "user", content = userPrompt }
            },
            temperature = 0,
            max_tokens = maxTokens
        };

        var client = _httpClientFactory.CreateClient("OpenRouterChat");
        using var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer-AuthorProfile");
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
            _logger.LogWarning(ex, "[CloudAuthorProfile] Erro ao contactar OpenRouter com o modelo {Model}.", model);
            return null;
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                _logger.LogWarning("[CloudAuthorProfile] 429 Too Many Requests do OpenRouter ({Model}).", model);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudAuthorProfile] HTTP {Status} ({Model}): {Body}",
                    (int)response.StatusCode, model,
                    errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return null;
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("[CloudAuthorProfile] Resposta sem conteúdo message.content ({Model}).", model);
                return null;
            }

            return content;
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
    /// When the resolved model is free, a profile without a usable bio is treated
    /// as a poor result and the sweep continues. Paid models are only swept when
    /// paid models are enabled; otherwise the candidates come from the LIVE
    /// OpenRouter catalog of actual free models (recommended ones first), never
    /// from fabricated ":free" names that may not exist. Non-free/recommended
    /// models run alone.
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

    private async Task SyncFreeUsageFromRateLimitHeadersAsync(HttpResponseMessage response, bool isFreeModel, CancellationToken ct)
    {
        if (!isFreeModel) return;
        var limitFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Limit", out var limit)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Limit", out limit);
        var remainingFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", out var remaining)
            || TryGetHeaderValue(response.Content.Headers, "X-RateLimit-Remaining", out remaining);
        if (!limitFound || !remainingFound) return;
        try { await _usageService.SyncFreeUsageFromHeadersAsync(limit, remaining, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "[CloudAuthorProfile] Falha ao sincronizar contadores 429."); }
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
}