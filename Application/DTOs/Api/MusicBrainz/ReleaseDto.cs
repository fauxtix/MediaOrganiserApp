namespace MediaOrganizerApp.Application.DTOs.Api.MusicBrainz
{
    public class ReleaseDto
    {
        public string Title { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string? CoverUrl { get; set; }
    }
}
