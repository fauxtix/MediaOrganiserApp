using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Threading;

namespace Web.Blazor.Services.ApiWrappers;

public partial class AiMediaClientService
{
    public async Task<string?> GetMediaEditionYearAsync(
        string artist,
        string title,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
                _httpClient,
                $"{_uri}/media-edition-year",
                new
                {
                    Artist = artist,
                    Title = title,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
                return null;

            var year = await resp.Content.ReadAsStringAsync(cancellationToken);
            var responseString = string.IsNullOrWhiteSpace(year) ? null : year.Trim();
            return responseString;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Media edition year request was canceled by the caller (timeout/user cancel). Artist={Artist} Title={Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media edition year.");
            return null;
        }
    }

    /// <summary>
    /// Resolves the media edition year via the OpenRouter-backed endpoint.
    /// Returns null when the cloud path is unavailable so callers fall back
    /// to the local Ollama provider.
    /// </summary>
    public async Task<string?> GetMediaEditionYearCloudAsync(
        string artist,
        string title,
        string language = "pt",
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(
                _httpClient,
                $"{_uri}/media-edition-year-cloud",
                new
                {
                    Artist = artist,
                    Title = title,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
                return null;

            var year = await resp.Content.ReadAsStringAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(year) ? null : year.Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Media edition year cloud request was canceled by the caller. Artist={Artist} Title={Title}", artist, title);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media edition year via OpenRouter.");
            return null;
        }
    }
}
