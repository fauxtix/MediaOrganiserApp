using MediaOrganizerApp.Domain.Enums;

namespace MediaOrganizerApp.Application.DTOs
{
    public class PlaylistItemDto
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public MediaType MediaType { get; set; }

        public string ResourcePathOrUrl { get; set; } = default!;

        public string? Title { get; set; }

        public int SortOrder { get; set; }
    }
}
