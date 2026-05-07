using MediaOrganizerApp.Application.DTOs.AI;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

public interface IAiBookSummaryService
{
    Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken);

    Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken);

    Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
            string title,
            string? author,
            string language,
            string? connectionId,
            string? correlationId,
             CancellationToken cancellationToken);
}
