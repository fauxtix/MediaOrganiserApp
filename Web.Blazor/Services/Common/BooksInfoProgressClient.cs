using Microsoft.AspNetCore.SignalR.Client;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web.Blazor.Services.Common;

public class BooksInfoProgressClient
{
    private HubConnection? _connection;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BooksInfoProgressClient> _logger;

    public BooksInfoProgressClient(IConfiguration configuration, ILogger<BooksInfoProgressClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
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

        // Backward compatibility: older server messages sent (connectionId, message).
        // The first parameter is ignored on the client because this connection already scopes the stream.
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
                if (_connection != null)
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
        await InvokeIfConnectedAsync("JoinCorrelation", correlationId);
    }

    public async Task LeaveCorrelationAsync(string correlationId)
    {
        // Best-effort cleanup: if connection is not active, skip and avoid surfacing UI errors.
        await InvokeIfConnectedAsync("LeaveCorrelation", correlationId);
    }

    private async Task InvokeIfConnectedAsync(string methodName, string correlationId)
    {
        if (_connection == null || string.IsNullOrWhiteSpace(correlationId))
            return;

        if (_connection.State != HubConnectionState.Connected)
        {
            _logger.LogDebug("Skipping hub invoke '{MethodName}' because connection state is {ConnectionState}.", methodName, _connection.State);
            return;
        }

        try
        {
            await _connection.InvokeAsync(methodName, correlationId);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the connection was disposed.", methodName);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the operation was canceled.", methodName);
        }

        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the connection is not active.", methodName);
        }
    }
}
