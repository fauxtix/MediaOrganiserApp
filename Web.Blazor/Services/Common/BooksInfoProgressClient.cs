using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common;

public class BooksInfoProgressClient
{
    private HubConnection? _connection;
    public event Action<string>? ProgressReceived;

    public async Task StartAsync(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:44303/books-info-progress")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string>("ReceiveProgress", (connectionId, message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null)
            await _connection.StopAsync();
    }

    public string? ConnectionId => _connection?.ConnectionId;
}
