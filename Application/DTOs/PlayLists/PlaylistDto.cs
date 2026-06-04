using MediaOrganizerApp.Domain.Enums;
using System;

namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class PlaylistDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public MediaType MediaType { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
