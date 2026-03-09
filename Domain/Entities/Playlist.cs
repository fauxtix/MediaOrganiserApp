using System;

namespace MediaOrganizerApp.Domain.Entities
{
    public class Playlist
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
