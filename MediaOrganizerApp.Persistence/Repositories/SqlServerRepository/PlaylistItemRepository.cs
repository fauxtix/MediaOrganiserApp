using Dapper;
using Google.Apis.YouTube.v3.Data;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using PlaylistItem = MediaOrganizerApp.Domain.Entities.PlaylistItem;

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
        public async Task<int?> InsertAsync(PlaylistItem item, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_Insert";
            var p = new DynamicParameters();
            p.Add("@PlaylistId", item.PlaylistId, dbType: DbType.Int32);
            p.Add("@MediaType", item.MediaType, dbType: DbType.Int32);
            p.Add("@MediaId", item.MediaId, dbType: DbType.Int32);
            p.Add("@ResourcePathOrUrl", item.ResourcePathOrUrl, dbType: System.Data.DbType.String);
            p.Add("@Title", item.Title, dbType: System.Data.DbType.String);
            p.Add("@SortOrder", item.SortOrder, dbType: System.Data.DbType.Int32);
            p.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);


            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(
                    sp, param: p,
                    commandType: CommandType.StoredProcedure);

                var _id = p.Get<int>("@Id");

                return _id;
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

        public async Task UpdateOrderAsync(int playlistId, IEnumerable<PlaylistItemOrderUpdateDto> items, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_UpdateOrder";

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("SortOrder", typeof(int));

            foreach (var item in items)
                table.Rows.Add(item.Id, item.SortOrder);

            var p = new DynamicParameters();
            p.Add("@PlaylistId", playlistId, dbType: DbType.Int32);
            p.Add("@Items", table.AsTableValuedParameter("dbo.PlaylistItemOrderUpdateType"));

            using (var connection = _context.CreateConnection())
            {
                var command = new CommandDefinition(
                    sp,
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);

                await connection.ExecuteAsync(command);
            }
        }
        public async Task<IEnumerable<PlaylistItemDto>> GetItemDtosByPlaylistIdAsync(int playlistId, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_PlaylistItem_GetByPlaylistId_WithMediaInfo";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<PlaylistItemDto>(
                    sp,
                    new { PlaylistId = playlistId },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }
    }
}