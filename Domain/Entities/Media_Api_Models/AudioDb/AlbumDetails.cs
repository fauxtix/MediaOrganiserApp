using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Domain.Entities.Media_Api_Models.AudioDb
{
    public class AlbumDetails
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public string? Artist { get; set; }

        public int? YearPublished { get; set; }

        public string? CoverUrl { get; set; }

        public double? Ranking { get; set; }

        public TimeSpan TotalDuration { get; set; }

        public List<AlbumTrack> Tracks { get; set; } = new();
    }
}
