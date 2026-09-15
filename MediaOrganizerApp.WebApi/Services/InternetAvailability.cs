using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Detects whether the machine currently has internet access by attempting
/// short TCP connects to well-known public endpoints. Used before any
/// OpenRouter call so the app can jump straight to the Ollama fallback
/// without wasting time offline.
/// </summary>
public interface IInternetAvailabilityService
{
    Task<bool> IsReachableAsync(CancellationToken ct = default);
}

public sealed class InternetAvailability : IInternetAvailabilityService
{
    private static readonly (string Host, int Port)[] Probes =
    {
        ("1.1.1.1", 443),
        ("8.8.8.8", 443),
        ("openrouter.ai", 443)
    };

    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan PositiveTtl = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan NegativeTtl = TimeSpan.FromSeconds(10);

    private readonly ILogger<InternetAvailability> _logger;
    private readonly SemaphoreSlim _sync = new(1, 1);

    private bool? _cached;
    private DateTime _cachedAtUtc = DateTime.MinValue;

    public InternetAvailability(ILogger<InternetAvailability> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsReachableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (TryReadCache(out var cached))
            return cached;

        await _sync.WaitAsync(ct);
        try
        {
            if (TryReadCache(out cached))
                return cached;

            foreach (var probe in Probes)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(probe.Host, probe.Port);
                    var finished = await Task.WhenAny(connectTask, Task.Delay(ProbeTimeout, ct));
                    if (finished != connectTask)
                        continue;

                    try
                    {
                        await connectTask;
                    }
                    catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
                    {
                        _logger.LogDebug(ex, "Probe {Host}:{Port} falhou.", probe.Host, probe.Port);
                        continue;
                    }

                    _cached = true;
                    _cachedAtUtc = DateTime.UtcNow;
                    return true;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
            }

            _cached = false;
            _cachedAtUtc = DateTime.UtcNow;
            _logger.LogDebug("Internet indisponível: todos os probes falharam.");
            return false;
        }
        finally
        {
            _sync.Release();
        }
    }

    private bool TryReadCache(out bool value)
    {
        if (_cached.HasValue)
        {
            var ttl = _cached.Value ? PositiveTtl : NegativeTtl;
            if (DateTime.UtcNow - _cachedAtUtc < ttl)
            {
                value = _cached.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}