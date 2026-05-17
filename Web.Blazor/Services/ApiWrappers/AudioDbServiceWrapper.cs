using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.Api.AudioDb;
using MediaOrganizerApp.Application.Interfaces.Services.Api;

namespace Web.Blazor.Services.ApiWrappers
{
    public class AudioDbServiceWrapper : IAudioDbServiceWrapper
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _env;
        private readonly ILogger<AudioDbServiceWrapper> _logger;

        private readonly string _uri;

        public AudioDbServiceWrapper(
            HttpClient http,
            ILogger<AudioDbServiceWrapper> logger,
            IConfiguration env)
        {
            _http = http;
            _logger = logger;
            _env = env;

            _http.DefaultRequestHeaders.Add(
                "ApiKey",
                _env["ApiKey"]);

            _uri = $"{_env["BaseUrl"]}/api/media";
        }

        public async Task<AlbumDetailsDto?> GetAlbumDetailsAsync(
            string artist,
            string album)
        {
            if (string.IsNullOrWhiteSpace(artist) ||
                string.IsNullOrWhiteSpace(album))
            {
                return null;
            }

            try
            {
                var url =
                    $"{_uri}/audiodb-data/" +
                    $"{Uri.EscapeDataString(artist)}/" +
                    $"{Uri.EscapeDataString(album)}";

                _logger.LogDebug(
                    "AudioDbServiceWrapper: GET {Url}",
                    url);

                using var response =
                    await _http.GetAsync(url);

                var body =
                    await response.Content.ReadAsStringAsync();

                _logger.LogDebug(
                    "AudioDbServiceWrapper: Response {StatusCode} Body: {Body}",
                    (int)response.StatusCode,
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var result =
                    JsonSerializer.Deserialize<AlbumDetailsDto>(
                        body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error calling AudioDB API");

                return null;
            }
        }
    }
}