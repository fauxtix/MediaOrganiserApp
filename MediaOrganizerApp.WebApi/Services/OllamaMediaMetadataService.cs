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

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaMetadataService : IAiMediaMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaMediaMetadataService> _logger;
    private readonly IConfiguration _config;

    public OllamaMediaMetadataService(HttpClient httpClient, ILogger<OllamaMediaMetadataService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<AiMediaMetadataResponseDto> GetMediaMetadataAsync(
        string artist,
        string title,
        string language,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPromptMediaMetadata(artist, title, language);
        return await ExecuteOllamaAsync(prompt, cancellationToken);
    }

    private async Task<AiMediaMetadataResponseDto> ExecuteOllamaAsync(string prompt, CancellationToken cancellationToken)
    {
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
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(80));

            using var response = await _httpClient.SendAsync(request, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
            var json = Encoding.UTF8.GetString(bytes);

            var text = TryExtractOllamaResponseText(json);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "Empty LLM response." };

            // Attempt parse as-is.
            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
                return parsed;

            // Attempt recovery by normalizing common JSON issues.
            var normalized = TryNormalizeJson(text);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                parsed = TryParseJsonContract(normalized);
                if (parsed is not null)
                    return parsed;
            }

            var extractedJson = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                    return parsed;

                normalized = TryNormalizeJson(extractedJson);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    parsed = TryParseJsonContract(normalized);
                    if (parsed is not null)
                        return parsed;
                }
            }

            return new AiMediaMetadataResponseDto
            {
                Confidence = 0,
                Warnings = $"LLM output was not valid JSON. Raw: {text.Trim()}"
            };
        }
        catch (OperationCanceledException)
        {
            return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = "AI request timed out or was canceled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating media metadata using Ollama.");
            return new AiMediaMetadataResponseDto { Confidence = 0, Warnings = ex.Message };
        }

        // not reached
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

    private static string BuildPromptMediaMetadata(string artist, string title, string language)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a media metadata assistant.");
        sb.AppendLine("Return ONLY valid JSON (no markdown, no code fences)." );
        sb.AppendLine("JSON schema: {\"artist\":string,\"title\":string,\"runtime\":string,\"released\":string,\"imdbRating\":string,\"actors\":string,\"year\":string,\"poster\":string,\"tempo\":string,\"coverFile\":string,\"notes\":string,\"confidence\":number,\"warnings\":string}");
        sb.AppendLine("Rules:");
        sb.AppendLine("- The response must be about the exact provided Author and Title.");
        sb.AppendLine("- Echo the EXACT input values in the JSON fields 'artist' and 'title' (do not translate, do not normalize).");
        sb.AppendLine("- Use EXACTLY the JSON schema keys and casing shown above. Do not add extra keys (e.g., do not output 'MediaType').");
        sb.AppendLine("- Do not duplicate keys in the JSON object.");
        sb.AppendLine("- Do not include HTML.");
        sb.AppendLine("- Do NOT write instructional/meta text as data.");
        sb.AppendLine("- You do NOT have browsing or external database access. Do not guess facts.");
        sb.AppendLine("- Only output non-null factual fields (actors/runtime/released/imdbRating/year/poster/tempo/coverFile) if you are highly confident they are correct for the exact title.");
        sb.AppendLine("- If you are not highly confident about a factual field: set it to null and add a short reason to 'warnings'.");
        sb.AppendLine("- Do NOT claim a work is based on/inspired by a novel/book/true story or name a source author unless that exact source information was provided in the INPUT.");
        sb.AppendLine("- Prefer movie/film-style fields when the title looks like a film and album-style fields when the title looks like an album.");
        sb.AppendLine("  - Movies/films: runtime/released/imdbRating/actors/year/poster/notes.");
        sb.AppendLine("  - Albums: tempo/coverFile/notes.");
        sb.AppendLine("- If you don't know a field with confidence, set it to null.");
        sb.AppendLine("- notes: plain text, 300-800 characters; awards/highlights only if confident.");
        sb.AppendLine("- notes must be generic and must NOT include specific names/dates/credits unless they were present in the INPUT.");
        sb.AppendLine("- If unsure about the match: lower confidence and put a brief warning in 'warnings' (<= 200 chars). Keep content generic.");
        sb.AppendLine("- Provide a confidence score between 0.0 and 1.0.");
        sb.AppendLine();
        sb.AppendLine("INPUT (must match exactly):");
        sb.AppendLine($"Language: {language}");
        sb.AppendLine($"Author: {artist}");
        sb.AppendLine($"Title: {title}");
        return sb.ToString();
    }

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
}
