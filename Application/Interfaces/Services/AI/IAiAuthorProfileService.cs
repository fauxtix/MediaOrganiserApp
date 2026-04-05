using MediaOrganizerApp.Application.DTOs.AI;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

/// <summary>
/// Server-side AI service for generating author profile information.
/// This should be used only as a fallback when external metadata APIs fail.
/// </summary>
public interface IAiAuthorProfileService
{
    Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, string language, CancellationToken cancellationToken = default);
}
