using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.Api.AudioDb
{
    public class AlbumDetailsDto
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public string? Artist { get; set; }

        public int? YearPublished { get; set; }

        public string? CoverUrl { get; set; }

        public double? Ranking { get; set; }

        public TimeSpan TotalDuration { get; set; }

        public List<AlbumTrackDto> Tracks { get; set; } = new();
    }

    public class AlbumTrackDto
    {
        public int TrackNumber { get; set; }

        public string? Title { get; set; }

        public TimeSpan Duration { get; set; }
    }
}