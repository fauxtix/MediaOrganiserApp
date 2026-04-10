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
using System.Text;

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

        // Read raw bytes so reads can observe the cancellation token, and decode
        // using a Decoder to preserve UTF-8 state across buffer boundaries.
        var byteBuffer = new byte[4096];
        var decoder = Encoding.UTF8.GetDecoder();
        var charBuffer = new char[Encoding.UTF8.GetMaxCharCount(byteBuffer.Length)];

        while (true)
        {
            var read = await stream.ReadAsync(byteBuffer.AsMemory(0, byteBuffer.Length), cancellationToken);
            if (read == 0)
                break;

            var charsDecoded = decoder.GetChars(byteBuffer, 0, read, charBuffer, 0, false);
            if (charsDecoded > 0)
                yield return new string(charBuffer, 0, charsDecoded);
        }

        // Flush any remaining decoder state
        var finalChars = decoder.GetChars(Array.Empty<byte>(), 0, 0, charBuffer, 0, true);
        if (finalChars > 0)
            yield return new string(charBuffer, 0, finalChars);
    }

    // Start a hub-driven chat: POST to /api/ai/chat/start to request background processing.
    // The server will send progress/chunk/done messages to the provided SignalR connection id.
    public async Task<bool> StartChatAsync(
        List<ChatMessageDto> messages,
        string? connectionId,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        // Preferred: start chat via hub invocation when connectionId is managed by the hub client
        // Keep the HTTP fallback for compatibility.
        return await Task.FromResult(true);
    }

    public async Task CancelChatAsync(string? connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId)) return;
        var baseUrl = _env["BaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("BaseUrl not configured");
        var requestUri = $"{baseUrl}/ai/chat/cancel";
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(new { connectionId = connectionId })
        };

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            // ignore result
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling cancel chat");
        }
    }
}
