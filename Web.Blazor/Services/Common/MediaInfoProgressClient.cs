using Microsoft.AspNetCore.SignalR.Client;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web.Blazor.Services.Common;

public sealed class MediaInfoProgressClient
{
    private HubConnection? _connection;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MediaInfoProgressClient> _logger;

    public MediaInfoProgressClient(IConfiguration configuration, ILogger<MediaInfoProgressClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public event Action<string>? ProgressReceived;
    public event Action<ProgressUpdateDto>? ProgressEventReceived;

    public async Task StartAsync(string apiBaseUrl, string? apiKey = null)
    {
        if (_connection != null)
            return;

        var hubUrl = $"{apiBaseUrl.TrimEnd('/')}/media-info-progress";
        var key = string.IsNullOrWhiteSpace(apiKey) ? _configuration["ApiKey"] : apiKey;

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrWhiteSpace(key))
                    options.Headers["ApiKey"] = key;
            })
            .WithAutomaticReconnect()
            .Build();

        // Server sends: ReceiveProgress(connectionId, message)
        _connection.On<string, string>("ReceiveProgress", (_, message) =>
        {
            ProgressReceived?.Invoke(message);
        });

        _connection.On<ProgressUpdateDto>("ReceiveProgressEvent", update =>
        {
            ProgressEventReceived?.Invoke(update);
            ProgressReceived?.Invoke(update.Message);
        });

        await _connection.StartAsync();
    }

    public async Task JoinCorrelationAsync(string correlationId)
    {
        await InvokeIfConnectedAsync("JoinCorrelation", correlationId);
    }

    public async Task LeaveCorrelationAsync(string correlationId)
    {
        await InvokeIfConnectedAsync("LeaveCorrelation", correlationId);
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
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the connection is not active.", methodName);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the connection was disposed.", methodName);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Skipping hub invoke '{MethodName}' because the operation was canceled.", methodName);
        }
    }
}
