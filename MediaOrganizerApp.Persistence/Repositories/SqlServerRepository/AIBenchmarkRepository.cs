using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
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
    }
}