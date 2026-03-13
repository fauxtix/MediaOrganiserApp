using MediaOrganizerApp.Application.DTOs;

namespace Web.Blazor.Pages.Publications;

public sealed class GoogleBooksSearchResult
{
    public PubsWithDescriptions Record { get; set; } = new();

    public string? AiNotes { get; set; }

    public double? AiConfidence { get; set; }

    public bool UsedAi { get; set; }

    public bool UsedOfflineFallback { get; set; }
}
