using MediaOrganizerApp.Domain.Enums;

namespace MediaOrganizerApp.Application.DTOs.PlayLists
{
    public class PlaylistCreateDto
    {
        public string Name { get; set; } = default!;

        public string? Description { get; set; }
        public MediaType  MediaType { get; set; }
    }
}
