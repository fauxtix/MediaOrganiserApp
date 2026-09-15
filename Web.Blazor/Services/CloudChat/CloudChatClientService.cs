using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;

namespace Web.Blazor.Services.CloudChat
{
    /// <summary>
    /// Blazor client for the OpenRouter proxy endpoints exposed by the WebApi
    /// (POST /api/cloud-chat/stream, GET /api/cloud-chat/models, GET/PUT /api/cloud-chat/settings).
    /// The API key stays server-side; this client only forwards the user identity headers.
    /// </summary>
    public sealed class CloudChatClientService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IConfiguration _env;
        private readonly CurrentUserIdentity _identity;

        public CloudChatClientService(HttpClient http, IConfiguration env, CurrentUserIdentity identity)
        {
            _http = http;
            _env = env;
            _identity = identity;

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var apiKey = _env["ApiKey"] ?? _env["API_Endpoints:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _http.DefaultRequestHeaders.Add("ApiKey", apiKey);
            }
        }

        private string BaseUrl => $"{_env["BaseUrl"]}/api/cloud-chat";

        private async Task ApplyUserHeadersAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var (userId, isAdmin) = await _identity.GetAsync(ct);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                request.Headers.Add("X-UserId", userId);
            }

            request.Headers.Add("X-IsAdmin", isAdmin ? "true" : "false");
        }

        public async IAsyncEnumerable<CloudChatChunk> StreamChatAsync(
            CloudChatPayload payload,
            string? language,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/stream");
            await ApplyUserHeadersAsync(request, cancellationToken);
            request.Content = JsonContent.Create(new CloudChatStreamRequest { Payload = payload, Language = language });

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await SafeReadBodyAsync(response, cancellationToken);
                throw new InvalidOperationException($"CloudChat: HTTP {(int)response.StatusCode}: {errorBody}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                    break;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                CloudChatChunk? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<CloudChatChunk>(line, JsonOptions);
                }
                catch (JsonException)
                {
                    chunk = null;
                }

                if (chunk is not null)
                    yield return chunk;
            }
        }

        public async Task<List<OpenRouterCatalogModel>> GetModelsAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/models");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return new List<OpenRouterCatalogModel>();

            return await response.Content.ReadFromJsonAsync<List<OpenRouterCatalogModel>>(JsonOptions, ct) ?? new List<OpenRouterCatalogModel>();
        }

        public async Task<CloudChatSettingsDto?> GetSettingsAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/settings");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CloudChatSettingsDto>(JsonOptions, ct);
        }

        public async Task<CloudChatRuntimeSettingsDto?> GetRuntimeSettingsAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/runtime-settings");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CloudChatRuntimeSettingsDto>(JsonOptions, ct);
        }

        public async Task<CloudChatUsageDto?> GetUsageAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/usage");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<CloudChatUsageDto>(JsonOptions, ct);
        }

        public async Task UpdateSettingsAsync(CloudChatSettingsUpdateDto dto, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/settings")
            {
                Content = JsonContent.Create(dto)
            };
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
        }

        private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                return (await response.Content.ReadAsStringAsync(ct)).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}