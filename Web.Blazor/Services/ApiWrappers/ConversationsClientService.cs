using MediaOrganizerApp.Application.DTOs.AI;
using System.Net.Http.Headers;
using Web.Blazor.Services.CloudChat;

namespace Web.Blazor.Services.ApiWrappers
{
    public sealed class ConversationsClientService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _env;
        private readonly CurrentUserIdentity _identity;

        public ConversationsClientService(HttpClient http, IConfiguration env, CurrentUserIdentity identity)
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

        private string BaseUrl => $"{_env["BaseUrl"]}/api";

        private async Task ApplyUserHeadersAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var (userId, isAdmin) = await _identity.GetAsync(ct);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                request.Headers.Add("X-UserId", userId);
            }

            request.Headers.Add("X-IsAdmin", isAdmin ? "true" : "false");
        }

        public async Task<List<ConversationDto>> GetAllAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/conversations");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return new List<ConversationDto>();

            return await response.Content.ReadFromJsonAsync<List<ConversationDto>>(ct) ?? new List<ConversationDto>();
        }

        public async Task<ConversationDto?> GetAsync(string id, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/conversations/{id}");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ConversationDto>(ct);
        }

        public async Task SaveAsync(ConversationUpdateDto dto, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/conversations/{dto.Id}")
            {
                Content = JsonContent.Create(dto)
            };
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task CreateAsync(ConversationCreateDto dto, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/conversations")
            {
                Content = JsonContent.Create(dto)
            };
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/conversations/{id}");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}