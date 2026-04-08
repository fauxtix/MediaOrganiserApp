using Microsoft.AspNetCore.SignalR;

namespace MediaOrganizerApp.WebApi.Hubs
{
    public class ChatProgressHub : Hub
    {
        private readonly ILogger<ChatProgressHub> _logger;

        public ChatProgressHub(ILogger<ChatProgressHub> logger)
        {
            _logger = logger;
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
    }
}
