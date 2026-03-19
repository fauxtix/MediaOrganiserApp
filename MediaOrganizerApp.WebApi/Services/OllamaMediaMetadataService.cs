using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediaOrganizerApp.WebApi.Hubs;

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

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(80));

            // Progress: waiting for AI response
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Waiting for AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Waiting for AI response...");
            }

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var json = Encoding.UTF8.GetString(bytes);

            // Progress: parsing AI response
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Parsing AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Parsing AI response...");
            }

            var text = TryExtractOllamaResponseText(json);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "Empty LLM response." };

            // Attempt parse as-is.
            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    LogProgress("Done!");
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    // Show warnings/errors as final progress message
                    if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, parsed.Warnings);
                }
                return parsed;
            }

            // Attempt recovery by normalizing common JSON issues.
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

                normalized = TryNormalizeJson(extractedJson);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    parsed = TryParseJsonContract(normalized);
                    if (parsed is not null)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveProgress", null, "Done!");
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

    private static string? TryNormalizeJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var s = text;

        // Some models return literal escape sequences ("\n") inside the JSON text.
        // Converting them to real newlines can help the JSON parser.
        s = s.Replace("\\r", "\r").Replace("\\n", "\n");

        // Remove trailing commas before object/array end.
        // Example: {"a":1,} or {"a":1,\n}
        s = System.Text.RegularExpressions.Regex.Replace(s, @",\s*([}\]])", "$1");

        // Some models accidentally emit an extra quote right after the final field
        // and before the trailing comma, e.g.:  "warnings":"",}\n
        s = System.Text.RegularExpressions.Regex.Replace(s, @"""\s*,\s*([}\]])", "$1");

        return s.Trim();
    }

    // Prompt text is stored as external templates under Prompts/.

    private static AiMediaMetadataResponseDto? TryParseJsonContract(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<AiMediaMetadataResponseDto>(text, JsonOptions);
        }
        catch
        {
            // LLMs may duplicate keys or include extra keys, which makes strict JSON parsing fail.
            // As a fallback, try to extract only the known schema fields from the first JSON object.
            try
            {
                var objText = TryExtractFirstJsonObject(text) ?? text;
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

            // Unknown keys are ignored by reading only the expected ones.
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

    public async Task<string?> GetMediaEditionYearAsync(
        string artist,
        string title,
        string language,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 1/2: Preparing AI prompt for edition year...", cancellationToken);
        var template = await _prompts.GetTemplateAsync("media-edition-year.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["artist"] = artist,
            ["title"] = title
        });
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 2/2: Sending prompt to AI model...", cancellationToken);
        var year = await ExecuteOllamaEditionYearAsync(prompt, connectionId, cancellationToken);
        if (string.IsNullOrWhiteSpace(year))
            return null;
        // Extract only a 4-digit year or 'N/A' from the AI response
        var match = System.Text.RegularExpressions.Regex.Match(year, @"\b(\d{4})\b");
        if (match.Success)
            return match.Value;
        if (year.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
            return "N/A";
        return null;
    }

    private async Task<string?> ExecuteOllamaEditionYearAsync(string prompt, string? connectionId, CancellationToken cancellationToken)
    {
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "llama3.2";
        var payload = new { model, prompt, stream = false };
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
            var text = TryExtractOllamaResponseText(json);
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
}
