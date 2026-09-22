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
    public string? CorrelationId { get; set; }
    public string? Type { get; set; }

    /// <summary>Optional context tag for AI usage logging (e.g. 'media-api').</summary>
    public string? Scenario { get; set; }
}
