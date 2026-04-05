using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Services.Playlists
{
    public class PlaylistItemService : IPlaylistItemService
    {
        private readonly IPlaylistItemRepository _itemRepository;
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistItemService(
            IPlaylistItemRepository itemRepository,
            IPlaylistRepository playlistRepository)
        {
            _itemRepository = itemRepository;
            _playlistRepository = playlistRepository;
        }

        public Task<IEnumerable<PlaylistItem>> GetByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default)
        {
            if (playlistId <= 0)
                throw new ArgumentException("PlaylistId must be greater than zero.", nameof(playlistId));

            return _itemRepository.GetByPlaylistIdAsync(playlistId, cancellationToken);
        }

        public Task<PlaylistItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            return _itemRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<int?> AddAsync(PlaylistItem item, CancellationToken cancellationToken = default)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.PlaylistId <= 0)
                throw new ArgumentException("PlaylistId must be greater than zero.", nameof(item));

            if (!Enum.IsDefined(typeof(MediaType), item.MediaType))
                throw new ArgumentException("Invalid media type.", nameof(item));

            if (string.IsNullOrWhiteSpace(item.ResourcePathOrUrl))
                throw new ArgumentException("Resource path or URL is required.", nameof(item));

            // valida se a playlist existe
            var playlist = await _playlistRepository.GetByIdAsync(item.PlaylistId, cancellationToken);
            if (playlist == null)
                throw new InvalidOperationException($"Playlist with id {item.PlaylistId} not found.");

            // SortOrder == 0 → deixa a SP calcular (passamos null no repositório)
            var created = await _itemRepository.InsertAsync(item, cancellationToken);
            return created;
        }

        public async Task UpdateAsync(PlaylistItem item, CancellationToken cancellationToken = default)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(item));

            if (!Enum.IsDefined(typeof(MediaType), item.MediaType))
                throw new ArgumentException("Invalid media type.", nameof(item));

            if (string.IsNullOrWhiteSpace(item.ResourcePathOrUrl))
                throw new ArgumentException("Resource path or URL is required.", nameof(item));

            // opcional: garantir que o item existe antes de atualizar
            var existing = await _itemRepository.GetByIdAsync(item.Id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Playlist item with id {item.Id} not found.");

            // garantir que não alteram sem querer o PlaylistId
            item.PlaylistId = existing.PlaylistId;

            await _itemRepository.UpdateAsync(item, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            await _itemRepository.DeleteAsync(id, cancellationToken);
        }


        public async Task<IEnumerable<PlaylistItemDto>> GetItemDtosByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default)
        {
            if (playlistId <= 0)
                throw new ArgumentException("PlaylistId must be greater than zero.", nameof(playlistId));

            return await _itemRepository.GetItemDtosByPlaylistIdAsync(playlistId, cancellationToken);
        }

        public async Task UpdateOrderAsync(int playlistId, IEnumerable<PlaylistItemOrderUpdateDto> items, CancellationToken cancellationToken = default)
        {
            if (playlistId <= 0)
                throw new ArgumentException("PlaylistId must be greater than zero.", nameof(playlistId));

            await _itemRepository.UpdateOrderAsync(playlistId, items, cancellationToken);
        }
    }
}