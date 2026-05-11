using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaMetadataService : IAiMediaMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaMediaMetadataService> _logger;
    private string _baseUrl;
    private string _model;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaMediaMetadataService(HttpClient httpClient,
                                      ILogger<OllamaMediaMetadataService> logger,
                                      IPromptTemplateProvider prompts,
                                      IProgressPublisher progressPublisher,
                                      IAppSettingsRepository appSettingsRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _prompts = prompts;
        _progressPublisher = progressPublisher;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<AiMediaMetadataResponseDto> GetMediaMetadataAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Step 1/3: Preparing AI prompt...", cancellationToken);

        var systemTemplate = await _prompts.GetTemplateAsync("system.prompt.txt", cancellationToken);
        var template = await _prompts.GetTemplateAsync("media-metadata.prompt.txt", cancellationToken);
        var systemPrompt = PromptTemplateRenderer.Render(systemTemplate, new Dictionary<string, string?>());

        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["artist"] = artist,
            ["title"] = title
        });

        var fullPrompt = $"{systemPrompt}\n\n{prompt}";

        await PublishAsync(connectionId, correlationId, "request_sent", 35, "Step 2/3: Sending prompt to AI model...", cancellationToken);

        var result = await ExecuteOllamaAsync(fullPrompt, connectionId, correlationId, cancellationToken);

        await PublishAsync(connectionId, correlationId, "parsing", 85, "Step 3/3: Processing AI response...", cancellationToken);

        return result;
    }

    private async Task<AiMediaMetadataResponseDto> ExecuteOllamaAsync(
        string prompt,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation($"Progress for client {connectionId}: {msg}");
        var (modelBaseUrl, modelName) = await GetAiSettings();
        _baseUrl = modelBaseUrl;
        _model = modelName;

        // ✅ Added temperature and deterministic options
        var payload = new
        {
            model = _model,
            prompt,
            stream = false,
            options = new
            {
                temperature = 0,
                top_p = 1,
            }
        };

        try
        {
            LogProgress("Sending prompt to AI model...");
            await PublishAsync(connectionId, correlationId, "request_sent", 45, "Sending prompt to AI model...", cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(80));

            LogProgress("Waiting for AI response...");
            await PublishAsync(connectionId, correlationId, "waiting_response", 65, "Waiting for AI response...", cancellationToken);

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var json = Encoding.UTF8.GetString(bytes);

            LogProgress("Parsing AI response...");
            await PublishAsync(connectionId, correlationId, "parsing", 80, "Parsing AI response...", cancellationToken);

            var text = OllamaResponseHelper.TryExtractResponseText(json);

            // ✅ Log raw response for debugging
            _logger.LogInformation("AI raw response: {Response}", text);

            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                LogProgress("Done!");
                await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                    await PublishAsync(connectionId, correlationId, "warning", 100, parsed.Warnings, cancellationToken, "warning");
                return parsed;
            }

            var normalized = TryNormalizeJson(text);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                parsed = TryParseJsonContract(normalized);
                if (parsed is not null)
                {
                    LogProgress("Done!");
                    await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                    return parsed;
                }
            }

            var extractedJson = OllamaResponseHelper.TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                {
                    LogProgress("Done!");
                    await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                    return parsed;
                }

                normalized = TryNormalizeJson(extractedJson);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    parsed = TryParseJsonContract(normalized);
                    if (parsed is not null)
                    {
                        LogProgress("Done!");
                        await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                        return parsed;
                    }
                }
            }

            LogProgress("Done!");
            await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
            return new AiMediaMetadataResponseDto
            {
                Confidence = 0,
                Warnings = $"LLM output was not valid JSON. Raw: {text.Trim()}"
            };
        }
        catch (OperationCanceledException)
        {
            LogProgress("AI request timed out or was canceled.");
            await PublishAsync(connectionId, correlationId, "cancelled", 100, "AI request timed out or was canceled.", cancellationToken, "warning");
            return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "AI request timed out or was canceled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media metadata using Ollama.");
            LogProgress($"Error: {ex.Message}");
            await PublishAsync(connectionId, correlationId, "failed", 100, $"Error: {ex.Message}", cancellationToken, "error");
            return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = ex.Message };
        }
    }

    // --------------------- KEEP ALL ORIGINAL HELPERS ---------------------
    private static string? TryNormalizeJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var s = text;
        s = s.Replace("\\r", "\r").Replace("\\n", "\n");
        s = Regex.Replace(s, @",\s*([}\]])", "$1");
        s = Regex.Replace(s, @"""\s*,\s*([}\]])", "$1");
        return s.Trim();
    }

    private static AiMediaMetadataResponseDto? TryParseJsonContract(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<AiMediaMetadataResponseDto>(text, JsonOptions);
        }
        catch
        {
            try
            {
                var objText = OllamaResponseHelper.TryExtractFirstJsonObject(text) ?? text;
                return TryParseLenientContract(objText);
            }
            catch
            {
                return null;
            }
        }
    }

    private static AiMediaMetadataResponseDto? TryParseLenientContract(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            var root = doc.RootElement;

            static string? GetStringOrNull(JsonElement obj, string name)
            {
                if (!obj.TryGetProperty(name, out var p) || p.ValueKind == JsonValueKind.Null)
                    return null;
                return p.GetString();
            }

            static double? GetDoubleOrNull(JsonElement obj, string name)
            {
                if (!obj.TryGetProperty(name, out var p) || p.ValueKind == JsonValueKind.Null)
                    return null;

                if (p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var d))
                    return d;

                if (p.ValueKind == JsonValueKind.String && double.TryParse(p.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var ds))
                    return ds;

                return null;
            }

            return new AiMediaMetadataResponseDto
            {
                Artist = GetStringOrNull(root, "artist"),
                Title = GetStringOrNull(root, "title"),
                Runtime = GetStringOrNull(root, "runtime"),
                Released = GetStringOrNull(root, "released"),
                ImdbRating = GetStringOrNull(root, "imdbRating"),
                Actors = GetStringOrNull(root, "actors"),
                Year = GetStringOrNull(root, "year"),
                Poster = GetStringOrNull(root, "poster"),
                Tempo = GetStringOrNull(root, "tempo"),
                CoverFile = GetStringOrNull(root, "coverFile"),
                Notes = GetStringOrNull(root, "notes"),
                Confidence = GetDoubleOrNull(root, "confidence"),
                Warnings = GetStringOrNull(root, "warnings")
            };
        }
        catch
        {
            return null;
        }
    }

    // --------------------- UPDATED Media Edition Year ---------------------
    public async Task<string?> GetMediaEditionYearAsync(
     string artist,
     string title,
     string language,
     string? connectionId,
     string? correlationId,
     CancellationToken cancellationToken)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Step 1/2: Preparing AI prompt for edition year...", cancellationToken);

        // Robust dynamic hint for the AI
        var additionalInfo = $"Release year not found in external databases for '{title}' by '{artist}'. Only return the year if certain.";

        var template = await _prompts.GetTemplateAsync("media-edition-year.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["Artist"] = artist,
            ["Title"] = title,
            ["AdditionalInfo"] = additionalInfo
        });

        await PublishAsync(connectionId, correlationId, "request_sent", 45, "Step 2/2: Sending prompt to AI model...", cancellationToken);

        var rawResponse = await ExecuteOllamaEditionYearAsync(prompt, connectionId, correlationId, cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
            return null;

        var cleaned = rawResponse.Trim().Trim('"');

        // Split into lines
        var lines = cleaned.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Step 1: If any line contains "N/A", return "N/A"
        if (lines.Any(l => l.Contains("N/A", StringComparison.OrdinalIgnoreCase)))
            return "N/A";

        // Step 2: Otherwise, search for a 4-digit year in all lines
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"\b(\d{4})\b");
            if (match.Success)
                return match.Value;
        }

        return null;
    }

    private async Task<string?> ExecuteOllamaEditionYearAsync(
        string prompt,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var (modelBaseUrl, modelName) = await GetAiSettings();
        _baseUrl = modelBaseUrl;
        _model = modelName;

        // ✅ Added temperature and deterministic options
        var payload = new
        {
            model = _model,
            prompt,
            stream = false,
            options = new
            {
                temperature = 0,
                top_p = 1,
                num_predict = 20
            }
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var json = Encoding.UTF8.GetString(bytes);

            var text = OllamaResponseHelper.TryExtractResponseText(json);

            // ✅ Log raw response for debugging
            _logger.LogInformation("AI raw year response: {Response}", text);

            if (string.IsNullOrWhiteSpace(text))
                return null;

            await PublishAsync(connectionId, correlationId, "completed", 100, "Edition year response processed.", cancellationToken);
            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media edition year using Ollama.");
            await PublishAsync(connectionId, correlationId, "failed", 100, ex.Message, cancellationToken, "error");
            return null;
        }
    }

    // --------------------- Helper: Clean Title ---------------------
    private string CleanTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title;

        return title
            .Replace("Deluxe Edition", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Remastered", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Special Edition", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }
    private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return (settings.ModelBaseUrl, settings.AiModelName);
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
                Service = nameof(OllamaMediaMetadataService),
                Operation = "media-metadata"
            },
            connectionId,
            cancellationToken);

}
