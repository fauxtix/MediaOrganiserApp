using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;

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

    public async Task<List<GoogleBookInfo>> GetAuthorBooksAsync(string author, string language = "pt")
    {
        try
        {
            var endpoint = $"{_uri}/author-books?author={Uri.EscapeDataString(author)}&language={language}";
            using var resp = await _httpClient.GetAsync(endpoint);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI author-books request failed. Status={StatusCode} for author={Author}", (int)resp.StatusCode, author);
                return new List<GoogleBookInfo>();
            }

            var list = await resp.Content.ReadFromJsonAsync<List<GoogleBookInfo>>();
            return list ?? new List<GoogleBookInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error fetching author books for {Author}", author);
            return new List<GoogleBookInfo>();
        }
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

    public async Task<AiBookSummaryResponseDto?> GetBookSummaryOnlyAsync(string title, string? author, string language = "pt", string? connectionId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_uri}/book-summary-only";

            var requestDto = new AiBookSummaryRequestDto
            {
                Title = title,
                Author = author,
                Language = language,
                ConnectionId = connectionId
            };

            using var resp = await HttpClientJsonExtensions.PostAsJsonAsync(
                   _httpClient,
                   endpoint,
                   requestDto,
                   cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var errorBody = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI - Book summary request failed. Status={StatusCode}. Body={Body}", (int)resp.StatusCode, errorBody);
                return null;
            }

            var aiResponse = await resp.Content.ReadFromJsonAsync<AiBookSummaryResponseDto>(cancellationToken);
            return aiResponse;

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
