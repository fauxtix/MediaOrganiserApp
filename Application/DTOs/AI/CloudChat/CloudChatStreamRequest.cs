namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

/// <summary>
/// Wraps the payload streamed to the OpenRouter cloud chat proxy with the
/// language of the UI session (used for auto-recorded benchmark rows).
/// </summary>
public class CloudChatStreamRequest
{
    public CloudChatPayload Payload { get; set; } = new();

    public string? Language { get; set; }
}