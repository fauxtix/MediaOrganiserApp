using System.Net.Http.Headers;
using MediaOrganizerApp.Application.DTOs.AI;
using Web.Blazor.Services.CloudChat;

namespace Web.Blazor.Services.ApiWrappers
{
    /// <summary>
    /// Blazor client for the internal AI usage summary endpoint
    /// (GET /api/ai-usage/summary). Forwards the user identity headers so
    /// non-admin users only see their own recorded AI usage.
    /// </summary>
    public sealed class AiUsageClientService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _env;
        private readonly CurrentUserIdentity _identity;

        public AiUsageClientService(HttpClient http, IConfiguration env, CurrentUserIdentity identity)
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

        public async Task<AiUsageSummaryDto?> GetSummaryAsync(CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/ai-usage/summary");
            await ApplyUserHeadersAsync(request, ct);

            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AiUsageSummaryDto>(ct);
        }
    }
}