using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

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
}
