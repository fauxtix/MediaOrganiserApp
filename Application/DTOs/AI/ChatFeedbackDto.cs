using System;

namespace MediaOrganizerApp.Application.DTOs.AI
{
    /// <summary>
    /// User feedback (like/dislike, as in ChatGPT) on an assistant response in
    /// the cloud (OpenRouter) or local (Ollama) chat. Persisted in ChatFeedback,
    /// keyed by (UserID, Interface, ConversationId, MessageIndex).
    /// Rating: 1 = agree/correct, -1 = disagree/incorrect, 0 = cleared (the
    /// unmarking is persisted: the row stays with Rating 0; responses never
    /// voted have no row).
    /// </summary>
    public class ChatFeedbackDto
    {
        public int? Id { get; set; }
        public string? UserId { get; set; }
        public string Interface { get; set; } = string.Empty;  // 'openrouter' | 'ollama'
        public string? ConversationId { get; set; }
        public int MessageIndex { get; set; }
        public string? PromptText { get; set; }
        public string ResponseText { get; set; } = string.Empty;
        public string? ModelName { get; set; }
        public int Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}