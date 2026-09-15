using MediaOrganizerApp.Application.DTOs.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Web.Blazor.Services.ApiWrappers;

public sealed class TranslateClientService
{
    private readonly HttpClient _http;
    private readonly ILogger<TranslateClientService> _logger;
    private readonly string _baseUrl;

    public TranslateClientService(HttpClient http, ILogger<TranslateClientService> logger, IConfiguration config)
    {
        _http = http;
        _logger = logger;
        _http.DefaultRequestHeaders.Add("ApiKey", config["ApiKey"]);
        _baseUrl = $"{config["BaseUrl"]}/api";
    }

    /// <summary>
    /// Translates the given text preferring the OpenRouter-backed endpoint and
    /// falling back to the local Ollama endpoint when the cloud path is
    /// unavailable (offline, no key, HTTP/parse failure).
    /// </summary>
    public async Task<TranslateResponseDto?> TranslateAsync(TranslateRequestDto req, CancellationToken cancellationToken = default)
    {
        var cloud = await TranslateCloudAsync(req, cancellationToken);
        if (cloud is not null)
            return cloud;

        _logger.LogInformation("OpenRouter translation unavailable; falling back to local Ollama.");
        return await TranslateLocalAsync(req, cancellationToken);
    }

    /// <summary>
    /// Calls the OpenRouter-backed translate endpoint. Returns null when the
    /// cloud path is unavailable so callers fall back to the local provider.
    /// </summary>
    public async Task<TranslateResponseDto?> TranslateCloudAsync(TranslateRequestDto req, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = $"{_baseUrl}/ai/translate-cloud";
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_http, uri, req, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Translate cloud request failed. Status={StatusCode} Body={Body}", (int)resp.StatusCode, body);
                return null;
            }
            return await resp.Content.ReadFromJsonAsync<TranslateResponseDto>(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling translate cloud API");
            return null;
        }
    }

    private async Task<TranslateResponseDto?> TranslateLocalAsync(TranslateRequestDto req, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = $"{_baseUrl}/ai/translate";
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_http, uri, req, cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<TranslateResponseDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling translate API");
            return null;
        }
    }
}
