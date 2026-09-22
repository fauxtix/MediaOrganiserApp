using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class TranslateRequestDto
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("sourceLanguage")]
    public string SourceLanguage { get; set; } = string.Empty;

    [JsonPropertyName("targetLanguage")]
    public string TargetLanguage { get; set; } = string.Empty;

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>Optional context tag for AI usage logging (e.g. 'cloud-fallback', 'direct').</summary>
    [JsonPropertyName("scenario")]
    public string? Scenario { get; set; }
}
