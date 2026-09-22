namespace MediaOrganizerApp.Application.DTOs.AI;

using System.Text.Json.Serialization;

/// <summary>
/// Request payload sent from Blazor to Web API to generate (fallback) author profile info.
/// This should be used only when the external metadata API returns empty/not found.
/// </summary>
public sealed class AiAuthorProfileRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>Optional context tag for AI usage logging (e.g. 'author-api').</summary>
    public string? Scenario { get; set; }
}
