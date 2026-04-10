using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using MediaOrganizerApp.Application.DTOs.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common;

public class ChatProgressClient
{
    private HubConnection? _connection;
    private volatile bool _isReconnecting = false;
    private readonly ILogger<ChatProgressClient>? _logger;
    private string? _hubUrl;
    private string? _lastConnectionId;
    public event Action<string>? ProgressReceived;
    public event Action<string>? ChunkReceived;
    public event Action? DoneReceived;

    public bool IsReconnecting => _isReconnecting;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    public event Action<string?>? Reconnected;
    public event Action? Reconnecting;
    public event Action<Exception?>? Closed;

        public ChatProgressClient(ILogger<ChatProgressClient>? logger = null)
        {
            _logger = logger;
        }

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
            // remember base host for reconnect attempts (avoid double appending)
            _hubUrl = baseHost;

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveProgress", (message) =>
        {
            try { _logger?.LogDebug("[ChatProgressClient] ReceiveProgress: {Message}", message); } catch { }
            ProgressReceived?.Invoke(message);
        });

        _connection.On<string>("ReceiveChunk", (chunk) =>
        {
            try
            {
                _logger?.LogDebug("[ChatProgressClient] ReceiveChunk: {Preview}",
                                    chunk == null ? "<null>" : chunk.Substring(0, Math.Min(50, chunk.Length)));
            }
            catch { }
            ChunkReceived?.Invoke(chunk);
        });

        _connection.On("ReceiveDone", () =>
        {
            try { _logger?.LogDebug("[ChatProgressClient] ReceiveDone"); } catch { }
            DoneReceived?.Invoke();
        });

        _connection.Reconnecting += (ex) =>
        {
            _isReconnecting = true;
            // capture last known connection id so we can reattach server-side processing after reconnect
            try { _lastConnectionId = _connection?.ConnectionId; } catch { }
            try { _logger?.LogWarning("[ChatProgressClient] Reconnecting: {Message}", ex?.Message); } catch { }
            try { Reconnecting?.Invoke(); } catch { }
            return Task.CompletedTask;
        };

        _connection.Reconnected += async (newId) =>
        {
            _isReconnecting = false;
            try { _logger?.LogInformation("[ChatProgressClient] Reconnected: {ConnectionId}", newId); } catch { }
            try { Reconnected?.Invoke(newId); } catch { }

            // Notify server to reattach any background processing from the previous connection id
            try
            {
                if (!string.IsNullOrWhiteSpace(_lastConnectionId) && _lastConnectionId != newId)
                {
                    try { await _connection.InvokeAsync("Reattach", _lastConnectionId); } catch { }
                }
            }
            catch { }

            // async lambda returns implicitly
        };

        // Ensure handlers are attached before sending any StartChat requests.
        // Expose an explicit marker to indicate handlers are registered.
        try { _logger?.LogDebug("[ChatProgressClient] Handlers registered"); } catch { }

        // Increase timeouts to tolerate longer server processing before first message
        try
        {
            _connection.ServerTimeout = TimeSpan.FromSeconds(120);
            _connection.KeepAliveInterval = TimeSpan.FromSeconds(15);
        }
        catch { }

        _connection.Closed += (ex) =>
        {
            _isReconnecting = false;
            try { _logger?.LogWarning("[ChatProgressClient] Closed: {Message}", ex?.Message); } catch { }
            try { Closed?.Invoke(ex); } catch { }

            // Try to reconnect in background using the stored hubUrl. This helps
            // tolerate transient network issues without requiring a full page reload.
            var hubToRestart = _hubUrl;
            _ = Task.Run(async () =>
            {
                try
                {
                    // Null out existing connection so StartAsync will create a fresh one.
                    try { _connection = null; } catch { }

                    var attempts = 0;
                    while (!string.IsNullOrWhiteSpace(hubToRestart) && (_connection == null || _connection.State != HubConnectionState.Connected) && attempts < 10)
                    {
                        attempts++;
                        try
                        {
                            await Task.Delay(1000 * Math.Min(attempts, 5));
                            await StartAsync(hubToRestart);
                            if (_connection != null && _connection.State == HubConnectionState.Connected)
                            {
                                try { _logger?.LogInformation("[ChatProgressClient] Reconnected after closed"); } catch { }
                                break;
                            }
                        }
                        catch (Exception ex2)
                        {
                            try { _logger?.LogDebug(ex2, "[ChatProgressClient] reconnect attempt failed"); } catch { }
                        }
                    }
                }
                catch { }
            });

            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            try { _logger?.LogInformation("[ChatProgressClient] Started connection {ConnectionId} -> {HubBase}", _connection.ConnectionId, _hubUrl); } catch { }
        }
        catch (Exception ex)
        {
            try { _logger?.LogWarning(ex, "[ChatProgressClient] StartAsync failed"); } catch { }
            // ensure caller sees a clean state
            try { await _connection.DisposeAsync(); } catch { }
            _connection = null;
        }
    }

    public async Task StopAsync()
    {
        // Capture the connection to avoid a race where _connection becomes null
        // between the null-check and the awaited operations.
        var conn = _connection;
        if (conn == null)
            return;

        // Null out the shared reference so other callers see it's stopped.
        try { _connection = null; } catch { }

        try { await conn.StopAsync(); } catch { }
        try { await conn.DisposeAsync(); } catch { }
    }

    public string? ConnectionId => _connection?.ConnectionId;

        public async Task<bool> StartChatAsync(List<ChatMessageDto> messages, string? language = null)
        {
            if (_connection == null || _connection.State != HubConnectionState.Connected)
            {
                try { _logger?.LogWarning("[ChatProgressClient] StartChatAsync: connection not ready"); } catch { }
                return false;
            }

            try { _logger?.LogInformation("[ChatProgressClient] StartChatAsync invoking for ConnectionId={ConnectionId} Messages={Count}", _connection.ConnectionId, messages?.Count ?? 0); } catch { }

            try
            {
                await _connection.InvokeAsync("StartChat", messages, language);
            _logger?.LogInformation("[ChatProgressClient] StartChatAsync succeeded");
            return true;
            }
            catch (Exception ex)
            {
                try { _logger?.LogError(ex, "[ChatProgressClient] StartChatAsync failed"); } catch { }
                return false;
            }
        }

        public async Task CancelChatAsync()
        {
            if (_connection == null) return;
            try
            {
                await _connection.InvokeAsync("CancelChat");
            _logger?.LogInformation("[ChatProgressClient] CancelChatAsync canceled");

        }
        catch (Exception ex)
            {
                try { _logger?.LogError(ex, "[ChatProgressClient] CancelChatAsync failed"); } catch { }
            }
        }
}
