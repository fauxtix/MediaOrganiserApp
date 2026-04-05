using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MediaOrganizerApp.Application.DTOs.AI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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

    // Streams chat response chunks from API POST /api/ai/chat
    public async IAsyncEnumerable<string> StreamChatAsync(
        List<ChatMessageDto> messages,
        string? connectionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var baseUrl = _env["BaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("BaseUrl not configured");
        var requestUri = $"{baseUrl}/ai/chat";

        var payload = new
        {
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            connectionId = connectionId
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(payload)
        };

        // Read as stream so we can yield partial chunks as the server writes them.
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        // Read in small blocks and yield each chunk as it arrives.
        var buffer = new char[4096];
        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await reader.ReadAsync(buffer, 0, buffer.Length);
            if (read <= 0)
                break;

            yield return new string(buffer, 0, read);
        }
    }
}
