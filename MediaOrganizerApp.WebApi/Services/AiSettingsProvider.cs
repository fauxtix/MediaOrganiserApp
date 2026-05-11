using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using Microsoft.Extensions.Caching.Memory;

namespace MediaOrganizerApp.WebApi.Services
{
    public sealed class AiSettingsProvider : IAiSettingsProvider
    {
        private const string CacheKey = "ai-settings";
        private readonly IMemoryCache _cache;
        private readonly IAppSettingsRepository _appSettingsRepository;

        public AiSettingsProvider(
            IMemoryCache cache,
            IAppSettingsRepository appSettingsRepository)
        {
            _cache = cache;
            _appSettingsRepository = appSettingsRepository;
        }

        public async Task<(string BaseUrl, string Model)> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey, out (string BaseUrl, string Model) cached))
                return cached;

            var settings = await _appSettingsRepository.GetSettingsAsync();

            var value = (
                settings.ModelBaseUrl ?? string.Empty,
                settings.AiModelName ?? string.Empty);

            _cache.Set(
                CacheKey,
                value,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(60)
                });

            return value;
        }

        public void Invalidate()
        {
            _cache.Remove(CacheKey);
        }
    }
}
