namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

public class CloudChatChunk
{
    public string Text { get; set; } = string.Empty;

    public string? Reasoning { get; set; }

    public string? Model { get; set; }

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public int? TotalTokens { get; set; }
}