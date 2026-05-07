using Microsoft.AspNetCore.SignalR.Client;
using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.Common;

public sealed class MediaInfoProgressClient
{
    private HubConnection? _connection;
    private readonly IConfiguration _configuration;

    public MediaInfoProgressClient(IConfiguration configuration)
    {
        _configuration = configuration;
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
