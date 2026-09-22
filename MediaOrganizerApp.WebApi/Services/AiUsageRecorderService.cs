using System;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.WebApi.Services
{
    /// <summary>
    /// Centralized, non-intrusive logging of internal (non-chat) AI usage into
    /// the AIUsageLog table. Never throws: recording failures are logged as
    /// warnings so they can never break the underlying AI operation.
    /// </summary>
    public sealed class AiUsageRecorderService
    {
        public const string ProviderOpenRouter = "openrouter";
        public const string ProviderOllama = "ollama";
        public const string ProviderOpenAiSdk = "openai-sdk";

        public const string ScenarioCloudFallback = "cloud-fallback";
        public const string ScenarioDirect = "direct";

        private readonly IAiUsageLogRepository _repository;
        private readonly ILogger<AiUsageRecorderService> _logger;

        public AiUsageRecorderService(IAiUsageLogRepository repository, ILogger<AiUsageRecorderService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Provider is the local Ollama fallback, i.e. 'ollama'.
        /// </summary>
        public static bool ProviderIsFallback(string? provider)
            => string.Equals(provider, ProviderOllama, StringComparison.OrdinalIgnoreCase);

        public static string NormalizeScenario(string? scenario, bool eraFallback)
            => string.IsNullOrWhiteSpace(scenario)
                ? (eraFallback ? ScenarioCloudFallback : ScenarioDirect)
                : scenario;

        public async Task RecordAsync(
            string? userId,
            string operacao,
            string provider,
            string? modelo = null,
            string? contexto = null,
            string? scenario = null,
            double durationMs = 0,
            int? tokenCount = null,
            bool sucesso = true,
            CancellationToken ct = default)
        {
            try
            {
                var eraFallback = ProviderIsFallback(provider);
                await _repository.InsertAsync(new AiUsageLog
                {
                    UserId = userId,
                    Operacao = operacao,
                    Provider = provider,
                    Modelo = modelo,
                    Contexto = Truncate(contexto, 512),
                    Scenario = NormalizeScenario(scenario, eraFallback),
                    DurationMs = durationMs,
                    TokenCount = tokenCount,
                    EraFallback = eraFallback,
                    Sucesso = sucesso,
                    DataCriacao = DateTime.UtcNow
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI usage log record failed for op={Operacao} provider={Provider}", operacao, provider);
            }
        }

        private static string? Truncate(string? value, int max)
            => string.IsNullOrWhiteSpace(value) ? null : value.Length <= max ? value : value[..max];
    }
}