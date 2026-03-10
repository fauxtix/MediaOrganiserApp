using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IPlaylistItemService
    {
        Task<IEnumerable<PlaylistItem>> GetByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default);

        Task<IEnumerable<PlaylistItemDto>> GetItemDtosByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default);

        Task<PlaylistItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<int?> AddAsync(PlaylistItem item, CancellationToken cancellationToken = default);

        Task UpdateAsync(PlaylistItem item, CancellationToken cancellationToken = default);
        Task UpdateOrderAsync(int playlistId, IEnumerable<PlaylistItemOrderUpdateDto> items, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
