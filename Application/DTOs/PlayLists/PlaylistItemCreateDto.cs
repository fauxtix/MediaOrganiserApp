using MediaOrganizerApp.Domain.Enums;

namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class PlaylistItemCreateDto
    {
        public int PlaylistId { get; set; }
        public int MediaId { get; set; }
        public MediaType MediaType { get; set; }

        public string ResourcePathOrUrl { get; set; } = default!;

        public string? Title { get; set; }

        /// <summary>
        /// Opcional: se não vier, o backend coloca no fim da lista.
        /// </summary>
        public int? SortOrder { get; set; }
    }
}
