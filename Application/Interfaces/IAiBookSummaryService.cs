using MediaOrganizerApp.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces;

public interface IAiBookSummaryService
{
    Task<Application.DTOs.AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken);

    Task<Application.DTOs.AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken);

    Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
            string title,
            string? author,
            string language,
            string? connectionId,
             CancellationToken cancellationToken);
}
