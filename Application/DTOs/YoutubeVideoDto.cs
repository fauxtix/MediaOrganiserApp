using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class YoutubeVideoDto
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateOnly PublishedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationFormatted =>
            Duration.TotalHours >= 1
                ? Duration.ToString(@"h\:mm\:ss")
                : Duration.ToString(@"m\:ss");
    }
}