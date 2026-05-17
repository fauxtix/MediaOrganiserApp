using MediaOrganizerApp.Domain.Entities.Media_Api_Models.AudioDb;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.DTOs.Api.AudioDb.AudioDbVM;

namespace MediaOrganizerApp.Persistence.Services.MusicMetadata
{
    public interface IAudioDbService
    {
        Task<AlbumDetails> GetAlbumDetailsAsync(
            string artistName,
            string albumName,
            CancellationToken ct = default);
    }

    public class AudioDbService : IAudioDbService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private readonly string _apiKey;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AudioDbService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            _apiKey = _config["AudioDb:ApiKey"]
                ?? throw new InvalidOperationException("AudioDb:ApiKey missing");

            _baseUrl = _config["AudioDb:BaseUrl"]
                ?? throw new InvalidOperationException("AudioDb:BaseUrl missing");
        }

        public async Task<AlbumDetails> GetAlbumDetailsAsync(
            string artistName,
            string albumName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(artistName) ||
                string.IsNullOrWhiteSpace(albumName))
            {
                return null;
            }

            var albumUrl =
                $"{_baseUrl}/{_apiKey}/searchalbum.php" +
                $"?s={Uri.EscapeDataString(artistName)}" +
                $"&a={Uri.EscapeDataString(albumName)}";

            var albumJson =
                await _httpClient.GetStringAsync(albumUrl, ct);

            var albumResponse =
                JsonSerializer.Deserialize<AlbumSearchResponse>(
                    albumJson,
                    JsonOptions);

            var album = albumResponse?.Album?.FirstOrDefault();

            if (album == null)
                return null;

            var tracks = new List<Track>();

            if (!string.IsNullOrWhiteSpace(album.IdAlbum))
            {
                var trackUrl =
                    $"{_baseUrl}/{_apiKey}/track.php?m={album.IdAlbum}";

                var trackJson =
                    await _httpClient.GetStringAsync(trackUrl, ct);

                var trackResponse =
                    JsonSerializer.Deserialize<TrackSearchResponse>(
                        trackJson,
                        JsonOptions);

                tracks = NormalizeTracks(trackResponse?.Track);
            }


            var totalMilliseconds =
                tracks.Sum(t =>
                    TryParseInt(t.IntDuration));


            return new AlbumDetails
            {
                Id = album.IdAlbum,
                Title = album.StrAlbum,
                Artist = album.StrArtist,

                CoverUrl =
                    album.StrAlbumThumbHQ ??
                    album.StrAlbumThumb,

                YearPublished =
                    TryParseIntNullable(album.IntYearReleased),

                Ranking =
                    TryParseDoubleNullable(album.IntScore),

                TotalDuration =
                    TimeSpan.FromMilliseconds(totalMilliseconds),

                Tracks = tracks
                    .OrderBy(t => TryParseInt(t.IntTrackNumber))
                    .Select(t => new AlbumTrack
                    {
                        TrackNumber = TryParseInt(t.IntTrackNumber),
                        Title = t.StrTrack,
                        Duration =
                            TimeSpan.FromMilliseconds(
                                TryParseInt(t.IntDuration))
                    })
                    .ToList()
            };
        }


        private static List<Track> NormalizeTracks(object raw)
        {
            if (raw == null)
                return new List<Track>();

            // Case 1: already list
            if (raw is List<Track> list)
                return list;

            // Case 2: single object returned instead of array
            if (raw is JsonElement element &&
                element.ValueKind == JsonValueKind.Object)
            {
                var single =
                    JsonSerializer.Deserialize<Track>(
                        element.GetRawText(),
                        JsonOptions);

                return single != null
                    ? new List<Track> { single }
                    : new List<Track>();
            }

            return new List<Track>();
        }

        private static int TryParseInt(string value)
            => int.TryParse(value, out var v) ? v : 0;

        private static int TryParseIntNullable(string value)
            => int.TryParse(value, out var v) ? v : 0;

        private static double? TryParseDoubleNullable(string value)
            => double.TryParse(value, out var v) ? v : null;
    }
}