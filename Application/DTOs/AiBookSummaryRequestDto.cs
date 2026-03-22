using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs;

public sealed class AiBookSummaryRequestDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }
    public string? ConnectionId { get; set; }

}
