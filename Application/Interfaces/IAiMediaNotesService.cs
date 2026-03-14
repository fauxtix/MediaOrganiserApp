using MediaOrganizerApp.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces;

public interface IAiMediaNotesService
{
    Task<AiMediaNotesResponseDto> GetMediaNotesAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken);
}
