using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class AiBookSummaryRequestDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
    public string? ConnectionId { get; set; }
    public string? CorrelationId { get; set; }

    /// <summary>Optional context tag for AI usage logging (e.g. 'googlebooks', 'cloud-fallback').</summary>
    public string? Scenario { get; set; }
}
