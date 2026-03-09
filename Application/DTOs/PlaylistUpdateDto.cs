namespace MediaOrganizerApp.Application.DTOs
{
    public class PlaylistUpdateDto
    {
        public string Name { get; set; } = default!;

        public string? Description { get; set; }
    }
}
