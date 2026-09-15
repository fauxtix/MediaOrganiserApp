namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

/// <summary>Non-sensitive cloud chat settings readable by any authenticated user.</summary>
public class CloudChatRuntimeSettingsDto
{
    public string DefaultModel { get; set; } = "openrouter/free";
    public bool EnableReasoning { get; set; }
}