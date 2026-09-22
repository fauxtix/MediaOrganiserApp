using System.Diagnostics;
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
    private readonly OpenRouterCatalogService _catalogService;
    private readonly ILogger<CloudAuthorBooksService> _logger;

    public CloudAuthorBooksService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IPromptTemplateProvider prompts,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        IProgressPublisher progressPublisher,
        OpenRouterCatalogService catalogService,
        ILogger<CloudAuthorBooksService> logger)
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
    /// Returns the author's books from OpenRouter, or null when the cloud path
    /// must be skipped (missing key, toggle off, network error, parse failure).
    /// When the resolved model is free and the author-less result comes back
    /// empty, the curated recommended models are tried in order before giving up.
    /// Kept for callers/tests that only need the book list; prefer
    /// <see cref="GetAuthorBooksResultAsync"/> to surface the failure reason.
    /// </summary>
    public async Task<List<GoogleBookInfo>?> GetAuthorBooksAsync(
        string author,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        var result = await GetAuthorBooksResultAsync(author, language, connectionId, correlationId, ct);
        return result.Books;
    }

    /// <summary>
    /// OpenRouter step with full outcome so the UI can explain why no cloud
    /// results were returned and decide whether to continue to the local model.
    /// </summary>
    public async Task<CloudAuthorBooksResult> GetAuthorBooksResultAsync(
        string author,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(author))
            return new CloudAuthorBooksResult { Books = new List<GoogleBookInfo>() };

        var useCloud = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AuthorBooksUseCloud, null, ct),
            out var v) ? v : true;
        if (!useCloud)
        {
            _logger.LogDebug("[CloudAuthorBooks] Cloud desativado pelo toggle.");
            return new CloudAuthorBooksResult { FailureReason = "Pesquisa OpenRouter desativada (toggle admin)." };
        }

        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogWarning("[CloudAuthorBooks] Sem acesso à internet; a saltar OpenRouter e a usar o fallback local (Ollama).");
            return new CloudAuthorBooksResult { FailureReason = "Sem acesso à internet para o OpenRouter." };
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogDebug("[CloudAuthorBooks] Chave OpenRouter não configurada.");
            return new CloudAuthorBooksResult { FailureReason = "Chave da API OpenRouter não configurada." };
        }

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, ct),
            out var paid) ? paid : false;

        var firstModel = await ResolveModelAsync(ct);
        if (string.IsNullOrWhiteSpace(firstModel))
            firstModel = CloudChatSettingsKeys.DefaultModelValue;
        firstModel = CloudChatSettingsKeys.ApplyPaidModelGate(firstModel, allowPaid);

        var template = await _prompts.GetTemplateAsync(PromptTemplate, ct);
        var userPrompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["author"] = author,
            ["language"] = string.IsNullOrWhiteSpace(language) ? "pt" : language
        });

        var candidates = await BuildCandidateModelsAsync(firstModel, allowPaid, ct);
        _logger.LogInformation(
            "[CloudAuthorBooks] A pesquisar bibliografia via OpenRouter para {Author}; modelos: {Models}",
            author, string.Join(", ", candidates));
        var outcomes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        List<GoogleBookInfo>? lastEmpty = null;

        // Phase 1: sweep the KNOWN-GOOD free models (GLM/Gemma/Nemotron). A weak
        // first hit (1-2 books) does NOT stop the sweep — we keep the richest
        // answer and keep trying; 4+ books is treated as "good enough" to stop.
        // Phase 2: fallback (router / recommended / catalog) — wait, preferred is
        // already phase 1; this handles the remaining candidates first-non-empty.
        const int goodEnoughCount = 4;
        List<GoogleBookInfo>? bestBooks = null;
        string? bestModel = null;

        for (var i = 0; i < candidates.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var model = candidates[i];
            var isKnownGood = RecommendedExtractionModels.IsRecommendedFree(model);

            // A preferred model already produced books; the router/catalog fallback
            // would only add randomness, so return the best preferred answer now.
            if (!isKnownGood && bestBooks is not null)
            {
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_success", $"{bestModel}|{bestBooks.Count}", 100, ct);
                return new CloudAuthorBooksResult
                {
                    Books = bestBooks,
                    ResolvedModel = bestModel,
                    Outcomes = outcomes
                };
            }

            var percent = Math.Min(10 + i * 60 / candidates.Count, 70);
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_try", model, percent, ct);

            var (attempt, failureReason) = await TryGetAuthorBooksWithModelAsync(model, apiKey, userPrompt, ct);
            if (attempt is null)
            {
                outcomes[model] = failureReason ?? "falha";
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_failed", model, percent, ct);
                continue;
            }

            if (attempt.Count > 0 && isKnownGood && bestBooks is null)
            {
                // First non-empty preferred-model result: remember it and, unless it
                // is already rich enough, keep sweeping the preferred models.
                bestBooks = attempt;
                bestModel = model;
                outcomes[model] = $"ok|{attempt.Count}";
                if (attempt.Count >= goodEnoughCount)
                {
                    await PublishCloudProgressAsync(
                        connectionId, correlationId, "cloud_success", $"{model}|{attempt.Count}", 100, ct);
                    return new CloudAuthorBooksResult
                    {
                        Books = attempt,
                        ResolvedModel = model,
                        Outcomes = outcomes
                    };
                }
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_partial", model, percent, ct);
                continue;
            }

            if (attempt.Count > 0 && isKnownGood && bestBooks is not null)
            {
                // Another preferred model answered: keep the richer list.
                if (attempt.Count > bestBooks.Count)
                {
                    bestBooks = attempt;
                    bestModel = model;
                }
                outcomes[model] = $"ok|{attempt.Count}";
                if (attempt.Count >= goodEnoughCount)
                {
                    await PublishCloudProgressAsync(
                        connectionId, correlationId, "cloud_success", $"{model}|{attempt.Count}", 100, ct);
                    return new CloudAuthorBooksResult
                    {
                        Books = attempt,
                        ResolvedModel = model,
                        Outcomes = outcomes
                    };
                }
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_partial", model, percent, ct);
                continue;
            }

            if (attempt.Count == 0)
            {
                outcomes[model] = "vazio";
                await PublishCloudProgressAsync(
                    connectionId, correlationId, "cloud_empty", model, percent, ct);
                lastEmpty = attempt;
                continue;
            }

            // Fallback model (router / recommended / catalog) answered non-empty:
            // use the best of what we already have from the preferred models.
            outcomes[model] = $"ok|{attempt.Count}";
            var chosen = bestBooks is not null && bestBooks.Count >= attempt.Count
                ? bestBooks
                : attempt;
            var chosenModel = ReferenceEquals(chosen, attempt) ? model : bestModel;
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_success", $"{chosenModel}|{chosen.Count}", 100, ct);
            return new CloudAuthorBooksResult
            {
                Books = chosen,
                ResolvedModel = chosenModel,
                Outcomes = outcomes
            };
        }

        if (bestBooks is not null)
        {
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_success", $"{bestModel}|{bestBooks.Count}", 100, ct);
            return new CloudAuthorBooksResult
            {
                Books = bestBooks,
                ResolvedModel = bestModel,
                Outcomes = outcomes
            };
        }

        if (candidates.Count > 1)
        {
            await PublishCloudProgressAsync(
                connectionId, correlationId, "cloud_fallback", string.Empty, 75, ct);
        }

        if (lastEmpty is null || lastEmpty.Count == 0)
        {
            _logger.LogInformation(
                "[CloudAuthorBooks] Sweep terminou sem resultados para {Author}; resultados dos modelos: {Outcomes}",
                author, string.Join("; ", outcomes.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        return new CloudAuthorBooksResult
        {
            Books = lastEmpty,
            ResolvedModel = firstModel,
            FailureReason = FormatOutcomesSummary(outcomes, candidates),
            Outcomes = outcomes,
            AttemptedModelCount = candidates.Count
        };
    }

    private static string FormatOutcomesSummary(IReadOnlyDictionary<string, string> outcomes, IReadOnlyList<string> candidates)
    {
        if (outcomes.Count == 0)
            return "Nenhum modelo OpenRouter respondeu.";

        var parts = new List<string>();
        foreach (var model in candidates)
        {
            if (!outcomes.TryGetValue(model, out var code))
                continue;
            parts.Add($"{model}={NormalizeOutcomeCode(code)}");
        }
        return "OpenRouter não devolveu resultados. Modelos testados: " + string.Join("; ", parts) + ".";
    }

    private static string NormalizeOutcomeCode(string code)
    {
        if (code.StartsWith("ok|"))
        {
            var count = code.Split('|')[1];
            return $"{count} títulos";
        }
        return code switch
        {
            "ok" => "ok",
            "vazio" => "sem resultados",
            "no content" => "resposta vazia",
            "network error" => "erro de rede",
            "429 Too Many Requests" => "limite de requisições (429)",
            "parse failure" => "JSON inválido",
            "falha" => "falha",
            _ => code
        };
    }

    /// <summary>
    /// When the resolved model is free (e.g. openrouter/free), a weak/empty result
    /// is treated as a poor answer and the sweep continues. With paid models
    /// enabled, the curated recommended models (real paid slugs) are kept as-is
    /// (router first, paid prefixes after). In the FREE-only path the known-good
    /// free models come FIRST (verified live slugs, recommended-free list) so the
    /// automatic "openrouter/free" router — which picks a random free model per
    /// call — is only used as a last resort; never fabricate ":free" variants that
    /// do not exist in the catalog (OpenAI/Anthropic return HTTP 404 for ":free").
    /// Non-free/recommended models run alone.
    /// </summary>
    private async Task<List<string>> BuildCandidateModelsAsync(string first, bool allowPaid, CancellationToken ct)
    {
        if (!CloudChatSettingsKeys.IsFreeModel(first))
            return new List<string> { first };

        var isAutoRouter = string.Equals(first, CloudChatSettingsKeys.DefaultModelValue, StringComparison.OrdinalIgnoreCase);

        var models = new List<string>();
        if (allowPaid)
        {
            models.Add(first);
            foreach (var prefix in RecommendedExtractionModels.Prefixes)
                AddUniqueModel(models, prefix);
            return models;
        }

        if (!isAutoRouter)
            models.Add(first);

        var catalog = await _catalogService.GetFreeModelsAsync(ct);

        // Dynamic ordering: rank the LIVE catalog by OpenRouter's own quality
        // signal (profile.intelligence), then recency, so brand-new free models
        // are picked up automatically. The curated RecommendedFreeModelIds list is
        // only a tie-break boost (order a matched model just before an equal-scoring
        // one), never the source of truth. Router is appended last as final resort.
        var ranked = catalog
            .Where(m => !string.IsNullOrWhiteSpace(m.Id))
            .Where(m => !models.Contains(m.Id, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(m => RecommendedExtractionModels.IsRecommendedFree(m.Id))
            .ThenByDescending(m => m.IntelligenceIndex ?? 0.0)
            .ThenByDescending(m => m.ReleaseDate)
            .Select(m => m.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12 - models.Count)
            .ToList();

        models.AddRange(ranked);
        if (isAutoRouter)
            models.Add(first);
        if (models.Count == 0)
            models.Add(CloudChatSettingsKeys.DefaultModelValue);
        return models;
    }

    private static void AddUniqueModel(List<string> models, string candidate)
    {
        if (!models.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            models.Add(candidate);
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

    private async Task<(List<GoogleBookInfo>? Books, string? FailureReason)> TryGetAuthorBooksWithModelAsync(
        string model, string apiKey, string userPrompt, CancellationToken ct)
    {
        if (CloudChatSettingsKeys.IsFreeModel(model))
        {
            if (await _usageService.GetFreeRequestsRemainingAsync(ct) <= 0)
            {
                _logger.LogWarning("[CloudAuthorBooks] Teto diário de modelos gratuitos do OpenRouter atingido.");
                return (null, "limite diário atingido");
            }

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
            temperature = 0,
            max_tokens = 4096
        };

        var client = _httpClientFactory.CreateClient("OpenRouterCatalog");
        using var request = new HttpRequestMessage(HttpMethod.Post, CompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer-AuthorBooks");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload, SnakeCaseJson), Encoding.UTF8, "application/json");

        var sw = Stopwatch.StartNew();
        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "[CloudAuthorBooks] Pedido a OpenRouter cancelado para {Model} após {DurationMs}ms (authorBooks).",
                model, sw.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudAuthorBooks] Erro ao contactar OpenRouter com o modelo {Model} após {DurationMs}ms.", model, sw.ElapsedMilliseconds);
            return (null, "network error");
        }

        using (response)
        {
            if ((int)response.StatusCode == 429)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(model), ct);
                var rateHeaders = FormatRateLimitHeaders(response);
                _logger.LogWarning(
                    "[CloudAuthorBooks] 429 Too Many Requests do OpenRouter ({Model}) após {DurationMs}ms. RateLimit=[{RateLimit}]",
                    model, sw.ElapsedMilliseconds, rateHeaders);
                return (null, "429 Too Many Requests");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[CloudAuthorBooks] HTTP {Status} ({Model}) após {DurationMs}ms: {Body}",
                    (int)response.StatusCode, model, sw.ElapsedMilliseconds,
                    errorBody.Length > 300 ? errorBody[..300] + "..." : errorBody);
                return (null, $"HTTP {(int)response.StatusCode}");
            }

            var fullBody = await response.Content.ReadAsStringAsync(ct);
            var content = ExtractMessageContent(fullBody);
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning(
                    "[CloudAuthorBooks] Resposta sem conteúdo message.content ({Model}) após {DurationMs}ms. BodyPreview={BodyPreview}",
                    model, sw.ElapsedMilliseconds,
                    fullBody.Length > 800 ? fullBody[..800] + "..." : fullBody);
                return (null, "no content");
            }

            var books = ParseBooksJson(StripFences(content));
            if (books is null)
            {
                _logger.LogWarning("[CloudAuthorBooks] Falha a parsear JSON de {Model} após {DurationMs}ms. ContentPreview={ContentPreview}",
                    model, sw.ElapsedMilliseconds,
                    content.Length > 500 ? content[..500] + "..." : content);
                return (null, "parse failure");
            }

            return (books, null);
        }
    }

    private static string FormatRateLimitHeaders(HttpResponseMessage response)
    {
        var parts = new List<string>();
        foreach (var name in new[] { "X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "X-RateLimit-Used" })
        {
            if ((response.Headers.TryGetValues(name, out var h) || response.Content.Headers.TryGetValues(name, out h))
                && h.Any())
                parts.Add($"{name}={string.Join(",", h)}");
        }
        return parts.Count == 0 ? "<sem headers de rate limit>" : string.Join("; ", parts);
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
                if (first.TryGetProperty("message", out var message))
                {
                    if (message.TryGetProperty("content", out var content)
                        && content.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(content.GetString()))
                    {
                        return content.GetString();
                    }

                    // Some free/thinking models (e.g. openrouter/free routers that
                    // pick reasoning models) return the answer only in the "reasoning"
                    // field, leaving "content" null/empty. Fall back to it so the
                    // result is not lost to a bogus "no content".
                    foreach (var field in new[] { "reasoning", "reasoning_content" })
                    {
                        if (message.TryGetProperty(field, out var reasoning)
                            && reasoning.ValueKind == JsonValueKind.String
                            && !string.IsNullOrWhiteSpace(reasoning.GetString()))
                        {
                            return reasoning.GetString();
                        }
                    }
                }
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
            var id = item.TryGetProperty("id", out var i) ? i.GetString() : null;
            var year = item.TryGetProperty("year", out var y) ? y.GetString() : null;
            var thumbnail = item.TryGetProperty("thumbnail", out var th) ? th.GetString() : null;
            var subtitle = item.TryGetProperty("subtitle", out var s) ? s.GetString() : null;
            var publisher = item.TryGetProperty("publisher", out var p) ? p.GetString() : null;
            var confidence = item.TryGetProperty("confidence", out var c) &&
                             c.ValueKind == JsonValueKind.Number && c.TryGetDouble(out var conf)
                ? conf
                : (double?)null;
            var synthesized = item.TryGetProperty("synthesized", out var syn) &&
                              syn.ValueKind == JsonValueKind.True;
            result.Add(new GoogleBookInfo
            {
                Id = string.IsNullOrWhiteSpace(id) ? null : id,
                Title = title,
                Subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle,
                PublishedDate = string.IsNullOrWhiteSpace(year) ? null : year,
                Publisher = string.IsNullOrWhiteSpace(publisher) ? null : publisher,
                Thumbnail = GoogleBookInfo.IsUsableCover(thumbnail) ? thumbnail : null,
                Confidence = confidence is not null ? Math.Clamp(confidence.Value, 0.0, 1.0) : 0.7,
                Synthesized = synthesized
            });
        }
        return result;
    }
}
