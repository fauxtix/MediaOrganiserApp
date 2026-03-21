using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
namespace MediaOrganizerApp.WebApi.Hubs;

[AllowAnonymous]
public class MediaInfoProgressHub : Hub
{
    public async Task SendProgress(string connectionId, string message)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, message);
    }
}
