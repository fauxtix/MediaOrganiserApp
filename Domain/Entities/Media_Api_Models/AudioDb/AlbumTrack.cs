using System;

namespace MediaOrganizerApp.Domain.Entities.Media_Api_Models.AudioDb
{
    public class AlbumTrack
    {
        public int TrackNumber { get; set; }

        public string? Title { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
