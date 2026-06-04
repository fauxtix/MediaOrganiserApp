using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class PlaylistDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<PlaylistItemDto> Items { get; set; } = new();
    }
}
