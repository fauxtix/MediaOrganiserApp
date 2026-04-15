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
}
