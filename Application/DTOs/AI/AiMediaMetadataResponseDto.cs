using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class AiMediaMetadataResponseDto
{
    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    // DVD/movie-ish fields
    [JsonPropertyName("runtime")]
    public string? Runtime { get; set; }

    [JsonPropertyName("released")]
    public string? Released { get; set; }

    [JsonPropertyName("imdbRating")]
    public string? ImdbRating { get; set; }

    [JsonPropertyName("actors")]
    public string? Actors { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("poster")]
    public string? Poster { get; set; }

    // Shared
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    // Album-ish fields
    [JsonPropertyName("tempo")]
    public string? Tempo { get; set; }

    [JsonPropertyName("coverFile")]
    public string? CoverFile { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("warnings")]
    public string? Warnings { get; set; }

    // Some models may return the singular field ("warning") even when asked for ("warnings").
    // Keep as an alias so we can inspect/capture it without breaking parsing.
    [JsonPropertyName("warning")]
    public string? Warning { get; set; }
}
