using Microsoft.AspNetCore.SignalR.Client;

namespace Web.Blazor.Services.Common;

public sealed class MediaInfoProgressClient
{
    private HubConnection? _connection;

    public event Action<string>? ProgressReceived;

    public async Task StartAsync(string apiBaseUrl)
    {
        var hubUrl = $"{apiBaseUrl.TrimEnd('/')}/media-info-progress";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Server sends: ReceiveProgress(connectionId, message)
        _connection.On<string, string>("ReceiveProgress", (_, message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            try
            {
                await _connection.StopAsync();
            }
            catch
            {
                // ignore
            }

            try
            {
                await _connection.DisposeAsync();
            }
            catch
            {
                // ignore
            }

            _connection = null;
        }
    }

    public string? ConnectionId => _connection?.ConnectionId;

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}