using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.SignalR;
using MediaOrganizerApp.WebApi.Hubs;
using System.Text.RegularExpressions;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaMetadataService : IAiMediaMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaMediaMetadataService> _logger;
    private readonly IConfiguration _config;
    private readonly IPromptTemplateProvider _prompts;
    private readonly IHubContext<MediaInfoProgressHub> _hubContext;

    public OllamaMediaMetadataService(HttpClient httpClient, ILogger<OllamaMediaMetadataService> logger, IConfiguration config, IPromptTemplateProvider prompts, IHubContext<MediaInfoProgressHub> hubContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        _prompts = prompts;
        _hubContext = hubContext;
    }

    public async Task<AiMediaMetadataResponseDto> GetMediaMetadataAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 1/3: Preparing AI prompt...", cancellationToken);

        var template = await _prompts.GetTemplateAsync("media-metadata.prompt.txt", cancellationToken);
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

    private async Task<AiMediaMetadataResponseDto> ExecuteOllamaAsync(string prompt, string? connectionId, CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation($"Progress for client {connectionId}: {msg}");
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

        // ✅ Added temperature and deterministic options
        var payload = new
        {
            model,
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
            if (_hubContext != null && !string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Sending prompt to AI model...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Sending prompt to AI model...");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(80));

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Waiting for AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Waiting for AI response...");
            }

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var json = Encoding.UTF8.GetString(bytes);

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Parsing AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Parsing AI response...");
            }

            var text = OllamaResponseHelper.TryExtractResponseText(json);

            // ✅ Log raw response for debugging
            _logger.LogInformation("AI raw response: {Response}", text);

            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "Empty LLM response." };

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

            var normalized = TryNormalizeJson(text);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                parsed = TryParseJsonContract(normalized);
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

            var extractedJson = OllamaResponseHelper.TryExtractFirstJsonObject(text);
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

                normalized = TryNormalizeJson(extractedJson);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    parsed = TryParseJsonContract(normalized);
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
            }

            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Done!");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
            }
            return new AiMediaMetadataResponseDto
            {
                Confidence = 0,
                Warnings = $"LLM output was not valid JSON. Raw: {text.Trim()}"
            };
        }
        catch (OperationCanceledException)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("AI request timed out or was canceled.");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "AI request timed out or was canceled.");
            }
            return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "AI request timed out or was canceled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media metadata using Ollama.");
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress($"Error: {ex.Message}");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, $"Error: {ex.Message}");
            }
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
     CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveProgress", connectionId, "Step 1/2: Preparing AI prompt for edition year...", cancellationToken);

        // Robust dynamic hint for the AI
        var additionalInfo = $"Release year not found in external databases for '{title}' by '{artist}'. Only return the year if certain.";

        var template = await _prompts.GetTemplateAsync("media-edition-year.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["artist"] = artist,
            ["title"] = title,
            ["AdditionalInfo"] = additionalInfo
        });

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveProgress", connectionId, "Step 2/2: Sending prompt to AI model...", cancellationToken);

        var rawResponse = await ExecuteOllamaEditionYearAsync(prompt, connectionId, cancellationToken);

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

    private async Task<string?> ExecuteOllamaEditionYearAsync(string prompt, string? connectionId, CancellationToken cancellationToken)
    {
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

        // ✅ Added temperature and deterministic options
        var payload = new
        {
            model,
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
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
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

            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media edition year using Ollama.");
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
}