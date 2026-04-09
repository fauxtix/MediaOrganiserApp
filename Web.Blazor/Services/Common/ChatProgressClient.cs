using Microsoft.AspNetCore.SignalR.Client;

namespace Web.Blazor.Services.Common;

public class ChatProgressClient
{
    private HubConnection? _connection;
    public event Action<string>? ProgressReceived;
    public event Action<string>? ChunkReceived;
    public event Action? DoneReceived;
    public event Action<string?>? Reconnected;
    public event Action? Reconnecting;
    public event Action<Exception?>? Closed;

        public async Task StartAsync(string baseUrl)
    {
        if (_connection != null)
            return; // already started

            var baseHost = (baseUrl ?? "").TrimEnd('/');
            if (baseHost.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                baseHost = baseHost.Substring(0, baseHost.Length - 4);
            }

            var hubUrl = baseHost + "/chat-progress";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveProgress", (message) => ProgressReceived?.Invoke(message));
        _connection.On<string>("ReceiveChunk", (chunk) => ChunkReceived?.Invoke(chunk));
        _connection.On("ReceiveDone", () => DoneReceived?.Invoke());

        _connection.Reconnecting += (ex) =>
        {
            try { Reconnecting?.Invoke(); } catch { }
            return Task.CompletedTask;
        };

        _connection.Reconnected += (newId) =>
        {
            try { Reconnected?.Invoke(newId); } catch { }
            return Task.CompletedTask;
        };

        _connection.Closed += (ex) =>
        {
            try { Closed?.Invoke(ex); } catch { }
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            try { await _connection.StopAsync(); } catch { }
            try { await _connection.DisposeAsync(); } catch { }
            _connection = null;
        }
    }

    public string? ConnectionId => _connection?.ConnectionId;
}
