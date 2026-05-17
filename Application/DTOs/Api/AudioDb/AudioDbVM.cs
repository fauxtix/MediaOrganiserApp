using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.Api.AudioDb
{
    public class AudioDbVM
    {
        public class AlbumSearchResponse
        {
            [JsonPropertyName("album")]
            public List<Album>? Album { get; set; }
        }

        public class TrackSearchResponse
        {
            [JsonPropertyName("track")]
            public List<Track>? Track { get; set; }
        }

        public class Album
        {
            [JsonPropertyName("idAlbum")]
            public string? IdAlbum { get; set; }

            [JsonPropertyName("strAlbum")]
            public string? StrAlbum { get; set; }

            [JsonPropertyName("strArtist")]
            public string? StrArtist { get; set; }

            [JsonPropertyName("intYearReleased")]
            public string? IntYearReleased { get; set; }

            [JsonPropertyName("strAlbumThumb")]
            public string? StrAlbumThumb { get; set; }

            [JsonPropertyName("strAlbumThumbHQ")]
            public string? StrAlbumThumbHQ { get; set; }

            [JsonPropertyName("intScore")]
            public string? IntScore { get; set; }
        }

        public class Track
        {
            [JsonPropertyName("strTrack")]
            public string? StrTrack { get; set; }

            [JsonPropertyName("intTrackNumber")]
            public string? IntTrackNumber { get; set; }

            [JsonPropertyName("intDuration")]
            public string? IntDuration { get; set; }
        }

    }
}
