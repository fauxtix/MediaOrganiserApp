namespace MediaOrganizerApp.Application.DTOs.AI;

/// <summary>
/// Streamed chat chunk for the local (Ollama) chat endpoint. Text carries the
/// assistant answer; Reasoning carries the model's thinking/raciocínio when the
/// global <c>CloudChat:EnableReasoning</c> toggle is ON and the model produces it.
/// </summary>
public sealed class AiChatChunk
{
    public string Text { get; set; } = string.Empty;

    public string? Reasoning { get; set; }
}