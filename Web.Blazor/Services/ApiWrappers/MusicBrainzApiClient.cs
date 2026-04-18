using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Services.Api;

namespace Web.Blazor.Services.ApiWrappers
{
    public class MusicBrainzApiClient : IMusicApiClient
    {
        private readonly HttpClient _http;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IConfiguration _env;
        private readonly ILogger<MusicBrainzApiClient> _logger;
        private string _Uri;
        private readonly ApplicationSettingsDto AppSettings;

        public MusicBrainzApiClient(HttpClient http, IAppSettingsService appSettingsService, ILogger<MusicBrainzApiClient> logger, IConfiguration env)
        {
            _http = http;
            _appSettingsService = appSettingsService;
            _logger = logger;
            _env = env;
            // DO NOT call async methods synchronously here
            _Uri = $"{_env["BaseUrl"]}/media";
        }

        public async Task<ArtistReleasesResponseDto?> GetArtistReleasesAsync(string artistName)
        {
            // Ensure ApiKey header is present (do this async to avoid blocking ctor)
            try
            {
                if (!_http.DefaultRequestHeaders.Contains("ApiKey"))
                {
                    var apiKey = await GetApiKey();
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        _http.DefaultRequestHeaders.Add("ApiKey", apiKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add ApiKey header for MusicBrainzApiClient.");
            }

            var request = new ArtistReleasesRequestDto { ArtistName = artistName };
            var response = await _http.PostAsJsonAsync($"{_Uri}/artist-releases", request);
            if (!response.IsSuccessStatusCode)
                return null;

            var resp = await response.Content.ReadFromJsonAsync<ArtistReleasesResponseDto>();
            return resp;
        }

        private async Task<string> GetApiKey()
        {
            var settings = await _appSettingsService.GetSettingsAsync();
            return settings.ApiKey;
        }
    }
}
