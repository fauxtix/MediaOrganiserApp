using Microsoft.AspNetCore.SignalR;

namespace MediaOrganizerApp.WebApi.Hubs;

public class MediaInfoProgressHub : Hub
{
    private static string GroupName(string correlationId) => $"corr:{correlationId}";

    public Task JoinCorrelation(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return Task.CompletedTask;

        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(correlationId));
    }

    public Task LeaveCorrelation(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            return Task.CompletedTask;

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(correlationId));
    }
}
