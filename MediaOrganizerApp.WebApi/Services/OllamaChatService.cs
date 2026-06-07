using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private readonly IAiSettingsProvider _aiSettingsProvider;
        private readonly IProgressPublisher _progressPublisher;

        public OllamaChatService(
            HttpClient httpClient,
            ILogger<OllamaChatService> logger,
            IAiSettingsProvider aiSettingsProvider,
            IProgressPublisher progressPublisher)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiSettingsProvider = aiSettingsProvider;
            _progressPublisher = progressPublisher;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            string? correlationId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var (baseUrl, model) = await _aiSettingsProvider.GetAiSettingsAsync();

            await PublishAsync(connectionId, correlationId, "started", 5,
                "Starting chat request...", cancellationToken);

            var payload = new
            {
                model,
                messages,
                temperature = 0.7,
                stream = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri($"{baseUrl}/api/chat"))
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

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

            string? line;

            while ((line = await reader.ReadLineAsync()) != null
                   && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string? content = null;

                try
                {
                    using var json = JsonDocument.Parse(line);
                    content = ExtractContent(json.RootElement);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro a processar chunk do Ollama");
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    yield return content;
                }
            }

            await PublishAsync(connectionId, correlationId, "completed", 100,
                "Chat request completed.", cancellationToken);
        }

        private static string? ExtractContent(JsonElement root)
        {
            if (!root.TryGetProperty("message", out var msg))
                return null;

            string? content = msg.TryGetProperty("content", out var c)
                ? c.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(content))
            {
                return Clean(content);
            }

            string? thinking = msg.TryGetProperty("thinking", out var t)
                ? t.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(thinking))
            {
                return Clean(thinking);
            }

            return null;
        }

        private static string Clean(string text)
        {
            return text
                .Replace("<think>", "")
                .Replace("</think>", "");
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