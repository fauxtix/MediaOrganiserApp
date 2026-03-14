using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaBookSummaryService : IAiBookSummaryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaBookSummaryService> _logger;
    private readonly IConfiguration _config;

    public OllamaBookSummaryService(HttpClient httpClient, ILogger<OllamaBookSummaryService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
    {
        var baseUrl = _config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = _config["Ai:Ollama:Model"] ?? "llama3.2";

        var prompt = BuildPromptFull(title, author, language);

        return await ExecuteOllamaAsync(prompt, cancellationToken);
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPromptSummaryOnly(title, author, language);

        return await ExecuteOllamaAsync(prompt, cancellationToken);
    }

    private async Task<AiBookSummaryResponseDto> ExecuteOllamaAsync(string prompt, CancellationToken cancellationToken)
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
                return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Empty LLM response." };

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
                return parsed;

            // Attempt to recover when the model adds extra text around the JSON.
            var extractedJson = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                    return parsed;
            }

            return new AiBookSummaryResponseDto
            {
                Summary = string.Empty,
                Confidence = 0,
                Notes = $"LLM output was not valid JSON. Raw: {text.Trim()}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary using Ollama.");
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = ex.Message };
        }
    }

    private static string BuildPromptSummaryOnly(string title, string? author, string language)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a bibliographic assistant.");
        sb.AppendLine("Return ONLY valid JSON (no markdown, no code fences)." );
        sb.AppendLine("JSON schema: {\"title\":string,\"author\":string,\"summary\":string,\"confidence\":number,\"notes\":string}");
        sb.AppendLine("Rules:");
        sb.AppendLine("- summary: provide a plain-text book description/synopsis for the requested title, 600-1200 characters.");
        sb.AppendLine("  Do NOT write instructional/meta text in summary (e.g., 'This is the actual book description').");
        sb.AppendLine("  Do NOT put the book description or plot into 'notes'.");
        sb.AppendLine("- If you don't know a field, set it to null (except summary, which can be empty string).");
        sb.AppendLine("- Do not include HTML.");
        sb.AppendLine("- notes: short meta-only message (<= 200 characters). Use notes ONLY for: title/author match uncertainty OR output/format issues.");
        sb.AppendLine("  Do NOT include plot, synopsis, publisher, year, awards, or any extra bibliographic details in notes.");
        sb.AppendLine("- If unsure, lower confidence and mention uncertainty briefly in notes.");
        sb.AppendLine("- IMPORTANT: The response must be about the exact provided Title and Author.");
        sb.AppendLine("- Echo the EXACT input values in the JSON fields 'title' and 'author' (do not translate, do not normalize).");
        sb.AppendLine("- If you are not fully sure about the match, you may still provide a best-effort summary, BUT:");
        sb.AppendLine("  - Lower confidence." );
        sb.AppendLine("  - Explain uncertainty briefly in notes (title/author match only)." );
        sb.AppendLine("  - Do not invent detailed facts (publisher, year, awards, plot specifics) unless you are confident." );
        sb.AppendLine("- Provide a confidence score between 0.0 and 1.0.");
        sb.AppendLine();
        sb.AppendLine("INPUT (must match exactly):");
        sb.AppendLine($"Language: {language}");
        sb.AppendLine($"Title: {title}");
        sb.AppendLine($"Author: {(string.IsNullOrWhiteSpace(author) ? "(none)" : author)}");

        return sb.ToString();
    }

    private static string BuildPromptFull(string title, string? author, string language)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a bibliographic assistant.");
        sb.AppendLine("Return ONLY valid JSON (no markdown, no code fences)." );
        sb.AppendLine("JSON schema: {\"title\":string,\"author\":string,\"category\":string,\"publishedDate\":string,\"pages\":number,\"coverImageUrl\":string,\"price\":number,\"summary\":string,\"confidence\":number,\"notes\":string}");
        sb.AppendLine("Rules:");
        sb.AppendLine("- summary: provide a plain-text book description/synopsis for the requested title, 600-1200 characters.");
        sb.AppendLine("  Do NOT write instructional/meta text in summary (e.g., 'This is the actual book description').");
        sb.AppendLine("  Do NOT put the book description or plot into 'notes'.");
        sb.AppendLine("- If you don't know a field, set it to null (except summary, which can be empty string).");
        sb.AppendLine("- Do not include HTML.");
        sb.AppendLine("- notes: short meta-only message (<= 200 characters). Use notes ONLY for: title/author match uncertainty OR output/format issues.");
        sb.AppendLine("  Do NOT include plot, synopsis, publisher, year, awards, or any extra bibliographic details in notes.");
        sb.AppendLine("- If unsure, lower confidence and mention uncertainty briefly in notes.");
        sb.AppendLine("- IMPORTANT: The response must be about the exact provided Title and Author.");
        sb.AppendLine("- Echo the EXACT input values in the JSON fields 'title' and 'author' (do not translate, do not normalize).");
        sb.AppendLine("- If you are not fully sure about the match, you may still provide a best-effort output, BUT:");
        sb.AppendLine("  - Lower confidence." );
        sb.AppendLine("  - Explain uncertainty briefly in notes (title/author match only)." );
        sb.AppendLine("  - Prefer null for unknown fields instead of guessing." );
        sb.AppendLine("  - Do not invent detailed facts (publisher, year, awards, plot specifics) unless you are confident." );
        sb.AppendLine("- Provide a confidence score between 0.0 and 1.0.");
        sb.AppendLine();
        sb.AppendLine("INPUT (must match exactly):");
        sb.AppendLine($"Language: {language}");
        sb.AppendLine($"Title: {title}");
        sb.AppendLine($"Author: {(string.IsNullOrWhiteSpace(author) ? "(none)" : author)}");

        return sb.ToString();
    }

    private static AiBookSummaryResponseDto? TryParseJsonContract(string text)
    {
        try
        {
            return JsonSerializer.Deserialize<AiBookSummaryResponseDto>(text, JsonOptions);
        }
        catch
        {
            return null;
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

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
                depth++;
            else if (ch == '}')
                depth--;

            if (depth == 0)
                return text.Substring(start, i - start + 1);
        }

        return null;
    }

    private static string? TryExtractOllamaResponseText(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
                return response.GetString();
        }
        catch
        {
            // ignored
        }

        // NDJSON fallback
        var sb = new StringBuilder();
        foreach (var line in json.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
                    sb.Append(response.GetString());
            }
            catch
            {
                // ignored
            }
        }

        var merged = sb.ToString();
        return string.IsNullOrWhiteSpace(merged) ? null : merged;
    }
}
