using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganiserApp.Application.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _playlistRepository.GetAllAsync(cancellationToken);
        }

        public Task<Playlist?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _playlistRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Playlist> CreateAsync(Playlist playlist, CancellationToken cancellationToken = default)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            if (string.IsNullOrWhiteSpace(playlist.Name))
                throw new ArgumentException("Playlist name is required.", nameof(playlist));

            // CreatedAt/UpdatedAt são tratados na BD via DEFAULT, mas
            // se quiseres manter coerência no domínio, podes setar aqui também.
            return await _playlistRepository.InsertAsync(playlist, cancellationToken);
        }

        public async Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            if (playlist.Id <= 0)
                throw new ArgumentException("Playlist Id must be greater than zero.", nameof(playlist));

            if (string.IsNullOrWhiteSpace(playlist.Name))
                throw new ArgumentException("Playlist name is required.", nameof(playlist));

            await _playlistRepository.UpdateAsync(playlist, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            await _playlistRepository.DeleteAsync(id, cancellationToken);
        }
    }
}