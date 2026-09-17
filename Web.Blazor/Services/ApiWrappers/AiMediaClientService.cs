using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Threading;

namespace Web.Blazor.Services.ApiWrappers;

public partial class AiMediaClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiMediaClientService> _logger;
    private readonly IConfiguration _env;
    private readonly string _uri;

    public AiMediaClientService(HttpClient httpClient, ILogger<AiMediaClientService> logger, IConfiguration env)
    {
        _httpClient = httpClient;
        _logger = logger;
        _env = env;

        _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
        _uri = $"{_env["BaseUrl"]}/api/Ai";
    }

    public async Task<AiMediaNotesResponseDto?> GetMediaNotesAsync(
        string artist,
        string title,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_uri}/media-notes"; 
            var requestDto = new AiMediaNotesRequestDto
            {
                Artist = artist,
                Title = title,
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
                _logger.LogError("AI - Media notes request failed. Status={StatusCode}. Body={Body}", (int)resp.StatusCode, errorBody);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<AiMediaNotesResponseDto>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media notes.");
            return null;
        }
    }

    public async Task<AiMediaMetadataResponseDto?> GetMediaMetadataAsync(string artist,
                                                                         string title,
                                                                         string language = "pt",
                                                                         string? connectionId = null,
                                                                         string? correlationId = null,
                                                                         CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await HttpClientJsonExtensions.PostAsJsonAsync(_httpClient,
                $"{_uri}/media-metadata",
                new AiMediaMetadataRequestDto
                {
                    Artist = artist,
                    Title = title,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI - Media metadata request failed. Status={StatusCode}. Body={Body}", (int)resp.StatusCode, body);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<AiMediaMetadataResponseDto>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Media metadata request was canceled by the caller (timeout/user cancel). Artist={Artist} Title={Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media metadata.");
            return null;
        }
    }

    /// <summary>
    /// Fetches media notes via the OpenRouter-backed endpoint. Returns null
    /// when the cloud path is unavailable (offline, no key, failure) so callers
    /// fall back to the local Ollama provider.
    /// </summary>
    public async Task<AiMediaNotesResponseDto?> GetMediaNotesCloudAsync(
        string artist,
        string title,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await HttpClientJsonExtensions.PostAsJsonAsync(
                _httpClient,
                $"{_uri}/media-notes-cloud",
                new AiMediaNotesRequestDto
                {
                    Artist = artist,
                    Title = title,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI - Media notes cloud request failed. Status={StatusCode} Body={Body}", (int)resp.StatusCode, body);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<AiMediaNotesResponseDto>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Media notes cloud request was canceled by the caller. Artist={Artist} Title={Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media notes via OpenRouter.");
            return null;
        }
    }

    /// <summary>
    /// Fetches media metadata via the OpenRouter-backed endpoint. Returns null
    /// when the cloud path is unavailable so callers fall back to Ollama.
    /// </summary>
    public async Task<AiMediaMetadataResponseDto?> GetMediaMetadataCloudAsync(
        string artist,
        string title,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await HttpClientJsonExtensions.PostAsJsonAsync(
                _httpClient,
                $"{_uri}/media-metadata-cloud",
                new AiMediaMetadataRequestDto
                {
                    Artist = artist,
                    Title = title,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI - Media metadata cloud request failed. Status={StatusCode} Body={Body}", (int)resp.StatusCode, body);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<AiMediaMetadataResponseDto>(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Media metadata cloud request was canceled by the caller. Artist={Artist} Title={Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media metadata via OpenRouter.");
            return null;
        }
    }
}
