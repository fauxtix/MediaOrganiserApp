using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.LastFM
{
    public interface ILastFmService
    {
        Task<List<ReleaseDto>> GetArtistReleasesAsync(string artist);
    }
}
