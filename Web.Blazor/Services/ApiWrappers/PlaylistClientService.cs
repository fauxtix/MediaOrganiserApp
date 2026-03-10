using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers
{
    public class PlaylistsClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;
        private readonly ILogger<PlaylistsClientService> _logger;
        private readonly string _uri;

        public PlaylistsClientService(
            HttpClient httpClient,
            ILogger<PlaylistsClientService> logger,
            IConfiguration env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = env;

            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _uri = $"{_env["BaseUrl"]}/Playlists";
        }

        // ------------- PLAYLISTS -------------

        public async Task<IEnumerable<PlaylistDto>> GetAllAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<PlaylistDto>>(_uri);
                return result ?? Array.Empty<PlaylistDto>();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Playlists - Erro ao obter todas as playlists");
                return Array.Empty<PlaylistDto>();
            }
        }

        public async Task<PlaylistDetailsDto?> GetByIdAsync(int id)
        {
            var uri = $"{_uri}/{id}";
            try
            {
                return await _httpClient.GetFromJsonAsync<PlaylistDetailsDto>(uri);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Playlists - Erro ao obter playlist por ID");
                throw;
            }
        }

        public async Task<int?> CreateAsync(PlaylistCreateDto dto)
        {
            try
            {
                using var result = await _httpClient.PostAsJsonAsync(_uri, dto);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<int?>();
            }
            catch (Exception exc)
            {
                _logger.LogError($"Playlists - Erro ao criar playlist: {exc.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateAsync(int id, PlaylistUpdateDto dto)
        {
            var uri = $"{_uri}/{id}";
            try
            {
                using var result = await _httpClient.PutAsJsonAsync(uri, dto);
                return result.IsSuccessStatusCode;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao atualizar playlist ({id}): {exc.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var uri = $"{_uri}/{id}";
            try
            {
                using var result = await _httpClient.DeleteAsync(uri);
                return result.IsSuccessStatusCode;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao apagar playlist ({id}): {exc.Message}");
                throw;
            }
        }

        // ------------- ITENS -------------

        public async Task<IEnumerable<PlaylistItemDto>> GetItemsAsync(int playlistId)
        {
            var uri = $"{_uri}/{playlistId}/items";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<PlaylistItemDto>>(uri);
                return result ?? Array.Empty<PlaylistItemDto>();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao obter itens da playlist ({playlistId})");
                throw;
            }
        }
        public async Task<PlaylistItemDto?> GetItemByIdAsync(int playlistId, int itemId)
        {
            var uri = $"{_uri}/{playlistId}/items/{itemId}";
            try
            {
                return await _httpClient.GetFromJsonAsync<PlaylistItemDto>(uri);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao obter item ({itemId}) da playlist ({playlistId})");
                throw;
            }
        }

        public async Task<int?> AddItemAsync(int playlistId, PlaylistItemCreateDto dto)
        {
            var uri = $"{_uri}/{playlistId}/items";
            try
            {
                using var result = await _httpClient.PostAsJsonAsync(uri, dto);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadFromJsonAsync<int?>();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao adicionar item à playlist ({playlistId}): {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateItemAsync(int playlistId, int itemId, PlaylistItemUpdateDto dto)
        {
            var uri = $"{_uri}/{playlistId}/items/{itemId}";
            try
            {
                using var result = await _httpClient.PutAsJsonAsync(uri, dto);
                return result.IsSuccessStatusCode;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao atualizar item ({itemId}) da playlist ({playlistId}): {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateSortOrderAsync(IEnumerable<PlaylistItemDto> itemsListDto)
        {
            var uri = $"{_uri}/UpdateSortOrder";
            try
            {
                using var result = await _httpClient.PutAsJsonAsync(uri, itemsListDto);
                return result.IsSuccessStatusCode;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao atualizar Ordem da lista: {exc.Message}");
                return false;
            }
        }


        public async Task<bool> DeleteItemAsync(int playlistId, int itemId)
        {
            var uri = $"{_uri}/{playlistId}/items/{itemId}";
            try
            {
                using var result = await _httpClient.DeleteAsync(uri);
                return result.IsSuccessStatusCode;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Playlists - Erro ao apagar item ({itemId}) da playlist ({playlistId}): {exc.Message}");
                throw;
            }
        }
    }
}