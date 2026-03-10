using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Playlist?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<int?> InsertAsync(Playlist playlist, CancellationToken cancellationToken = default);

        Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
