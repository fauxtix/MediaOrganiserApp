namespace MediaOrganizerApp.Application.DTOs.AI;

/// <summary>
/// AI-generated author profile response (fallback).
/// Treat this as best-effort content; it may contain uncertainty and should be user-editable.
/// </summary>
public sealed class AiAuthorProfileResponseDto
{
    public string? Bio { get; set; }

    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Optional confidence score (0..1) returned by the model/prompt contract.
    /// </summary>
    public double? Confidence { get; set; }

    /// <summary>
    /// Notes about ambiguity, uncertainty, etc.
    /// </summary>
    public string? Notes { get; set; }
}
