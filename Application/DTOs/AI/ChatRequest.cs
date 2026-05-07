using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.AI
{
    public class ChatRequest
    {
        public List<ChatMessageDto> Messages { get; set; } = new();
        public string? ConnectionId { get; set; }
        public string? CorrelationId { get; set; }
        public string? Language { get; set; } // new - e.g. "en-US", "en", "pt-PT"
    }
}
