using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MediaOrganizerApp.WebApi.Hubs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;

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
        var result = await ExecuteOllamaAsync(prompt, artist, title, connectionId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 3/3: Processing AI response...", cancellationToken);
        return result;
    }

private async Task<AiMediaNotesResponseDto> ExecuteOllamaAsync(string prompt, string artist, string title, string? connectionId, CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation($"Progress for client {connectionId}: {msg}");
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "phi4-mini:latest";

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

            var text = OllamaResponseHelper.TryExtractResponseText(json);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                // Validate notes: must mention artist and title, and confidence should be reasonable
                bool notesValid = !string.IsNullOrWhiteSpace(parsed.Notes)
                    && parsed.Notes.IndexOf(parsed.Artist ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                    && parsed.Notes.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                bool confidenceLow = parsed.Confidence < 0.5;
                if (notesValid && !confidenceLow)
                {
                    parsed.Warnings = string.Empty;
                }
                else
                {
                    parsed.Warnings = $"AI notes may not be accurate for '{parsed.Artist}' - '{parsed.Title}'.";
                }
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    LogProgress("Done!");
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, parsed.Warnings);
                }
                return parsed;
            }

            // Attempt to recover 'notes' from any embedded JSON object first
            var extractedJson = OllamaResponseHelper.TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(extractedJson);
                    if (doc.RootElement.TryGetProperty("notes", out var notesProp) && notesProp.ValueKind == JsonValueKind.String)
                    {
                        var recovered = notesProp.GetString() ?? string.Empty;
                        _logger.LogInformation("Recovered notes from embedded JSON object.");
                        bool notesValidRecovered = !string.IsNullOrWhiteSpace(recovered)
                            && recovered.IndexOf(artist ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                            && recovered.IndexOf(title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                        return new AiMediaNotesResponseDto
                        {
                            Notes = recovered,
                            Confidence = 0,
                            Warnings = notesValidRecovered ? string.Empty : $"AI notes may not be accurate for '{artist}' - '{title}'."
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to parse extracted JSON object while recovering notes.");
                }
            }

            // Last-resort: robust regex that allows escaped chars inside the string
            var notesPattern = new System.Text.RegularExpressions.Regex("\"notes\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            var notesMatch = notesPattern.Match(text);
            if (notesMatch.Success)
            {
                // Unescape JSON-style escapes
                var raw = notesMatch.Groups[1].Value;
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(raw);
                _logger.LogInformation($"Recovered notes from malformed JSON (regex): {unescaped}");
                bool notesValidMalformed = !string.IsNullOrWhiteSpace(unescaped)
                    && unescaped.IndexOf(artist ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                    && unescaped.IndexOf(title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                return new AiMediaNotesResponseDto
                {
                    Notes = unescaped,
                    Confidence = 0,
                    Warnings = notesValidMalformed ? string.Empty : $"AI notes may not be accurate for '{artist}' - '{title}'."
                };
            }
            else
            {
                _logger.LogWarning($"Could not recover notes from AI response: {text}");
                return new AiMediaNotesResponseDto
                {
                    Notes = string.Empty,
                    Confidence = 0,
                    Warnings = "AI response could not be parsed. No notes available."
                };
            }
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
}
