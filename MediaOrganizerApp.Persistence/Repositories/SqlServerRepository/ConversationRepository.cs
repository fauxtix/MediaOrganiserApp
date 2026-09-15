using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Persistence;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly IDapperContext _context;

        public ConversationRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Conversation>> GetAllAsync(
            string? userId = null,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            using (var connection = _context.CreateConnection())
            {
                var sproc = "usp_Conversation_GetAll";
                return await connection.QueryAsync<Conversation>(
                    sproc,
                    new { UserID = userId, IsAdmin = isAdmin },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<Conversation?> GetByIdAsync(
            string id,
            string? userId = null,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            using (var connection = _context.CreateConnection())
            {
                var sproc = "usp_Conversation_GetById";
                return await connection.QueryFirstOrDefaultAsync<Conversation>(
                    sproc,
                    new { Id = id, UserID = userId, IsAdmin = isAdmin },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task InsertAsync(Conversation conv, CancellationToken cancellationToken = default)
        {
            using (var connection = _context.CreateConnection())
            {
                var sproc = "usp_Conversation_Insert";
                await connection.ExecuteAsync(
                    sproc,
                    new { conv.Id, conv.Title, conv.Json, conv.CreatedAt, conv.UpdatedAt, UserID = conv.UserId },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task UpdateAsync(
            Conversation conv,
            string? userId = null,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            using (var connection = _context.CreateConnection())
            {
                var sproc = "usp_Conversation_Update";
                await connection.ExecuteAsync(
                    sproc,
                    new { conv.Id, conv.Title, conv.Json, conv.UpdatedAt, UserID = userId, IsAdmin = isAdmin },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task DeleteAsync(
            string id,
            string? userId = null,
            bool isAdmin = false,
            CancellationToken cancellationToken = default)
        {
            using (var connection = _context.CreateConnection())
            {
                var sproc = "usp_Conversation_Delete";
                await connection.ExecuteAsync(
                    sproc,
                    new { Id = id, UserID = userId, IsAdmin = isAdmin },
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}