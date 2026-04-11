using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
// SignalR hub removed for chat; HTTP streaming only
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaChatService> _logger;
        private readonly IConfiguration _config;
        private readonly IPromptTemplateProvider _prompts;

        public OllamaChatService(
            HttpClient httpClient,
            ILogger<OllamaChatService> logger,
            IConfiguration config,
            IPromptTemplateProvider prompts)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            _prompts = prompts;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model = _config["Ai:Ollama:Model"] ?? "phi4-mini:latest";
//            var model = "phi4-mini:latest";

            var payload = new
            {
                model,
                messages,
                stream = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{baseUrl}/api/chat"))
            {
                Content = JsonContent.Create(payload)
            };

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

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
        }
    }
}
