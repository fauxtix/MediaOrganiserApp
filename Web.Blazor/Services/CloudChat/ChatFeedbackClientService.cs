using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.AI;

namespace Web.Blazor.Services.CloudChat
{
    /// <summary>
    /// Blazor client for the chat feedback endpoints (POST /api/chat-feedback,
    /// GET /api/chat-feedback/by-conversation). Used by both chats (cloud + local)
    /// to record like/dislike ratings on assistant responses.
    /// </summary>
    public sealed class ChatFeedbackClientService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IConfiguration _env;
        private readonly CurrentUserIdentity _identity;

        public ChatFeedbackClientService(HttpClient http, IConfiguration env, CurrentUserIdentity identity)
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

        private string BaseUrl => $"{_env["BaseUrl"]}/api/chat-feedback";

        private async Task ApplyUserHeadersAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var (userId, isAdmin) = await _identity.GetAsync(ct);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                request.Headers.Add("X-UserId", userId);
            }

            request.Headers.Add("X-IsAdmin", isAdmin ? "true" : "false");
        }

        public async Task UpsertAsync(ChatFeedbackDto dto, CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
            {
                Content = JsonContent.Create(dto)
            };
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ChatFeedbackDto>> GetByConversationAsync(
            string @interface,
            string? conversationId,
            CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/by-conversation?interface={Uri.EscapeDataString(@interface)}";
            if (!string.IsNullOrWhiteSpace(conversationId))
            {
                url += $"&conversationId={Uri.EscapeDataString(conversationId)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new List<ChatFeedbackDto>();
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
            {
                return new List<ChatFeedbackDto>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<ChatFeedbackDto>>(body, JsonOptions) ?? new List<ChatFeedbackDto>();
            }
            catch (System.Text.Json.JsonException)
            {
                return new List<ChatFeedbackDto>();
            }
        }
    }
}