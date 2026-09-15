using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class AIChatSettingsRepository : IAIChatSettingsRepository
    {
        private readonly IDapperContext _context;

        public AIChatSettingsRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AIChatSetting>> GetAllAsync(
            string? userId,
            bool isAdmin,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<AIChatSetting>(
                "usp_AIChatSettings_GetAll",
                new { UserID = userId, IsAdmin = isAdmin },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<string?> GetValueAsync(
            string chave,
            string? userId,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<string>(
                "usp_AIChatSettings_GetValue",
                new { Chave = chave, UserID = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task SetValueAsync(
            string chave,
            string valor,
            string? userId,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "usp_AIChatSettings_Upsert",
                new { Chave = chave, Valor = valor, UserID = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteValueAsync(
            string chave,
            string? userId,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "usp_AIChatSettings_Delete",
                new { Chave = chave, UserID = userId },
                commandType: CommandType.StoredProcedure);
        }
    }
}