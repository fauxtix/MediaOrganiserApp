using System;

namespace MediaOrganizerApp.Application.DTOs.AI
{
    public class ConversationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Json { get; set; } = "[]";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UserId { get; set; }
    }
}