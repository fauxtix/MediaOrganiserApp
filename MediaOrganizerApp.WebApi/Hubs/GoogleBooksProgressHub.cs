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

        private static string GroupName(string correlationId) => $"corr:{correlationId}";

        public Task JoinCorrelation(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogWarning("JoinCorrelation called with null/empty correlationId.");
                return Task.CompletedTask;
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(correlationId));
        }

        public Task LeaveCorrelation(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogWarning("LeaveCorrelation called with null/empty correlationId.");
                return Task.CompletedTask;
            }

            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(correlationId));
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }

}
