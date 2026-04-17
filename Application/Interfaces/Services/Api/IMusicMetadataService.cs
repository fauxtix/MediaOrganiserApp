using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IMusicMetadataService
    {
        Task<ArtistReleasesResponseDto> GetArtistReleasesAsync(ArtistReleasesRequestDto request);
    }
}
