using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<GoogleBooksProgressHub> _hubContext;

        public OllamaChatService(
            HttpClient httpClient,
            ILogger<OllamaChatService> logger,
            IConfiguration config,
            IPromptTemplateProvider prompts,
            IHubContext<GoogleBooksProgressHub> hubContext)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            _prompts = prompts;
            _hubContext = hubContext;
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

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

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();

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
                    // ✔️ agora já não está dentro de try/catch
                    yield return content;

                    if (!string.IsNullOrWhiteSpace(connectionId))
                    {
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveChunk", content, cancellationToken);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveDone", cancellationToken);
            }
        }
    }
}
