using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    /// <summary>
    /// Key/value settings store for the OpenRouter cloud chat (AIChatSettings table).
    /// UserId NULL = global (admin-managed); non-null = per-user override.
    /// </summary>
    public interface IAIChatSettingsRepository
    {
        Task<IEnumerable<AIChatSetting>> GetAllAsync(string? userId, bool isAdmin, CancellationToken ct = default);
        Task<string?> GetValueAsync(string chave, string? userId, CancellationToken ct = default);
        Task SetValueAsync(string chave, string valor, string? userId, CancellationToken ct = default);
        Task DeleteValueAsync(string chave, string? userId, CancellationToken ct = default);
    }
}