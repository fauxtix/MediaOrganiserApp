using MediaOrganizerApp.Domain.Enums;

namespace MediaOrganizerApp.Application.DTOs
{
    public class PlaylistItemUpdateDto
    {
        public MediaType MediaType { get; set; }

        public string ResourcePathOrUrl { get; set; } = default!;

        public string? Title { get; set; }

        public int SortOrder { get; set; }
    }
}
