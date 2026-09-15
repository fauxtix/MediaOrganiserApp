using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

/// <summary>
/// Tracks OpenRouter usage consumed by the cloud chat: credit balance/usage via
/// the /key and /credits endpoints, and a local daily counter for free-model
/// requests (the OpenRouter platform does not expose a "free requests remaining
/// today" endpoint; it only reports it via X-RateLimit-* headers on 429s).
/// </summary>
public interface ICloudChatUsageService
{
    Task<CloudChatUsageDto> GetUsageAsync(CancellationToken ct = default);
    Task RegisterFreeModelUsageAsync(CancellationToken ct = default);
    Task SyncFreeUsageFromHeadersAsync(int cap, int remaining, CancellationToken ct = default);
}