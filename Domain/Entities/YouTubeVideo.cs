using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Domain.Entities
{
    public class YouTubeVideo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ChannelId { get; set; }

        public string VideoId { get; set; }

        public string VideoTitle { get; set; }

        public string VideoDescription { get; set; }

        public DateTime? PublishedAt { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
