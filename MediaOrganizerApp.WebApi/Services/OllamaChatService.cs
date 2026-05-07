using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private readonly IPromptTemplateProvider _prompts;
        private string _baseUrl;
        private string _model;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly IProgressPublisher _progressPublisher;


        public OllamaChatService(
            HttpClient httpClient,
            ILogger<OllamaChatService> logger,
            IConfiguration config,
            IPromptTemplateProvider prompts,
            IAppSettingsRepository appSettingsRepository,
            IProgressPublisher progressPublisher)
        {
            _httpClient = httpClient;
            _logger = logger;
            _prompts = prompts;
            _appSettingsRepository = appSettingsRepository;
            _progressPublisher = progressPublisher;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            string? correlationId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var (modelBaseUrl, modelName) = await GetAiSettings();
            _baseUrl = modelBaseUrl;
            _model = modelName;
            await PublishAsync(connectionId, correlationId, "started", 5, "Starting chat request...", cancellationToken);

            var payload = new
            {
                model = _model,
                messages,
                stream = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_baseUrl}/api/chat"))
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            await PublishAsync(connectionId, correlationId, "response_received", 60, "Receiving chat response...", cancellationToken);

            // Chat hub removed: progress will be delivered via HTTP streaming only

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string? content = null;

                try
                {
                    using var json = JsonDocument.Parse(line);

                    if (json.RootElement.TryGetProperty("message", out var msg))
                    {
                        content = msg.GetProperty("content").GetString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro a processar chunk do Ollama");
                    continue;
                }

                if (!string.IsNullOrEmpty(content))
                {
                    // yield chunk for HTTP streaming clients
                    yield return content;
                }
            }

            // done
            await PublishAsync(connectionId, correlationId, "completed", 100, "Chat request completed.", cancellationToken);
        }
        private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
        {
            var settings = await _appSettingsRepository.GetSettingsAsync();
            return (settings.ModelBaseUrl, settings.AiModelName);
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
