using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Services.LastFM;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.MusicMetadata;


// Application name	Media Organizer App
// API key a95fd7dd581211aff4ec8d60e1239a1a
// Shared secret ac6208640e5bd1fbe3f2bdb4f54e949d
// Registered to   fauxtixluix

public class LastFmService : ILastFmService
{
    private readonly LastfmClient _lastFMclient;
    private readonly HttpClient _http;
    private readonly ILogger<LastFmService> _logger;
    public LastFmService(LastfmClient lastFMclient, HttpClient http, ILogger<LastFmService> logger)
    {
        _lastFMclient = lastFMclient;
        _http = http;
        _logger = logger;
    }

    public async Task<List<ReleaseDto>> GetArtistReleasesAsync(string artist)
    {
        var response = await _lastFMclient.Artist.GetTopAlbumsAsync(artist);

        if (response.Status != LastResponseStatus.Successful || response.Content is null)
            return [];

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

        return [.. (await Task.WhenAll(tasks))
            .GroupBy(x => x.Title)   // remove duplicados comuns do Last.fm
            .Select(g => g.First())];
    }

    public async Task<string> GetAlbumAsync(string artist, string track, string apiKey)
    {
        artist = Clean(artist);
        track = Clean(track);

        //var albumName = await GetFromLastFm(artist, track);
        var album = await GetAlbumName(artist, track,  apiKey);
        if (!string.IsNullOrEmpty(album))
            return album;

        return null;
    }

    private async Task<string> GetFromLastFm(string artist, string track)
    {
        try
        {
            var url =
                $"http://ws.audioscrobbler.com/2.0/?method=track.getInfo" +
                $"&artist={Uri.EscapeDataString(artist)}&track={Uri.EscapeDataString(track)}&format=json";

            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("track", out var trackObj) &&
                trackObj.TryGetProperty("album", out var albumObj) &&
                albumObj.TryGetProperty("title", out var title))
            {
                return title.GetString();
            }
        }
        catch { }

        return null;
    }

    public async Task<string> GetAlbumName(string artist, string track, string apiKey)
    {
        string url = $"http://ws.audioscrobbler.com/2.0/?method=track.getInfo" +
                     $"&api_key={apiKey}" +
                     $"&artist={Uri.EscapeDataString(artist)}" +
                     $"&track={Uri.EscapeDataString(track)}" +
                     $"&format=json";
        try
        {
            var response = await _http.GetStringAsync(url);
            using JsonDocument doc = JsonDocument.Parse(response);

            // O Last.fm pode retornar músicas que não pertencem a um álbum específico (ex: singles)
            if (doc.RootElement.TryGetProperty("track", out var trackElement) &&
                trackElement.TryGetProperty("album", out var albumElement))
            {
                return albumElement.GetProperty("title").GetString();
            }

            return "";

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching album name from Last.fm for artist {Artist} and track {Track}", artist, track);
            return null;
        }
    }
    private static string GetBestImage(LastAlbum album) => album.Images?.Large?.AbsoluteUri
            ?? album.Images?.Medium?.AbsoluteUri
            ?? album.Images?.Small?.AbsoluteUri
            ?? "images/No-image-available.png";

    private static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // remove coisas comuns que estragam matching
        string[] remove = { "feat.", "ft.", "remaster", "remastered", "live", "(", ")" };

        foreach (var r in remove)
            input = input.Replace(r, "", StringComparison.OrdinalIgnoreCase);

        return input.Trim();
    }
}