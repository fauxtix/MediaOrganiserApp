using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories;

/// <summary>
/// AI usage logging persistence (AIUsageLog table), isolated per user.
/// </summary>
public interface IAiUsageLogRepository
{
    Task InsertAsync(AiUsageLog log, CancellationToken ct = default);

    Task<AiUsageSummaryDto> GetSummaryAsync(
        string? userId = null,
        bool isAdmin = false,
        CancellationToken ct = default);
}