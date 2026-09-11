using MediaOrganizerApp.Application.Interfaces.Services.Api;

#nullable enable

namespace Web.Blazor.Services.Common
{
    public interface IAiModelResolver
    {
        Task<string> ResolveModelAsync(CancellationToken cancellationToken = default);
    }

    public class AiModelResolver : IAiModelResolver
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);
        private static readonly string[] PreferredModels =
        {
            "qwen3.5:latest",
            "phi4-mini:latest",
            "mistral:latest"
        };

        private readonly IAppSettingsService _appSettingsService;
        private readonly IAiModelsService _aiModelsService;
        private readonly ILogger<AiModelResolver> _logger;

        private readonly SemaphoreSlim _gate = new(1, 1);
        private string? _cached;
        private DateTime _cacheExpiry;

        public AiModelResolver(
            IAppSettingsService appSettingsService,
            IAiModelsService aiModelsService,
            ILogger<AiModelResolver> logger)
        {
            _appSettingsService = appSettingsService;
            _aiModelsService = aiModelsService;
            _logger = logger;
        }

        public async Task<string> ResolveModelAsync(CancellationToken cancellationToken = default)
        {
            if (_cached is not null && DateTime.UtcNow < _cacheExpiry)
                return _cached;

            string resolved;
            try
            {
                resolved = await ResolveCoreAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao resolver o modelo de IA; a usar valor vazio.");
                resolved = "";
            }

            if (string.IsNullOrWhiteSpace(resolved))
                return "";

            _cached = resolved;
            _cacheExpiry = DateTime.UtcNow.Add(CacheTtl);
            return resolved;
        }

        private async Task<string> ResolveCoreAsync(CancellationToken cancellationToken)
        {
            var settings = await _appSettingsService.GetSettingsAsync();
            if (!string.IsNullOrWhiteSpace(settings.AiModelName))
                return settings.AiModelName.Trim();

            var installed = (await _aiModelsService.GetAllAsync())
                .Select(m => m?.Descricao)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToList();

            foreach (var preferred in PreferredModels)
            {
                var match = installed.FirstOrDefault(name =>
                    string.Equals(name, preferred, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    return match;
            }

            return installed.FirstOrDefault() ?? "";
        }
    }
}