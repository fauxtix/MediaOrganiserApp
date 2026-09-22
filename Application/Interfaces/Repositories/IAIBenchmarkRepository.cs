using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    /// <summary>
    /// Auto-recorded benchmark persistence (AIPrompts + AIResponses tables).
    /// Rows are isolated per user.
    /// </summary>
    public interface IAIBenchmarkRepository
    {
        Task<int> GetOrCreatePromptIdAsync(
            string textoPrompt,
            string userId,
            string descricao,
            double temperatura,
            CancellationToken ct = default);

        Task CreateResponseAsync(AIResponse response, CancellationToken ct = default);

        Task<AiBenchmarkSummaryDto> GetBenchmarkSummaryAsync(
            string? userId = null,
            bool isAdmin = false,
            CancellationToken ct = default);
    }
}