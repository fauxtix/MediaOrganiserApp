using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class AIBenchmarkRepository : IAIBenchmarkRepository
    {
        private readonly IDapperContext _context;

        public AIBenchmarkRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task<int> GetOrCreatePromptIdAsync(
            string textoPrompt,
            string userId,
            string descricao,
            double temperatura,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TextoPrompt", textoPrompt);
            parameters.Add("@UserID", userId);
            parameters.Add("@Descricao", descricao);
            parameters.Add("@Temperatura", temperatura);
            parameters.Add("@PromptId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_AIPrompt_GetOrCreate",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@PromptId");
        }

        public async Task CreateResponseAsync(AIResponse response, CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PromptId", response.PromptId);
            parameters.Add("@UserID", response.UserId);
            parameters.Add("@NomeModelo", response.NomeModelo);
            parameters.Add("@ModeloResolvido", response.ModeloResolvido);
            parameters.Add("@TextoResposta", response.TextoResposta);
            parameters.Add("@TempoProcessamento", response.TempoProcessamento);
            parameters.Add("@TokensPorSegundo", response.TokensPorSegundo);
            parameters.Add("@TempoPuroMs", response.TempoPuroMs);
            parameters.Add("@TempoCargaMs", response.TempoCargaMs);
            parameters.Add("@TamanhoTokens", response.TamanhoTokens);
            parameters.Add("@IdiomaSessao", response.IdiomaSessao);
            parameters.Add("@AIResponseId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "usp_AIResponse_Insert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<AiBenchmarkSummaryDto> GetBenchmarkSummaryAsync(
            string? userId = null,
            bool isAdmin = false,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var paramsCount = new { UserID = isAdmin ? null : userId };
            var stats = await connection.QuerySingleOrDefaultAsync<BenchmarkStatsRow>(
                @"
                SELECT
                    COUNT(*)                                                                   AS Total,
                    SUM(CASE WHEN DataCriacao >= DATEADD(day, -7, GETUTCDATE()) THEN 1 ELSE 0 END) AS Ultimos7,
                    -- Duração: wall-time médio de todas as respostas (inclui cached; ok).
                    AVG(CASE WHEN TempoProcessamento > 0 THEN CAST(TempoProcessamento AS float) END) AS Latencia,
                    -- Tokens/s só tem significado físico para streams reais (>= 1 s): respostas
                    -- sub-segundo são cache/burst e dão taxas fisicamente impossíveis.
                    AVG(CASE WHEN TempoProcessamento >= 1.0 THEN CAST(TokensPorSegundo AS float) END)     AS TokensPorSegundo,
                    AVG(CASE WHEN TamanhoTokens > 0 THEN CAST(TamanhoTokens AS float) END)           AS TamanhoTokens
                FROM AIResponses
                WHERE (@UserID IS NULL OR UserID = @UserID)
                  AND (TempoProcessamento IS NULL OR TempoProcessamento >= 0.05);",
                paramsCount);

            var models = await connection.QueryAsync<ModeloUsageContagem>(
                @"
                SELECT TOP 5
                    COALESCE(NULLIF(ModeloResolvido, ''), NomeModelo) AS Modelo,
                    COUNT(*)                                           AS Contagem
                FROM AIResponses
                WHERE (@UserID IS NULL OR UserID = @UserID)
                  AND (TempoProcessamento IS NULL OR TempoProcessamento >= 0.05)
                GROUP BY COALESCE(NULLIF(ModeloResolvido, ''), NomeModelo)
                ORDER BY Contagem DESC;",
                paramsCount);

            var modelos = models?.ToList() ?? new List<ModeloUsageContagem>();
            foreach (var m in modelos)
            {
                m.Provider = ModeloUsageContagem.ProviderFromModelName(m.Modelo);
            }

            return new AiBenchmarkSummaryDto
            {
                TotalRespostas = stats?.Total ?? 0,
                Ultimos7Dias = stats?.Ultimos7 ?? 0,
                LatenciaMediaSegundos = Round(stats?.Latencia),
                TokensPorSegundoMedio = Round(stats?.TokensPorSegundo),
                TamanhoTokensMedio = Round(stats?.TamanhoTokens),
                ModelosMaisUsados = modelos
            };
        }

        private static double? Round(double? value)
            => value.HasValue ? Math.Round(value.Value, 2) : null;

        private sealed class BenchmarkStatsRow
        {
            public int Total { get; set; }
            public int Ultimos7 { get; set; }
            public double? Latencia { get; set; }
            public double? TokensPorSegundo { get; set; }
            public double? TamanhoTokens { get; set; }
        }
    }
}