using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.WebApi.Hubs
{
    public class GoogleBooksProgressHub : Hub
    {
        private readonly ILogger<GoogleBooksProgressHub> _logger;

        public GoogleBooksProgressHub(ILogger<GoogleBooksProgressHub> logger)
        {
            _logger = logger;
        }

        public async Task SendProgress(string connectionId, string message)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("SendProgress called with null/empty connectionId. Message dropped.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveProgress", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending progress to connection {ConnectionId}", connectionId);
            }
        }

        public async Task SendChunk(string connectionId, string chunk)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("SendChunk called with null/empty connectionId. Chunk dropped.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveChunk", chunk);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chunk to connection {ConnectionId}", connectionId);
            }
        }

        public async Task SendDone(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.LogWarning("SendDone called with null/empty connectionId.");
                return;
            }

            try
            {
                await Clients.Client(connectionId).SendAsync("ReceiveDone");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending done to connection {ConnectionId}", connectionId);
            }
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }

}