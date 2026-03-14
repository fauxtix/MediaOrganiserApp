using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Web.Blazor.Services.ApiWrappers;

/// <summary>
/// Blazor client wrapper for AI endpoints exposed by MediaOrganizerApp.WebApi.
/// NOTE: The API key is sent as header "ApiKey" like other wrappers in this solution.
/// </summary>
public sealed class AiClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiClientService> _logger;
    private readonly IConfiguration _env;
    private readonly string _uri;

    public AiClientService(HttpClient httpClient, ILogger<AiClientService> logger, IConfiguration env)
    {
        _httpClient = httpClient;
        _logger = logger;
        _env = env;

        // Match existing wrapper convention (e.g., AutorService).
        _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);

        // In this solution BaseUrl seems to already include "/api".
        _uri = $"{_env["BaseUrl"]}/Ai";
    }

    public async Task<AiAuthorProfileResponseDto?> GetAuthorProfileAsync(string name, string language)
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/author-profile",
                new AiAuthorProfileRequestDto { Name = name, Language = language });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<AiAuthorProfileResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating author profile.");
            return null;
        }
    }
}
