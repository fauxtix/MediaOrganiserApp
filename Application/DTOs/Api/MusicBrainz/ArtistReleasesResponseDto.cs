using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.Api.MusicBrainz
{
    public class ArtistReleasesResponseDto
    {
        public string ArtistName { get; set; } = string.Empty;
        public List<ReleaseDto> Releases { get; set; } = new();
    }
}
