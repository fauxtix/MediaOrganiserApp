using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

public class CloudChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}