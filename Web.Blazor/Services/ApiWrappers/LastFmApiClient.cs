using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using System.Text.Json;

namespace Web.Blazor.Services.ApiWrappers
{
    public class LastFmApiClient : IMusicApiClient
    {
        private readonly HttpClient _http;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IConfiguration _env;
        private readonly ILogger<LastFmApiClient> _logger;
        private string _Uri;
        private readonly ApplicationSettingsDto AppSettings;

        public LastFmApiClient(HttpClient http, IAppSettingsService appSettingsService, ILogger<LastFmApiClient> logger, IConfiguration env)
        {
            _http = http;
            _appSettingsService = appSettingsService;
            _logger = logger;
            _env = env;
            _http.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/media";
        }


        private async Task<string> GetApiKey()
        {
            var settings = await _appSettingsService.GetSettingsAsync();
            return settings.ApiKey;
        }

        public async Task<ArtistReleasesResponseDto?> GetLastFMArtistReleasesAsync(string artistName)
        {
            if (string.IsNullOrWhiteSpace(artistName))
                return null;

            try
            {
                var releases = await _http.GetFromJsonAsync<List<ReleaseDto>>(
                    $"{_Uri}/artist-albums/{Uri.EscapeDataString(artistName)}");
                releases ??= new List<ReleaseDto>();

                return new ArtistReleasesResponseDto
                {
                    ArtistName = artistName,
                    Releases = releases
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching artist releases for {Artist}", artistName);
                return null;
            }
        }
    }
}
