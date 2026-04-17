// MyMusic.Api/Services/MusicMetadataService.cs
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.MusicMetadata;
public class MusicMetadataService : IMusicMetadataService
{
    private readonly HttpClient _http;

    public MusicMetadataService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("MyMusicApp/1.0 ( your-email@example.com )");
    }

    public async Task<ArtistReleasesResponseDto> GetArtistReleasesAsync(ArtistReleasesRequestDto request)
    {
        var response = new ArtistReleasesResponseDto
        {
            ArtistName = request.ArtistName
        };

        // 1) Procurar artista
        var artistSearchUrl =
            $"https://musicbrainz.org/ws/2/artist?query={Uri.EscapeDataString(request.ArtistName)}&fmt=json&limit=1";

        using var artistResult = await _http.GetAsync(artistSearchUrl);
        artistResult.EnsureSuccessStatusCode();

        using var artistDoc = JsonDocument.Parse(await artistResult.Content.ReadAsStringAsync());
        var artistId = artistDoc.RootElement
            .GetProperty("artists")[0]
            .GetProperty("id")
            .GetString();

        if (artistId is null)
            return response;

        // 2) Buscar releases do artista
        var releasesUrl =
            $"https://musicbrainz.org/ws/2/release?artist={artistId}&fmt=json&limit=20";

        using var releasesResult = await _http.GetAsync(releasesUrl);
        releasesResult.EnsureSuccessStatusCode();

        using var releasesDoc = JsonDocument.Parse(await releasesResult.Content.ReadAsStringAsync());
        var releases = releasesDoc.RootElement.GetProperty("releases");

        foreach (var rel in releases.EnumerateArray())
        {
            var title = rel.GetProperty("title").GetString() ?? "";
            int? year = null;

            if (rel.TryGetProperty("date", out var dateProp))
            {
                var dateStr = dateProp.GetString();
                if (!string.IsNullOrWhiteSpace(dateStr) &&
                    int.TryParse(dateStr[..4], out var y))
                {
                    year = y;
                }
            }

            var mbid = rel.GetProperty("id").GetString();

            string? coverUrl = null;
            if (!string.IsNullOrEmpty(mbid))
            {
                // 3) Cover Art Archive
                var caaUrl = $"https://coverartarchive.org/release/{mbid}/front-250";
                var coverResponse = await _http.GetAsync(caaUrl);
                if (coverResponse.IsSuccessStatusCode)
                {
                    // redireciona para a imagem final
                    coverUrl = coverResponse.RequestMessage?.RequestUri?.ToString();
                }
            }

            response.Releases.Add(new ReleaseDto
            {
                Title = title,
                Year = year,
                CoverUrl = coverUrl
            });
        }

        return response;
    }
}
