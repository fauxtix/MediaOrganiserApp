using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Exceptions;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// Streams OpenRouter chat completions to the caller, applies the provider
/// fallback/retry rules from the reference implementation, and auto-records
/// benchmark rows for each completed exchange.
/// </summary>
public sealed class OpenRouterChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IAIBenchmarkRepository _benchmarkRepository;
    private readonly ICloudChatUsageService _usageService;
    private readonly IInternetAvailabilityService _internet;
    private readonly ILogger<OpenRouterChatService> _logger;

    public OpenRouterChatService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IAIBenchmarkRepository benchmarkRepository,
        ICloudChatUsageService usageService,
        IInternetAvailabilityService internet,
        ILogger<OpenRouterChatService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _benchmarkRepository = benchmarkRepository;
        _usageService = usageService;
        _internet = internet;
        _logger = logger;
    }

    public const string ChatCompletionsUrl = "https://openrouter.ai/api/v1/chat/completions";

    public async IAsyncEnumerable<CloudChatChunk> StreamChatAsync(
        CloudChatPayload payload,
        string? userId,
        string? idiomaSessao,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogWarning("[OpenRouter] Sem acesso à internet; a saltar OpenRouter (fallback local Ollama).");
            throw new CloudProviderTransientException("Sem ligação à internet. A usar apenas o serviço local (Ollama).");
        }

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Chave de API OpenRouter não configurada. Configure em Configurações.");

        var allowPaid = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, ct),
            out var paid) ? paid : false;
        payload.Model = CloudChatSettingsKeys.ApplyPaidModelGate(payload.Model, allowPaid);

        if (CloudChatSettingsKeys.IsFreeModel(payload.Model))
        {
            try
            {
                await _usageService.RegisterFreeModelUsageAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OpenRouter] Falha a registar requisição gratuita.");
            }
        }

        var reasoningRetried = false;
        while (true)
        {
            var inner = StreamOpenRouterInternalAsync(payload, apiKey, userId, idiomaSessao, ct);
            await using var enumerator = inner.GetAsyncEnumerator(ct);

            var retry = false;
            while (true)
            {
                CloudChatChunk chunk;
                try
                {
                    if (!await enumerator.MoveNextAsync()) break;
                    chunk = enumerator.Current;
                }
                catch (CloudModelReasoningUnsupportedException) when (!reasoningRetried && payload.EnableReasoning)
                {
                    reasoningRetried = true;
                    payload.EnableReasoning = false;
                    _logger.LogWarning("[OpenRouter] Reasoning não suportado pelo modelo {Model}; a repetir sem reasoning.", payload.Model);
                    retry = true;
                    break;
                }

                yield return chunk;
            }

            if (!retry)
                yield break;
        }
    }

    private async IAsyncEnumerable<CloudChatChunk> StreamOpenRouterInternalAsync(
        CloudChatPayload payload,
        string apiKey,
        string? userId,
        string? idiomaSessao,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var (response, reader) = await OpenOpenRouterConnectionAsync(payload, apiKey, ct);

        if (response is null || reader is null)
            yield break;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var textBuilder = new StringBuilder();
        bool anyText = false;
        bool doneReached = false;
        double firstChunkSeconds = 0;
        string lastNonEmptyLine = string.Empty;
        int completionTokens = 0;
        int promptTokens = 0;
        string? servedModel = null;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;

                if (line.StartsWith("data: ")) line = line[6..];
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line == "[DONE]")
                {
                    doneReached = true;
                    break;
                }

                lastNonEmptyLine = line;

                var chunk = ChatChunkParser.ParseOpenRouterChunk(line);
                if (chunk is not null)
                {
                    if (!string.IsNullOrEmpty(chunk.Text))
                    {
                        textBuilder.Append(chunk.Text);
                        if (!anyText)
                        {
                            anyText = true;
                            firstChunkSeconds = sw.Elapsed.TotalSeconds;
                            _logger.LogInformation("[OpenRouter] Primeiro token após {Seconds:F1}s (modelo {Model}, msgs {Count})",
                                firstChunkSeconds, payload.Model, payload.Messages.Count);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(chunk.Model))
                        servedModel = chunk.Model;
                    if (chunk.CompletionTokens is > 0) completionTokens += chunk.CompletionTokens.Value;
                    if (chunk.PromptTokens is > 0) promptTokens += chunk.PromptTokens.Value;

                    yield return chunk;
                }
            }
        }
        finally
        {
            sw.Stop();

            if (!anyText)
            {
                _logger.LogWarning("[OpenRouter] Stream terminou sem texto após {Seconds:F1}s. done={Done}, cancelled={Cancelled}, ttf={TtF:F1}s, ultima linha: {LastLine}",
                    sw.Elapsed.TotalSeconds, doneReached, ct.IsCancellationRequested, firstChunkSeconds, TruncateForLog(lastNonEmptyLine));
            }

            reader.Dispose();
            response.Dispose();
        }

        if (anyText && !ct.IsCancellationRequested)
        {
            await TryRecordBenchmarkAsync(payload, userId, idiomaSessao, servedModel,
                sw.Elapsed, firstChunkSeconds, promptTokens, completionTokens, textBuilder.ToString(), ct);
        }
    }

    private async Task<(HttpResponseMessage? Response, System.IO.StreamReader? Reader)> OpenOpenRouterConnectionAsync(
        CloudChatPayload payload, string apiKey, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("OpenRouterChat");

        var json = CloudRequestBodyBuilder.Serialize(payload);
        _logger.LogInformation("[OpenRouter] Envio: model={Model}, enableReasoning={EnableReasoning}, msgs={Count}",
            payload.Model, payload.EnableReasoning, payload.Messages.Count);

        using var request = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("HTTP-Referer", "http://localhost");
        request.Headers.Add("X-Title", "MediaOrganizer");
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[OpenRouter] Erro de rede ao chamar API.");
            throw new CloudProviderTransientException($"Erro de rede ao comunicar com OpenRouter: {ex.Message}", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            _logger.LogWarning("[OpenRouter] HTTP {StatusCode}: {Body}", (int)statusCode, responseBody);
            response.Dispose();

            if (statusCode == System.Net.HttpStatusCode.NotFound)
            {
                if (string.Equals(payload.Model, CloudChatSettingsKeys.DefaultModelValue, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"OpenRouter: modelo '{payload.Model}' não encontrado. Nenhum modelo gratuito funcionou.");

                _logger.LogWarning("[OpenRouter] Modelo '{Model}' devolveu 404; a tentar openrouter/free.", payload.Model);
                payload.Model = CloudChatSettingsKeys.DefaultModelValue;
                return await OpenOpenRouterConnectionAsync(payload, apiKey, ct);
            }

            if (statusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await SyncFreeUsageFromRateLimitHeadersAsync(response, CloudChatSettingsKeys.IsFreeModel(payload.Model), ct);
                throw new CloudProviderTransientException("OpenRouter: limite de requisições excedido (429). Aguarde alguns minutos e tente novamente.");
            }

            if (statusCode == System.Net.HttpStatusCode.BadRequest && payload.EnableReasoning && IsReasoningErrorResponse(responseBody))
                throw new CloudModelReasoningUnsupportedException(
                    $"OpenRouter: o modelo '{payload.Model}' não suporta reasoning. {TruncateForLog(responseBody, 300)}");

            throw new InvalidOperationException($"OpenRouter: erro HTTP {(int)statusCode}: {responseBody}");
        }

        var stream = await response.Content.ReadAsStreamAsync(ct);
        return (response, new System.IO.StreamReader(stream, System.Text.Encoding.UTF8));
    }

    private async Task TryRecordBenchmarkAsync(
        CloudChatPayload payload,
        string? userId,
        string? idiomaSessao,
        string? servedModel,
        TimeSpan elapsed,
        double firstChunkSeconds,
        int promptTokens,
        int completionTokens,
        string text,
        CancellationToken ct)
    {
        try
        {
            var promptText = payload.Messages.LastOrDefault(m => string.Equals(m.Role, "user", StringComparison.OrdinalIgnoreCase))?.Content ?? string.Empty;
            var promptId = await _benchmarkRepository.GetOrCreatePromptIdAsync(
                promptText,
                userId ?? string.Empty,
                CloudResponseMetrics.SmartFallbackDescription(promptText),
                payload.Temperature,
                ct);

            var totalTokens = promptTokens + completionTokens;
            var seconds = elapsed.TotalSeconds > 0 ? elapsed.TotalSeconds : 0.001;
            var response = new AIResponse
            {
                PromptId = promptId,
                UserId = userId ?? string.Empty,
                NomeModelo = payload.Model,
                ModeloResolvido = servedModel != null && !string.Equals(servedModel, payload.Model, StringComparison.OrdinalIgnoreCase)
                    ? servedModel
                    : null,
                TextoResposta = text,
                TempoProcessamento = seconds,
                TokensPorSegundo = totalTokens > 0 ? Math.Round(totalTokens / seconds, 2) : null,
                TempoPuroMs = elapsed.TotalMilliseconds,
                TempoCargaMs = firstChunkSeconds > 0 ? firstChunkSeconds * 1000 : null,
                TamanhoTokens = completionTokens > 0 ? completionTokens : CloudResponseMetrics.EstimateTokens(text),
                IdiomaSessao = idiomaSessao
            };

            await _benchmarkRepository.CreateResponseAsync(response, ct);
            _logger.LogInformation("[OpenRouter] Benchmark registado: prompt={PromptId}, modelo={Model}, servido={Served}, tokens={Tokens}, tempo={Seconds:F1}s",
                promptId, payload.Model, servedModel, totalTokens, seconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OpenRouter] Falha ao registar benchmark.");
        }
    }

    private static bool IsReasoningErrorResponse(string body)
    {
        if (string.IsNullOrEmpty(body)) return false;
        return body.Contains("reasoning", StringComparison.OrdinalIgnoreCase)
            || body.Contains("thinking", StringComparison.OrdinalIgnoreCase);
    }

    private static string TruncateForLog(string value, int max = 500)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value[..max] + "...(truncado)";
    }

    private async Task SyncFreeUsageFromRateLimitHeadersAsync(HttpResponseMessage response, bool isFreeModel, CancellationToken ct)
    {
        if (!isFreeModel) return;

        var limitFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Limit", out var limit)
            || TryGetHeaderValue(response.Content, "X-RateLimit-Limit", out limit);
        var remainingFound = TryGetHeaderValue(response.Headers, "X-RateLimit-Remaining", out var remaining)
            || TryGetHeaderValue(response.Content, "X-RateLimit-Remaining", out remaining);

        if (!limitFound || !remainingFound) return;

        try
        {
            await _usageService.SyncFreeUsageFromHeadersAsync(limit, remaining, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[OpenRouter] Falha a sincronizar contador de uso gratuito a partir dos headers 429.");
        }
    }

    private static bool TryGetHeaderValue(
        System.Net.Http.Headers.HttpHeaders headers, string name, out int value)
    {
        value = 0;
        if (!headers.TryGetValues(name, out var vals)) return false;
        var raw = vals.FirstOrDefault();
        return int.TryParse(raw, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static bool TryGetHeaderValue(
        System.Net.Http.HttpContent content, string name, out int value)
    {
        value = 0;
        if (content is null) return false;
        return TryGetHeaderValue(content.Headers, name, out value);
    }
}