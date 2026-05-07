using Microsoft.AspNetCore.SignalR.Client;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.Common;

public class BooksInfoProgressClient
{
    private HubConnection? _connection;
    private readonly IConfiguration _configuration;

    public BooksInfoProgressClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public event Action<string>? ProgressReceived;
    public event Action<ProgressUpdateDto>? ProgressEventReceived;
    public event Action<string>? ChunkReceived;
    public event Action? DoneReceived;

    public async Task StartAsync(string baseUrl, string? apiKey = null)
    {
        if (_connection != null)
            return; // already started

        // Build hub url from provided baseUrl (avoid hardcoded host/port)
        var hubUrl = (baseUrl ?? "").TrimEnd('/') + "/books-info-progress";
        var key = string.IsNullOrWhiteSpace(apiKey) ? _configuration["ApiKey"] : apiKey;

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrWhiteSpace(key))
                    options.Headers["ApiKey"] = key;
            })
            .WithAutomaticReconnect()
            .Build();

        // Server may send plain progress messages
        _connection.On<string>("ReceiveProgress", (message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        // backward compatibility with previous 2-arg payload
        _connection.On<string, string>("ReceiveProgress", (_, message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        _connection.On<ProgressUpdateDto>("ReceiveProgressEvent", update =>
        {
            ProgressEventReceived?.Invoke(update);
            ProgressReceived?.Invoke(update.Message);
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

    public async Task JoinCorrelationAsync(string correlationId)
    {
        if (_connection == null || string.IsNullOrWhiteSpace(correlationId))
            return;
        await _connection.InvokeAsync("JoinCorrelation", correlationId);
    }

    public async Task LeaveCorrelationAsync(string correlationId)
    {
        if (_connection == null || string.IsNullOrWhiteSpace(correlationId))
            return;
        await _connection.InvokeAsync("LeaveCorrelation", correlationId);
    }
}
