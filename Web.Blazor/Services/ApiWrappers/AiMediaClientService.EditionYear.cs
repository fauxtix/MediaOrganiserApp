using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Threading;

namespace Web.Blazor.Services.ApiWrappers;

public partial class AiMediaClientService
{
    public async Task<string?> GetMediaEditionYearAsync(string artist, string title, string language = "pt", string? connectionId = null, CancellationToken cancellationToken = default)
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
                    ConnectionId = connectionId
                },
                cancellationToken);

            if (!resp.IsSuccessStatusCode)
                return null;

            var year = await resp.Content.ReadAsStringAsync(cancellationToken);
            // Expecting year as plain string (e.g., "1999") or "N/A"
            return string.IsNullOrWhiteSpace(year) || year == "N/A" ? null : year.Trim();
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
}
