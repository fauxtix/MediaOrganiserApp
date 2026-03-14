using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Threading;

namespace Web.Blazor.Services.ApiWrappers;

public sealed class AiMediaClientService
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
        _uri = $"{_env["BaseUrl"]}/Ai";
    }

    public async Task<MediaOrganizerApp.Application.DTOs.AiMediaNotesResponseDto?> GetMediaNotesAsync(string artist, string title, string language = "pt")
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/media-notes",
                new AiMediaNotesRequestDto
                {
                    Artist = artist,
                    Title = title,
                    Language = language
                });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<MediaOrganizerApp.Application.DTOs.AiMediaNotesResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media notes.");
            return null;
        }
    }

    public async Task<MediaOrganizerApp.Application.DTOs.AiMediaMetadataResponseDto?> GetMediaMetadataAsync(string artist, string title, string language = "pt", CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_httpClient,
                $"{_uri}/media-metadata",
                new AiMediaMetadataRequestDto
                {
                    Artist = artist,
                    Title = title,
                    Language = language
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("AI - Media metadata request failed. Status={StatusCode}. Body={Body}", (int)resp.StatusCode, body);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<MediaOrganizerApp.Application.DTOs.AiMediaMetadataResponseDto>();
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
}
