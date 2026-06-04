using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class YoutubeVideoDto
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateOnly PublishedAt { get; set; }
    }
}