using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganiserApp.Persistence.Repositories.SqlServerRepository
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly IDapperContext _context;

        public PlaylistRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sp = "usp_Playlist_GetAll";

            using (var connection = _context.CreateConnection())
            {
                // Dapper não usa CancellationToken diretamente; se quiseres, podes gerir timeout via connection/command
                var result = await connection.QueryAsync<Playlist>(
                    sp,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task<Playlist?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_Playlist_GetById";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleOrDefaultAsync<Playlist>(
                    sp,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        /// <summary>
        /// Insere uma nova playlist e devolve a entidade tal como ficou na BD.
        /// </summary>
        public async Task<Playlist> InsertAsync(Playlist playlist, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_Playlist_Insert";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Playlist>(
                    sp,
                    new
                    {
                        Name = playlist.Name,
                        Description = playlist.Description
                    },
                    commandType: CommandType.StoredProcedure);

                return result;
            }
        }

        public async Task UpdateAsync(Playlist playlist, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_Playlist_Update";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(
                    sp,
                    new
                    {
                        Id = playlist.Id,
                        Name = playlist.Name,
                        Description = playlist.Description
                    },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sp = "usp_Playlist_Delete";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(
                    sp,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}