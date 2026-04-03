using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common;

public class BooksInfoProgressClient
{
    private HubConnection? _connection;
    public event Action<string>? ProgressReceived;
    public event Action<string>? ChunkReceived;
    public event Action? DoneReceived;

    public async Task StartAsync(string baseUrl)
    {
        if (_connection != null)
            return; // already started

        // Build hub url from provided baseUrl (avoid hardcoded host/port)
        var hubUrl = (baseUrl ?? "").TrimEnd('/') + "/books-info-progress";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Server may send plain progress messages
        _connection.On<string>("ReceiveProgress", (message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        // Chat streaming uses chunk/done messages
        _connection.On<string>("ReceiveChunk", (chunk) =>
        {
            ChunkReceived?.Invoke(chunk);
        });

        _connection.On("ReceiveDone", () =>
        {
            DoneReceived?.Invoke();
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
}
