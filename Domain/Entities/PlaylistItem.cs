using MediaOrganizerApp.Domain.Enums;
using System;

namespace MediaOrganizerApp.Domain.Entities
{
    public class PlaylistItem
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }

        public MediaType MediaType { get; set; }

        /// <summary>
        /// Referência ao recurso:
        /// - Para MP3: caminho local (path)
        /// - Para MP4/YouTube: URL
        /// </summary>
        public string ResourcePathOrUrl { get; set; } = default!;

        public string? Title { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
