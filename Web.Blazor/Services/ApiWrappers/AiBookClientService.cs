using MediaOrganizerApp.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using MediaOrganizerApp.WebApi.Hubs;

namespace Web.Blazor.Services.ApiWrappers;

public sealed class AiBookClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiBookClientService> _logger;
    private readonly IConfiguration _env;
    private readonly string _uri;
    private readonly IHubContext<MediaInfoProgressHub> _hubContext;

    public AiBookClientService(HttpClient httpClient, ILogger<AiBookClientService> logger, IConfiguration env, IHubContext<MediaInfoProgressHub> hubContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _env = env;
        _hubContext = hubContext;

        _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
        _uri = $"{_env["BaseUrl"]}/Ai";
    }

    public async Task<AiBookSummaryResponseDto?> GetBookSummaryAsync(string title, string? author, string language = "pt")
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/book-summary",
                new AiBookSummaryRequestDto { Title = title, Author = author, Language = language });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<AiBookSummaryResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary.");
            return null;
        }
    }

    public async Task<AiBookSummaryResponseDto?> GetBookSummaryOnlyAsync(string title, string? author, string language = "pt")
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/book-summary-only",
                new AiBookSummaryRequestDto { Title = title, Author = author, Language = language });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<AiBookSummaryResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary (summary-only).");
            return null;
        }
    }

    public async Task NotifyProgress(string? connectionId, string message)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, message);
        }
    }
}
