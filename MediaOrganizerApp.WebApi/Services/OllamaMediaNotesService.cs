using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediaOrganizerApp.WebApi.Hubs;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaNotesService : IAiMediaNotesService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaMediaNotesService> _logger;
    private readonly IConfiguration _config;
    private readonly IPromptTemplateProvider _prompts;

    private readonly IHubContext<MediaInfoProgressHub> _hubContext;

    public OllamaMediaNotesService(HttpClient httpClient, ILogger<OllamaMediaNotesService> logger, IConfiguration config, IPromptTemplateProvider prompts, IHubContext<MediaInfoProgressHub> hubContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        _prompts = prompts;
        _hubContext = hubContext;
    }

    public async Task<AiMediaNotesResponseDto> GetMediaNotesAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 1/3: Preparing AI prompt...", cancellationToken);
        var template = await _prompts.GetTemplateAsync("media-notes.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["artist"] = artist,
            ["title"] = title
        });
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 2/3: Sending prompt to AI model...", cancellationToken);
        var result = await ExecuteOllamaAsync(prompt, connectionId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 3/3: Processing AI response...", cancellationToken);
        return result;
    }

    private async Task<AiMediaNotesResponseDto> ExecuteOllamaAsync(string prompt, string? connectionId, CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation($"Progress for client {connectionId}: {msg}");
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

        var payload = new
        {
            model,
            prompt,
            stream = false
        };

        try
        {
            // Progress: sending prompt
            if (_hubContext != null && !string.IsNullOrWhiteSpace(prompt) && !string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Sending prompt to AI model...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Sending prompt to AI model...");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            // Progress: waiting for AI response
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Waiting for AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Waiting for AI response...");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes);

            // Progress: parsing AI response
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Parsing AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Parsing AI response...");
            }

            var text = TryExtractOllamaResponseText(json);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    LogProgress("Done!");
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, parsed.Warnings);
                }
                return parsed;
            }

            var extractedJson = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                {
                    if (!string.IsNullOrWhiteSpace(connectionId))
                    {
                        LogProgress("Done!");
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    }
                    return parsed;
                }
            }

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Done!");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
            }
            return new AiMediaNotesResponseDto
            {
                Notes = string.Empty,
                Confidence = 0,
                Warnings = $"LLM output was not valid JSON. Raw: {text.Trim()}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media notes using Ollama.");
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress($"Error: {ex.Message}");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, $"Error: {ex.Message}");
            }
            return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = ex.Message };
        }
    }

    // Prompt text is stored as external templates under Prompts/.

    private static AiMediaNotesResponseDto? TryParseJsonContract(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<AiMediaNotesResponseDto>(text, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractOllamaResponseText(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("response", out var resp))
                return resp.GetString();

            return json;
        }
        catch
        {
            // Ollama can occasionally return NDJSON. Attempt simple extraction of "response" chunks.
            var lines = json.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("response", out var resp))
                        sb.Append(resp.GetString());
                }
                catch
                {
                    // ignore
                }
            }

            var merged = sb.ToString();
            return string.IsNullOrWhiteSpace(merged) ? null : merged;
        }
    }

    private static string? TryExtractFirstJsonObject(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var start = text.IndexOf('{');
        if (start < 0)
            return null;

        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var ch = text[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '\"')
                    inString = false;

                continue;
            }

            if (ch == '\"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
                depth++;
            else if (ch == '}')
            {
                depth--;
                if (depth == 0)
                    return text.Substring(start, i - start + 1);
            }
        }

        return null;
    }
}
