using System.Runtime.CompilerServices;
using MediaOrganizerApp.Application.DTOs.AI;
using System.Text;
using System.Text.Json;

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

        _uri = $"{_env["BaseUrl"]}/api/ai";
    }

    public async Task<AiAuthorProfileResponseDto?> GetAuthorProfileAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        string? scenario = null)
    {
        var cloud = await GetAuthorProfileCloudAsync(name, language, connectionId, correlationId, scenario);
        if (cloud is not null)
            return cloud;

        _logger.LogWarning("OpenRouter author-profile unavailable; falling back to local provider.");
        return await GetAuthorProfileLocalAsync(name, language, connectionId, correlationId, "cloud-fallback");
    }

    public async Task<AiAuthorProfileResponseDto?> GetAuthorProfileCloudAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        string? scenario = null)
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/author-profile-cloud",
                new AiAuthorProfileRequestDto
                {
                    Name = name,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId,
                    Scenario = scenario
                });

            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<AiAuthorProfileResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating author profile via OpenRouter.");
            return null;
        }
    }

    private async Task<AiAuthorProfileResponseDto?> GetAuthorProfileLocalAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        string? scenario = null)
    {
        try
        {
            using var resp = await _httpClient.PostAsJsonAsync(
                $"{_uri}/author-profile",
                new AiAuthorProfileRequestDto
                {
                    Name = name,
                    Language = language,
                    ConnectionId = connectionId,
                    CorrelationId = correlationId,
                    Scenario = scenario
                });

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
    public async IAsyncEnumerable<AiChatChunk> StreamChatAsync(
        List<ChatMessageDto> messages,
        string? connectionId,
        string? correlationId,
        string? language,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var baseUrl = _uri;
        var requestUri = $"{baseUrl}/chat";

        var payload = new
        {
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            connectionId,
            correlationId,
            language
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(payload)
        };

        // Read as stream so we can yield partial chunks as the server writes them.
        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        // The server streams NDJSON (one AiChatChunk per line).
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // TEMP DIAG (Ollama chat reasoning): counts per request.
        int diagLines = 0, diagChunks = 0, diagReasonChunks = 0, diagWhiteSpaceOnly = 0;

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            diagLines++;

            AiChatChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<AiChatChunk>(line);
            }
            catch (JsonException)
            {
                chunk = null;
            }

            if (chunk is not null)
            {
                diagChunks++;
                if (!string.IsNullOrWhiteSpace(chunk.Reasoning))
                    diagReasonChunks++;
                if (string.IsNullOrEmpty(chunk.Text))
                    diagWhiteSpaceOnly++;
                var reasonInfo = chunk.Reasoning is null
                    ? "null"
                    : $"len={chunk.Reasoning.Length} head=\"{Trunc(chunk.Reasoning, 60)}\"";
            }
            else
            {
                _logger.LogWarning("[AI-WIRE] PARSE_FAIL line=\"{Line}\"", Trunc(line, 120));
            }

            if (chunk is not null)
                yield return chunk;
        }
    }

    private static string Trunc(string s, int max)
    {
        s = s.Replace("\n", "\\n").Replace("\r", "\\r");
        return s.Length <= max ? s : s[..max] + "...";
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

    // Não usado: API de cancelamento é chamada diretamente pelo cliente SignalR
    // para garantir que o token de cancelamento seja respeitado mesmo se a conexão
    // HTTP original já tiver sido encerrada.
    public async Task CancelChatAsync(string? connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId)) return;
        var baseUrl = _uri;
        var requestUri = $"{baseUrl}/chat/cancel";
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
