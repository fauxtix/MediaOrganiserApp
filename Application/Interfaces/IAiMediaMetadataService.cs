using MediaOrganizerApp.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces;

public interface IAiMediaMetadataService
{
    Task<AiMediaMetadataResponseDto> GetMediaMetadataAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken);

    Task<string?> GetMediaEditionYearAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken);
}
