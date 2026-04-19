using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IMusicApiClient
    {
        Task<ArtistReleasesResponseDto?> GetLastFMArtistReleasesAsync(string artistName);
    }
}
