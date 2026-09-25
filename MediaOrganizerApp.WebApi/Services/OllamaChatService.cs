using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private readonly IAiSettingsProvider _aiSettingsProvider;
        private readonly IProgressPublisher _progressPublisher;
        private readonly IAIChatSettingsRepository _settingsRepository;

        public OllamaChatService(
            HttpClient httpClient,
            ILogger<OllamaChatService> logger,
            IAiSettingsProvider aiSettingsProvider,
            IProgressPublisher progressPublisher,
            IAIChatSettingsRepository settingsRepository)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiSettingsProvider = aiSettingsProvider;
            _progressPublisher = progressPublisher;
            _settingsRepository = settingsRepository;
        }

        public async IAsyncEnumerable<AiChatChunk> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            string? correlationId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var (baseUrl, model) = await _aiSettingsProvider.GetAiSettingsAsync();

            var showReasoning = bool.TryParse(
                await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.EnableReasoning, null, cancellationToken),
                out var enabled) && enabled;

            await PublishAsync(connectionId, correlationId, "started", 5,
                "Starting chat request...", cancellationToken);

            var payload = new Dictionary<string, object?>
            {
                ["model"] = model,
                ["messages"] = messages,
                ["temperature"] = 0.7,
                ["stream"] = true
            };

            // Modelos com capacidade de "thinking" (Qwen3, deepseek-r1, qwq, ...)
            // recebem o campo `think` explícito: true com o reasoning ativo, false com
            // desativado. Em modelos comuns o campo é omitido (o Ollama devolve 400
            // "does not support thinking" se for enviado a um modelo sem suporte).
            if (SupportsThinking(model))
            {
                payload["think"] = showReasoning;
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri($"{baseUrl}/api/chat"))
            {
                Content = JsonContent.Create(payload)
            };

using var response = await _httpClient.SendAsync(
    request,
    HttpCompletionOption.ResponseHeadersRead,
    cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogError("Ollama error: {StatusCode} - {Error}",
                    response.StatusCode,
                    error);

                yield break;
            }

            await PublishAsync(connectionId, correlationId, "response_received", 60,
                "Receiving chat response...", cancellationToken);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            // Estado cross-chunk para modelos que emitem o raciocínio INLINE no
            // `content` (com marcadores `...`/`...`), em vez de o colocar no
            // campo `thinking`/`reasoning_content`. O bloco de thinking atravessa
            // vários tokens, por isso é acumulado aqui até ao marcador de fecho.
            var pendingInlineReasoning = new StringBuilder();
            var inlineMarkerCarry = new StringBuilder();
            var inInlineThinking = false;

            string? line;

            while ((line = await reader.ReadLineAsync()) != null
                   && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                AiChatChunk? chunk = null;

                try
                {
                    using var json = JsonDocument.Parse(line);
                    var root = json.RootElement;

                    chunk = ExtractChunk(
                        root,
                        showReasoning,
                        pendingInlineReasoning,
                        inlineMarkerCarry,
                        ref inInlineThinking);

                    // Alguns modelos (ex.: qwq, deepseek-r1) enviam o reasoning apenas
                    // na mensagem final `done:true` e não em chunks incrementais. Nesse
                    // caso o reasoning final é extraído da própria mensagem final e é
                    // emitido como um chunk autónomo, para não se perder. O mesmo se
                    // aplica a um bloco de thinking inline que ficou aberto até ao fim.
                    if (root.TryGetProperty("done", out var doneProp)
                        && doneProp.GetBoolean())
                    {
                        var flushedText = FlushInlineThinking(
                            showReasoning,
                            pendingInlineReasoning,
                            inlineMarkerCarry,
                            ref inInlineThinking,
                            out var inlineFinalReasoning);

                        var rootReasoning = ExtractReasoning(root, showReasoning);
                        var text = (chunk?.Text ?? string.Empty) + flushedText;
                        var reasoning = !string.IsNullOrEmpty(rootReasoning)
                            ? rootReasoning
                            : !string.IsNullOrEmpty(chunk?.Reasoning)
                                ? chunk.Reasoning
                                : inlineFinalReasoning;

                        chunk = text.Length == 0 && string.IsNullOrEmpty(reasoning)
                            ? null
                            : new AiChatChunk { Text = text, Reasoning = reasoning };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro a processar chunk do Ollama");
                }

                if (chunk is not null
                    && (!string.IsNullOrEmpty(chunk.Text) || !string.IsNullOrEmpty(chunk.Reasoning)))
                {
                    yield return chunk;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                var flushedText = FlushInlineThinking(
                    showReasoning,
                    pendingInlineReasoning,
                    inlineMarkerCarry,
                    ref inInlineThinking,
                    out var finalInlineReasoning);

                if (flushedText.Length > 0 || !string.IsNullOrEmpty(finalInlineReasoning))
                {
                    yield return new AiChatChunk
                    {
                        Text = flushedText,
                        Reasoning = finalInlineReasoning
                    };
                }
            }

            await PublishAsync(connectionId, correlationId, "completed", 100,
                "Chat request completed.", cancellationToken);
        }

        private static AiChatChunk? ExtractChunk(
            JsonElement root,
            bool showReasoning,
            StringBuilder pendingInlineReasoning,
            StringBuilder inlineMarkerCarry,
            ref bool inInlineThinking)
        {
            if (!root.TryGetProperty("message", out var msg))
                return null;

            // Ollama streams the answer token by token; each token carries the
            // whitespace that separates it from the next one (e.g. " this"). Do NOT
            // trim, otherwise word separators are lost. Whitespace-only tokens
            // (spaces, "\n") must also be preserved to keep paragraph breaks.
            string? rawContent = msg.TryGetProperty("content", out var c)
                ? c.GetString()
                : null;

            string? inlineFlushedReasoning = null;

            string text = rawContent is { Length: > 0 }
                ? ProcessInlineThinking(
                    rawContent,
                    showReasoning,
                    pendingInlineReasoning,
                    inlineMarkerCarry,
                    ref inInlineThinking,
                    out inlineFlushedReasoning)
                : string.Empty;

            // Field-based reasoning (message.thinking / reasoning_content) wins;
            // inline-extracted reasoning (the `...`/`...` block) is only a
            // fallback for models that stream the thinking inside `content`.
            string? reasoning = ExtractReasoning(msg, showReasoning) ?? inlineFlushedReasoning;

            // Text and Reasoning are independent and may coexist in the same chunk:
            // reasoning models emit whitespace-only "content" tokens around/while the
            // thinking flows (spaces, "\n"), so "content" being present must NOT hide a
            // "thinking" field on the same message. With the toggle OFF the thinking is
            // discarded so it never bleeds into the answer bubble.
            if (text.Length == 0 && string.IsNullOrEmpty(reasoning))
                return null;

            return new AiChatChunk { Text = text, Reasoning = reasoning };
        }

        private static string? ExtractReasoning(JsonElement source, bool showReasoning)
        {
            if (!showReasoning)
                return null;

            // Ollama naming varies by model/version: `reasoning_content` (OpenAI-style
            // alias) or `thinking` (deepseek-r1/qwq/Qwen3 native). Read either.
            if (source.TryGetProperty("reasoning_content", out var rc)
                && rc.GetString() is { Length: > 0 } reasoningContent)
            {
                return reasoningContent;
            }

            return source.TryGetProperty("thinking", out var t)
                ? t.GetString()
                : null;
        }

        private static bool SupportsThinking(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return false;

            // Only send the `think` field to models known to support it, otherwise the
            // Ollama API rejects the request with 400 "does not support thinking".
            var m = model.ToLowerInvariant();

            return m.StartsWith("qwen3", StringComparison.Ordinal)
                || m.Contains("deepseek-r1", StringComparison.Ordinal)
                || m.StartsWith("qwq", StringComparison.Ordinal)
                || m.Contains(":reasoning", StringComparison.Ordinal);
        }

        private static readonly string[] InlineThinkingOpenMarkers =
        {
            "\u003Cthink\u003E",
            " thinking"
        };

        private static readonly string[] InlineThinkingCloseMarkers =
        {
            "\u003C/think\u003E",
            " response"
        };

        private static readonly string[] InlineThinkingOutsideMarkers =
        {
            "\u003Cthink\u003E",
            " thinking",
            "\u003C/think\u003E"
        };

        // Qwen3 (and other reasoning models) sometimes stream the thinking block
        // INLINE inside `content` (with `...`/`...` wrappers, also the naked
        // ` thinking`/` response` tokens) instead of putting it in the
        // `message.thinking` field. The block can span several NDJSON tokens, so
        // the extraction must carry state across chunks. Whitespace must never be
        // trimmed (word separators are prefixed to tokens).
        private static string ProcessInlineThinking(
            string content,
            bool showReasoning,
            StringBuilder pendingInlineReasoning,
            StringBuilder inlineMarkerCarry,
            ref bool inInlineThinking,
            out string? flushedReasoning)
        {
            flushedReasoning = null;

            var result = new StringBuilder(content.Length);
            inlineMarkerCarry.Append(content);
            var buffer = inlineMarkerCarry.ToString();
            inlineMarkerCarry.Clear();

            var pos = 0;

            while (pos < buffer.Length)
            {
                var markers = inInlineThinking
                    ? InlineThinkingCloseMarkers
                    : InlineThinkingOutsideMarkers;

                if (!TryFindAny(buffer, markers, pos, out var idx, out var len))
                {
                    var rest = buffer.Substring(pos);
                    var carryLength = PartialMarkerLength(rest, markers);
                    var emitLength = rest.Length - carryLength;

                    AppendInlineSegment(
                        rest.Substring(0, emitLength),
                        showReasoning,
                        pendingInlineReasoning,
                        inInlineThinking,
                        result);

                    if (carryLength > 0)
                        inlineMarkerCarry.Append(rest, rest.Length - carryLength, carryLength);

                    break;
                }

                if (idx > pos)
                {
                    AppendInlineSegment(
                        buffer.Substring(pos, idx - pos),
                        showReasoning,
                        pendingInlineReasoning,
                        inInlineThinking,
                        result);
                }

                if (inInlineThinking)
                {
                    inInlineThinking = false;

                    if (showReasoning && pendingInlineReasoning.Length > 0)
                    {
                        flushedReasoning = pendingInlineReasoning.ToString();
                        pendingInlineReasoning.Clear();
                    }
                }
                else if (IsOpenMarker(buffer, idx, len))
                {
                    inInlineThinking = true;
                }

                pos = idx + len;
            }

            return result.ToString();
        }

        private static void AppendInlineSegment(
            string segment,
            bool showReasoning,
            StringBuilder pendingInlineReasoning,
            bool inInlineThinking,
            StringBuilder result)
        {
            if (segment.Length == 0)
                return;

            if (inInlineThinking)
            {
                if (showReasoning)
                    pendingInlineReasoning.Append(segment);
            }
            else
            {
                result.Append(segment);
            }
        }

        private static string FlushInlineThinking(
            bool showReasoning,
            StringBuilder pendingInlineReasoning,
            StringBuilder inlineMarkerCarry,
            ref bool inInlineThinking,
            out string? flushedReasoning)
        {
            flushedReasoning = null;
            var result = new StringBuilder();

            if (inlineMarkerCarry.Length > 0)
            {
                AppendInlineSegment(
                    inlineMarkerCarry.ToString(),
                    showReasoning,
                    pendingInlineReasoning,
                    inInlineThinking,
                    result);
                inlineMarkerCarry.Clear();
            }

            if (showReasoning && pendingInlineReasoning.Length > 0)
            {
                flushedReasoning = pendingInlineReasoning.ToString();
                pendingInlineReasoning.Clear();
            }

            inInlineThinking = false;
            return result.ToString();
        }

        private static bool IsOpenMarker(string text, int index, int length)
        {
            foreach (var marker in InlineThinkingOpenMarkers)
            {
                if (marker.Length == length
                    && string.Equals(text.Substring(index, length), marker, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int PartialMarkerLength(string text, string[] markers)
        {
            var longest = 0;

            foreach (var marker in markers)
            {
                var maxLength = Math.Min(text.Length, marker.Length - 1);
                for (var length = maxLength; length > longest; length--)
                {
                    if (string.Equals(
                            text.Substring(text.Length - length),
                            marker[..length],
                            StringComparison.OrdinalIgnoreCase))
                    {
                        longest = length;
                        break;
                    }
                }
            }

            return longest;
        }

        private static bool TryFindAny(
            string text,
            string[] markers,
            int start,
            out int index,
            out int length)
        {
            index = int.MaxValue;
            length = 0;

            foreach (var marker in markers)
            {
                var idx = text.IndexOf(marker, start, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0 && idx < index)
                {
                    index = idx;
                    length = marker.Length;
                }
            }

            if (index == int.MaxValue)
            {
                index = -1;
                length = 0;
                return false;
            }

            return true;
        }

        private Task PublishAsync(
            string? connectionId,
            string? correlationId,
            string stage,
            int percent,
            string message,
            CancellationToken cancellationToken,
            string level = "info")
            => _progressPublisher.PublishAsync(
                ProgressChannel.Chat,
                new ProgressUpdateDto
                {
                    CorrelationId = correlationId,
                    Stage = stage,
                    Percent = percent,
                    Message = message,
                    Level = level,
                    Service = nameof(OllamaChatService),
                    Operation = "chat"
                },
                connectionId,
                cancellationToken);
    }
}