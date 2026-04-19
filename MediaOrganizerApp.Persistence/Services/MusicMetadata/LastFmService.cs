using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Services.LastFM;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.MusicMetadata;

public class LastFmService : ILastFmService
{
    private readonly LastfmClient _lastFMclient;

    public LastFmService(LastfmClient lastFMclient)
    {
        _lastFMclient = lastFMclient;
    }

    public async Task<List<ReleaseDto>> GetArtistReleasesAsync(string artist)
    {
        var response = await _lastFMclient.Artist.GetTopAlbumsAsync(artist);

        if (response.Status != LastResponseStatus.Successful || response.Content is null)
            return new List<ReleaseDto>();

        var tasks = response.Content.Select(async album =>
        {
            try
            {
                var info = await _lastFMclient.Album.GetInfoAsync(artist, album.Name);

                return new ReleaseDto
                {
                    Title = album.Name,
                    Year = info.Content?.ReleaseDateUtc?.Year,
                    CoverUrl = GetBestImage(album)
                };
            }
            catch
            {
                return new ReleaseDto
                {
                    Title = album.Name,
                    Year = null,
                    CoverUrl = GetBestImage(album)
                };
            }
        });

        return (await Task.WhenAll(tasks))
            .GroupBy(x => x.Title)   // remove duplicados comuns do Last.fm
            .Select(g => g.First())
            .ToList();
    }

    private string GetBestImage(LastAlbum album)
    {
        return album.Images?.Large?.AbsoluteUri
            ?? album.Images?.Medium?.AbsoluteUri
            ?? album.Images?.Small?.AbsoluteUri
            ?? "images/No-image-available.png";
    }
}