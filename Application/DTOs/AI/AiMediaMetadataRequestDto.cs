using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class AiMediaMetadataRequestDto
{
    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    public string? ConnectionId { get; set; }
}
