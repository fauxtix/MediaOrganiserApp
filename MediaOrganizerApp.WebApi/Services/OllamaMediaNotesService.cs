using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaNotesService : OllamaBaseClient, IAiMediaNotesService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<OllamaMediaNotesService> _logger;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaMediaNotesService(
        HttpClient httpClient,
        ILogger<OllamaMediaNotesService> logger,
        IPromptTemplateProvider prompts,
        IProgressPublisher progressPublisher,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService)
        : base(httpClient, aiSettingsProvider, systemPromptService, prompts)
    {
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public async Task<AiMediaNotesResponseDto> GetMediaNotesAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Step 1/3: Preparing AI prompt...", cancellationToken);
        var fullPrompt = await BuildFullPromptAsync("media-notes.prompt.txt", new Dictionary<string, string?>
        {
            ["language"] = language,
            ["artist"] = artist,
            ["title"] = title
        }, cancellationToken);

        await PublishAsync(connectionId, correlationId, "request_sent", 35, "Step 2/3: Sending prompt to AI model...", cancellationToken);
        var result = await ExecuteOllamaAsync(fullPrompt, artist, title, connectionId, correlationId, cancellationToken);
        await PublishAsync(connectionId, correlationId, "parsing", 85, "Step 3/3: Processing AI response...", cancellationToken);
        return result;
    }

    private async Task<AiMediaNotesResponseDto> ExecuteOllamaAsync(
        string prompt,
        string artist,
        string title,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation("Progress for client {ConnectionId}: {Message}", connectionId, msg);

        try
        {
            LogProgress("Waiting for AI response...");
            await PublishAsync(connectionId, correlationId, "waiting_response", 65, "Waiting for AI response...", cancellationToken);

            LogProgress("Parsing AI response...");
            await PublishAsync(connectionId, correlationId, "parsing", 80, "Parsing AI response...", cancellationToken);

            var text = await ExecuteGenerateAsync(prompt, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                var notesValid = !string.IsNullOrWhiteSpace(parsed.Notes)
                    && parsed.Notes.IndexOf(parsed.Artist ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                    && parsed.Notes.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                var confidenceLow = parsed.Confidence < 0.5;
                parsed.Warnings = notesValid && !confidenceLow
                    ? string.Empty
                    : $"AI notes may not be accurate for '{parsed.Artist}' - '{parsed.Title}'.";

                LogProgress("Done!");
                await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                    await PublishAsync(connectionId, correlationId, "warning", 100, parsed.Warnings, cancellationToken, "warning");
                return parsed;
            }

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
                        var notesValidRecovered = !string.IsNullOrWhiteSpace(recovered)
                            && recovered.IndexOf(artist, StringComparison.OrdinalIgnoreCase) >= 0
                            && recovered.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0;

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

            var notesPattern = new System.Text.RegularExpressions.Regex("\"notes\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            var notesMatch = notesPattern.Match(text);
            if (notesMatch.Success)
            {
                var raw = notesMatch.Groups[1].Value;
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(raw);
                _logger.LogInformation("Recovered notes from malformed JSON (regex): {Notes}", unescaped);
                var notesValidMalformed = !string.IsNullOrWhiteSpace(unescaped)
                    && unescaped.IndexOf(artist, StringComparison.OrdinalIgnoreCase) >= 0
                    && unescaped.IndexOf(title, StringComparison.OrdinalIgnoreCase) >= 0;

                return new AiMediaNotesResponseDto
                {
                    Notes = unescaped,
                    Confidence = 0,
                    Warnings = notesValidMalformed ? string.Empty : $"AI notes may not be accurate for '{artist}' - '{title}'."
                };
            }

            _logger.LogWarning("Could not recover notes from AI response: {Text}", text);
            return new AiMediaNotesResponseDto
            {
                Notes = string.Empty,
                Confidence = 0,
                Warnings = "AI response could not be parsed. No notes available."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media notes using Ollama.");
            LogProgress($"Error: {ex.Message}");
            await PublishAsync(connectionId, correlationId, "failed", 100, $"Error: {ex.Message}", cancellationToken, "error");
            return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = ex.Message };
        }
    }

    private Task PublishAsync(
        string? connectionId,
        string? correlationId,
        string stage,
        int percent,
        string message,
        CancellationToken cancellationToken,
        string level = "info")
        => _progressPublisher.PublishAsync(
            ProgressChannel.MediaInfo,
            new ProgressUpdateDto
            {
                CorrelationId = correlationId,
                Stage = stage,
                Percent = percent,
                Message = message,
                Level = level,
                Service = nameof(OllamaMediaNotesService),
                Operation = "media-notes"
            },
            connectionId,
            cancellationToken);

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
