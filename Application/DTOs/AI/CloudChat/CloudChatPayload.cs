using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI.CloudChat;

public class CloudChatPayload
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<CloudChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonIgnore]
    public bool EnableReasoning { get; set; }
}