using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Playlist?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Playlist> CreateAsync(Playlist playlist, CancellationToken cancellationToken = default);

        Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default);

        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
