namespace MediaOrganizerApp.Application.DTOs.AI
{
    public class ConversationCreateDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Json { get; set; } = "[]";
    }
}