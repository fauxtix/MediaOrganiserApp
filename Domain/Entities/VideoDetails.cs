using System;

namespace MediaOrganizerApp.Domain.Entities
{
    public class YouTubeVideoDetails
    {
        public string VideoId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string ChannelTitle { get; set; }
        public string Duration { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Thumbnail { get; set; }
    }
}
