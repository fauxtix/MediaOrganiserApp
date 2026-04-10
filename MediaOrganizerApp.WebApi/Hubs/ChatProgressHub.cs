using Microsoft.AspNetCore.SignalR;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Collections.Concurrent;

namespace MediaOrganizerApp.WebApi.Hubs
{
    public class ChatProgressHub : Hub
    {
        private readonly ILogger<ChatProgressHub> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        // Track background processing per connection for hub-started chats
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _processing = new();

        public ChatProgressHub(ILogger<ChatProgressHub> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // Optional reattach API: allows a client that reconnected to tell the server
        // to move any existing background processing from the old connection id
        // to the current one. This is best-effort.
        public Task Reattach(string oldConnectionId)
        {
            try
            {
                var current = Context.ConnectionId;
                if (string.IsNullOrWhiteSpace(oldConnectionId) || string.IsNullOrWhiteSpace(current))
                    return Task.CompletedTask;

                if (_processing.TryRemove(oldConnectionId, out var cts))
                {
                    // transfer processing to current connection id
                    _processing[current] = cts;
                    _logger.LogInformation("Reattached processing from {Old} to {New}", oldConnectionId, current);
                }
            }
            catch (Exception ex)
            {
                try { _logger.LogWarning(ex, "Reattach failed"); } catch { }
            }

            return Task.CompletedTask;
        }

        public async Task SendProgress(string connectionId, string message)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("A função SendProgress foi chamada com um connectionId nulo/vazio. A mensagem foi descartada.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveProgress", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao enviar o progresso para a conexão {ConnectionId}", connectionId);
            }
        }

        public async Task SendChunk(string connectionId, string chunk)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("O método SendChunk foi chamado com um connectionId nulo ou vazio. O chunk foi descartado.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveChunk", chunk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar o bloco para a ligação {ConnectionId}", connectionId);
            }
        }

        public async Task SendDone(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("O método SendDone foi chamado com um connectionId nulo ou vazio.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveDone");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar a confirmação de conclusão para a ligação {ConnectionId}", connectionId);
            }
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public Task CancelChat()
        {
            var connectionId = Context.ConnectionId;
            if (_processing.TryRemove(connectionId, out var cts))
            {
                try { cts.Cancel(); } catch { }
                try { cts.Dispose(); } catch { }
            }

            return Task.CompletedTask;
        }

        // Start background chat processing using the caller's SignalR connection id.
        public Task StartChat(List<ChatMessageDto> messages, string? language)
        {
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("StartChat called with empty connection id.");
                return Task.CompletedTask;
            }

            try
            {
                _logger.LogInformation("StartChat invoked for {ConnectionId} with {Count} messages", connectionId, messages?.Count ?? 0);
            }
            catch { }


            var cts = new CancellationTokenSource();
            if (!_processing.TryAdd(connectionId, cts))
            {
                // already processing for this connection
                _logger.LogWarning("StartChat called for connection already processing: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            }

            _ = Task.Run(async () =>
            {
                // Create a new DI scope for the background work so scoped services (like HttpClient)
                // are resolved within a lifetime that lives for the duration of this background task.
                using var scope = _scopeFactory.CreateScope();
                var chatService = scope.ServiceProvider.GetRequiredService<IAiChatService>();

                try
                {
                    if (!string.IsNullOrWhiteSpace(language))
                    {
                        messages.Insert(0, new ChatMessageDto
                        {
                            Role = "system",
                            Content = $"Please respond in {language}. Keep answers concise and avoid repetition."
                        });
                    }

                    await foreach (var _ in chatService.StreamChatAsync(messages, connectionId, cts.Token))
                    {
                        // IAiChatService implementations will push hub messages.
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background chat processing for {ConnectionId}", connectionId);
                }
                finally
                {
                    _processing.TryRemove(connectionId, out var _);
                }
            });

            return Task.CompletedTask;
        }
    }
}
