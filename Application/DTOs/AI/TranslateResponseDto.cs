using System.Text.Json.Serialization;

namespace MediaOrganizerApp.Application.DTOs.AI;

public sealed class TranslateResponseDto
{
    [JsonPropertyName("translatedText")]
    public string TranslatedText { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Provider that actually produced the answer ("openrouter" | "ollama"). Lets the
    /// UI explain a slow translation that fell back to the local provider.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Model that actually served the answer. For OpenRouter this is the model
    /// reported by the response body, which differs from the requested one when the
    /// <c>openrouter/free</c> router is in play.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}
