namespace MediaOrganizerApp.Application.DTOs;

public class MediaEditionYearRequestDto
{
    public string Artist { get; set; }
    public string Title { get; set; }
    public string MediaType { get; set; }
    public string Language { get; set; }
    public string? ConnectionId { get; set; }
    public string? CorrelationId { get; set; }

    /// <summary>Optional context tag for AI usage logging (e.g. 'media-api').</summary>
    public string? Scenario { get; set; }
}
