using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class VideoNotificationDto
    {
        public int Id { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}