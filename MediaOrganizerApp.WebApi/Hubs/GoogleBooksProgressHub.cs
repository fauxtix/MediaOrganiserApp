using Microsoft.AspNetCore.SignalR;

namespace MediaOrganizerApp.WebApi.Hubs
{
    public class GoogleBooksProgressHub : Hub
    {
        public async Task SendProgress(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveProgress", message);
        }
    }
}