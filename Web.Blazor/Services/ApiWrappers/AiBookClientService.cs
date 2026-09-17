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
    public async Task<List<GoogleBookInfo>?> GetAuthorBooksAsync(
        string author,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null)
    {
        try
        {
            var endpoint = $"{_uri}/author-books?author={Uri.EscapeDataString(author)}&language={language}";
            if (!string.IsNullOrWhiteSpace(connectionId))
                endpoint += $"&connectionId={Uri.EscapeDataString(connectionId)}";
            if (!string.IsNullOrWhiteSpace(correlationId))
                endpoint += $"&correlationId={Uri.EscapeDataString(correlationId)}";
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

    public async Task<AiBookSummaryResponseDto?> GetBookSummaryAsync(
        string title,
        string? author,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null)
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/book-summary",
                new AiBookSummaryRequestDto
                {
                    Title = title,
                    Author = author,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                });

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

    /// <summary>
    /// Fetches an author's books via the OpenRouter-backed endpoint. Returns a
    /// result with the books and, when the cloud path yields nothing, a failure
    /// reason/outcomes so the UI can explain and let the user decide whether to
    /// continue to the local AI provider.
    /// </summary>
    public async Task<CloudAuthorBooksResult> GetAuthorBooksCloudAsync(
        string author,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // The OpenRouter sweep can try several models sequentially (esp. for :free),
            // so the client HTTP timeout must not cut it short. Give it up to 5 minutes;
            // the user can still abort via the provided cancellation token.
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(5));

            var endpoint = $"{_uri}/author-books-cloud?author={Uri.EscapeDataString(author)}&language={language}";
            if (!string.IsNullOrWhiteSpace(connectionId))
                endpoint += $"&connectionId={Uri.EscapeDataString(connectionId)}";
            if (!string.IsNullOrWhiteSpace(correlationId))
                endpoint += $"&correlationId={Uri.EscapeDataString(correlationId)}";
            using var resp = await _httpClient.GetAsync(endpoint, cts.Token);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("AI author-books-cloud request failed. Status={StatusCode} Body={Body} for author={Author}", (int)resp.StatusCode, body, author);

                var parsed = TryDeserializeResult(body);
                if (parsed is not null)
                    return parsed;

                return new CloudAuthorBooksResult
                {
                    FailureReason = $"OpenRouter respondeu com HTTP {(int)resp.StatusCode}."
                };
            }

            var result = await resp.Content.ReadFromJsonAsync<CloudAuthorBooksResult>(cts.Token);
            if (result is not null)
                return result;

            return new CloudAuthorBooksResult
            {
                FailureReason = "Resposta vazia do servidor OpenRouter."
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI author-books-cloud request foi abatida por timeout (5 min) para author={Author}", author);
            return new CloudAuthorBooksResult
            {
                FailureReason = "A pesquisa OpenRouter excedeu o tempo limite (5 min) e foi cancelada."
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AI author-books-cloud request cancelled for author={Author}", author);
            return new CloudAuthorBooksResult
            {
                FailureReason = "Pesquisa OpenRouter cancelada.",
                WasCancelled = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error fetching author books via OpenRouter for {Author}", author);
            return new CloudAuthorBooksResult
            {
                FailureReason = $"Erro ao contactar o OpenRouter para {author}."
            };
        }
    }

    private static CloudAuthorBooksResult? TryDeserializeResult(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<CloudAuthorBooksResult>(body);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<AiBookSummaryResponseDto?> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_uri}/book-summary-only";

            var requestDto = new AiBookSummaryRequestDto
            {
                Title = title,
                Author = author,
                Language = language,
                ConnectionId = connectionId,
                CorrelationId = correlationId
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
