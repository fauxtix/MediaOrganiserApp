using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;

using Microsoft.AspNetCore.SignalR;

namespace Web.Blazor.Services.ApiWrappers;

public sealed class AiBookClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiBookClientService> _logger;
    private readonly IConfiguration _env;
    private readonly string _uri;

    public AiBookClientService(HttpClient httpClient, ILogger<AiBookClientService> logger, IConfiguration env)
    {
        _httpClient = httpClient;
        _logger = logger;
        _env = env;

        if (!string.IsNullOrWhiteSpace(_env["ApiKey"]))
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
        _uri = $"{_env["BaseUrl"]}/api/Ai";
    }

    // Return null on transient failure/cancellation; return list (possibly empty) only on success
    public async Task<List<GoogleBookInfo>?> GetAuthorBooksAsync(string author, string language = "pt")
    {
        try
        {
            var endpoint = $"{_uri}/author-books?author={Uri.EscapeDataString(author)}&language={language}";
            using var resp = await _httpClient.GetAsync(endpoint);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning("AI author-books request failed. Status={StatusCode} Body={Body} for author={Author}", (int)resp.StatusCode, body, author);
                return null;
            }

            try
            {
                var list = await resp.Content.ReadFromJsonAsync<List<GoogleBookInfo>>();
                _logger.LogDebug("AI author-books returned {Count} items for {Author}", list?.Count ?? 0, author);
                return list ?? new List<GoogleBookInfo>();
            }
            catch (Exception ex)
            {
                var raw = await resp.Content.ReadAsStringAsync();
                _logger.LogWarning(ex, "Failed to deserialize AI author-books response for {Author}. Raw: {RawPreview}", author, raw?.Substring(0, Math.Min(512, raw?.Length ?? 0)));
                return null;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AI author-books request cancelled for author={Author}", author);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error fetching author books for {Author}", author);
            return null;
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
}
