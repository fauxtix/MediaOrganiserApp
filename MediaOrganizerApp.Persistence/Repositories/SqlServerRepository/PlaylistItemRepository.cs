using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganiserApp.Persistence.Repositories.SqlServerRepository
{
    public class PlaylistItemRepository : IPlaylistItemRepository
    {
        private readonly IDapperContext _context;

        public PlaylistItemRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlaylistItem>> GetByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_GetByPlaylistId";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<PlaylistItem>(
                    sp,
                    new { PlaylistId = playlistId },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<PlaylistItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_GetById";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleOrDefaultAsync<PlaylistItem>(
                    sp,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        /// <summary>
        /// Insere um novo item de playlist e devolve a entidade tal como ficou na BD.
        /// </summary>
        public async Task<PlaylistItem> InsertAsync(PlaylistItem item, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_Insert";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<PlaylistItem>(
                    sp,
                    new
                    {
                        PlaylistId = item.PlaylistId,
                        MediaType = (int)item.MediaType,
                        ResourcePathOrUrl = item.ResourcePathOrUrl,
                        Title = item.Title,
                        SortOrder = item.SortOrder == 0 ? (int?)null : item.SortOrder // se 0, deixamos a SP calcular
                    },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task UpdateAsync(PlaylistItem item, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_Update";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(
                    sp,
                    new
                    {
                        Id = item.Id,
                        MediaType = (int)item.MediaType,
                        ResourcePathOrUrl = item.ResourcePathOrUrl,
                        Title = item.Title,
                        SortOrder = item.SortOrder
                    },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_Delete";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(
                    sp,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public Task UpdateOrderAsync(int playlistId, IEnumerable<PlaylistItemOrderUpdateDto> items, CancellationToken cancellationToken = default)
        {

            // TODO seem as stored procedure and implement this method
            throw new System.NotImplementedException();
        }
    }
}