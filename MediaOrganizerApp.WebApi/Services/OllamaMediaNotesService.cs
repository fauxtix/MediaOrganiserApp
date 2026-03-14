using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaMediaNotesService : IAiMediaNotesService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaMediaNotesService> _logger;
    private readonly IConfiguration _config;

    public OllamaMediaNotesService(HttpClient httpClient, ILogger<OllamaMediaNotesService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<AiMediaNotesResponseDto> GetMediaNotesAsync(
        string artist,
        string title,
        string language,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPromptMediaNotes(artist, title, language);
        return await ExecuteOllamaAsync(prompt, cancellationToken);
    }

    private async Task<AiMediaNotesResponseDto> ExecuteOllamaAsync(string prompt, CancellationToken cancellationToken)
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

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes);

            var text = TryExtractOllamaResponseText(json);
            if (string.IsNullOrWhiteSpace(text))
                return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
                return parsed;

            var extractedJson = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                    return parsed;
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
            return new AiMediaNotesResponseDto { Notes = string.Empty, Confidence = 0, Warnings = ex.Message };
        }
    }

    private static string BuildPromptMediaNotes(string artist, string title, string language)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a media metadata assistant.");
        sb.AppendLine("Return ONLY valid JSON (no markdown, no code fences)." );
        sb.AppendLine("JSON schema: {\"artist\":string,\"title\":string,\"notes\":string,\"confidence\":number,\"warnings\":string}");
        sb.AppendLine("Rules:");
        sb.AppendLine("- The response must be about the exact provided Author and Title.");
        sb.AppendLine("- Echo the EXACT input values in the JSON fields 'artist' and 'title' (do not translate, do not normalize).");
        sb.AppendLine("- notes: plain-text liner notes / awards / key highlights, 300-800 characters.");
        sb.AppendLine("  - For albums: mention genre/style; mention tracks only if confident.");
        sb.AppendLine("  - For movies/films: mention major awards/nominations only if confident.");
        sb.AppendLine("- Do NOT write instructional/meta text in notes (e.g., 'this is the notes').");
        sb.AppendLine("- Do not include HTML.");
        sb.AppendLine("- If unsure about the match: lower confidence, add a brief warning in 'warnings' (<= 200 chars), and keep notes generic.");
        sb.AppendLine("- Do not invent specific awards, dates, record labels, publishers, track lists, cast, or plot specifics unless confident.");
        sb.AppendLine("- Provide a confidence score between 0.0 and 1.0.");
        sb.AppendLine();
        sb.AppendLine("INPUT (must match exactly):");
        sb.AppendLine($"Language: {language}");
        sb.AppendLine($"Author: {artist}");
        sb.AppendLine($"Title: {title}");
        return sb.ToString();
    }

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
