using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories;

public interface IPlaylistItemRepository
{
    Task<IEnumerable<PlaylistItem>> GetByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default);

    Task<PlaylistItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PlaylistItem> InsertAsync(PlaylistItem item, CancellationToken cancellationToken = default);

    Task UpdateAsync(PlaylistItem item, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}