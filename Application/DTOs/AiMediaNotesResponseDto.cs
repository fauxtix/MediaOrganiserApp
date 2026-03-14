using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs;

public sealed class AiMediaNotesResponseDto
{
    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("warnings")]
    public string? Warnings { get; set; }
}
