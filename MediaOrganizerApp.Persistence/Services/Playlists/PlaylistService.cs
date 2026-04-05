using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp .Persistence.Services.Playlists
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IPlaylistItemRepository _itemRepository;

        public PlaylistService(IPlaylistRepository playlistRepository, IPlaylistItemRepository itemRepository)
        {
            _playlistRepository = playlistRepository;
            _itemRepository = itemRepository;
        }

        public Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return _playlistRepository.GetAllAsync(cancellationToken);
        }

        public Task<Playlist?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _playlistRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<int?> CreateAsync(Playlist playlist, CancellationToken cancellationToken = default)
        {
            if (playlist == null)
                throw new ArgumentNullException(nameof(playlist));

            if (string.IsNullOrWhiteSpace(playlist.Name))
                throw new ArgumentException("Playlist name is required.", nameof(playlist));

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
            playlist.UpdatedAt = DateTime.UtcNow;
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