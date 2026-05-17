using MediaOrganizerApp.Application.DTOs.Api.AudioDb;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IAudioDbServiceWrapper
    {
        Task<AlbumDetailsDto?> GetAlbumDetailsAsync(string artist, string album);
    }
}