using MediaOrganizerApp.Domain.Enums;

namespace MediaOrganizerApp.Application.DTOs
{
    public class PlaylistItemDto
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public int MediaId { get; set; }

        public MediaType MediaType { get; set; }

        public string ResourcePathOrUrl { get; set; } = default!;

        public string? Title { get; set; }
        public string? Author { get; set; }
        public string TrackTime { get; set; }
        public string CategoriaMedia { get; set; }
        public int SortOrder { get; set; }
    }
}
