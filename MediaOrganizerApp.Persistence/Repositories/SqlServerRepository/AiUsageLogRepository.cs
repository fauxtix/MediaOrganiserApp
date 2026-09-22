using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class AiUsageLogRepository : IAiUsageLogRepository
    {
        private readonly IDapperContext _context;

        public AiUsageLogRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task InsertAsync(AiUsageLog log, CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", log.UserId);
            parameters.Add("@Operacao", log.Operacao);
            parameters.Add("@Provider", log.Provider);
            parameters.Add("@Modelo", log.Modelo);
            parameters.Add("@Contexto", log.Contexto);
            parameters.Add("@Scenario", log.Scenario);
            parameters.Add("@DurationMs", log.DurationMs);
            parameters.Add("@TokenCount", log.TokenCount);
            parameters.Add("@EraFallback", log.EraFallback);
            parameters.Add("@Sucesso", log.Sucesso);

            await connection.ExecuteAsync(
                @"
                INSERT INTO dbo.AIUsageLog
                    (UserID, Operacao, Provider, Modelo, Contexto, Scenario, DurationMs, TokenCount, EraFallback, Sucesso)
                VALUES
                    (@UserID, @Operacao, @Provider, @Modelo, @Contexto, @Scenario, @DurationMs, @TokenCount, @EraFallback, @Sucesso);",
                parameters);
        }

        public async Task<AiUsageSummaryDto> GetSummaryAsync(
            string? userId = null,
            bool isAdmin = false,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var paramsCount = new { UserID = isAdmin ? null : userId };

            var stats = await connection.QuerySingleOrDefaultAsync<StatsRow>(
                @"
                SELECT
                    COUNT(*)                                                               AS TotalGeral,
                    SUM(CASE WHEN DataCriacao >= DATEADD(day, -7, SYSUTCDATETIME()) THEN 1 ELSE 0 END) AS Ultimos7,
                    SUM(CASE WHEN Provider = 'openrouter' THEN 1 ELSE 0 END)                AS UsosOpenRouter,
                    SUM(CASE WHEN Provider = 'ollama'     THEN 1 ELSE 0 END)                AS UsosOllama,
                    SUM(CASE WHEN EraFallback = 1         THEN 1 ELSE 0 END)                AS UsosFallback
                FROM dbo.AIUsageLog
                WHERE (@UserID IS NULL OR UserID = @UserID);",
                paramsCount);

            var porOperacao = await connection.QueryAsync<AiUsageOperacaoDto>(
                @"
                SELECT
                    Operacao,
                    COUNT(*)                                                               AS Contagem,
                    SUM(CASE WHEN DataCriacao >= DATEADD(day, -7, SYSUTCDATETIME()) THEN 1 ELSE 0 END) AS Ultimos7,
                    SUM(CASE WHEN Provider = 'openrouter' THEN 1 ELSE 0 END)                AS OpenRouter,
                    SUM(CASE WHEN Provider = 'ollama'     THEN 1 ELSE 0 END)                AS Ollama,
                    SUM(CASE WHEN EraFallback = 1         THEN 1 ELSE 0 END)                AS Fallback
                FROM dbo.AIUsageLog
                WHERE (@UserID IS NULL OR UserID = @UserID)
                GROUP BY Operacao
                ORDER BY Contagem DESC;",
                paramsCount);

            var ultimos = await connection.QueryAsync<AiUsageRegistoDto>(
                @"
                SELECT TOP 15
                    AIUsageId AS Id,
                    Operacao,
                    Provider,
                    Contexto,
                    Scenario,
                    DurationMs,
                    Sucesso,
                    DataCriacao
                FROM dbo.AIUsageLog
                WHERE (@UserID IS NULL OR UserID = @UserID)
                ORDER BY AIUsageId DESC;",
                paramsCount);

            return new AiUsageSummaryDto
            {
                TotalGeral = stats?.TotalGeral ?? 0,
                Ultimos7 = stats?.Ultimos7 ?? 0,
                UsosOpenRouter = stats?.UsosOpenRouter ?? 0,
                UsosOllama = stats?.UsosOllama ?? 0,
                UsosFallback = stats?.UsosFallback ?? 0,
                PorOperacao = porOperacao as List<AiUsageOperacaoDto> ?? new List<AiUsageOperacaoDto>(porOperacao),
                UltimosRegistos = ultimos as List<AiUsageRegistoDto> ?? new List<AiUsageRegistoDto>(ultimos)
            };
        }

        private sealed class StatsRow
        {
            public int TotalGeral { get; set; }
            public int Ultimos7 { get; set; }
            public int UsosOpenRouter { get; set; }
            public int UsosOllama { get; set; }
            public int UsosFallback { get; set; }
        }
    }
}