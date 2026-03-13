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

        var prompt = BuildPrompt(title, author, language);

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

            return new AiBookSummaryResponseDto
            {
                Summary = text.Trim(),
                Confidence = 0.5,
                Notes = "LLM output was not valid JSON; returning raw text."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary using Ollama.");
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = ex.Message };
        }
    }

    private static string BuildPrompt(string title, string? author, string language)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a bibliographic assistant.");
        sb.AppendLine("Return ONLY valid JSON (no markdown, no code fences)." );
        sb.AppendLine("JSON schema: {\"title\":string,\"author\":string,\"category\":string,\"publishedDate\":string,\"pages\":number,\"coverImageUrl\":string,\"price\":number,\"summary\":string,\"confidence\":number,\"notes\":string}");
        sb.AppendLine("Rules:");
        sb.AppendLine("- summary: plain text, 600-1200 characters.");
        sb.AppendLine("- If you don't know a field, set it to null (except summary, which can be empty string).");
        sb.AppendLine("- Do not include HTML.");
        sb.AppendLine("- If unsure, lower confidence and mention uncertainty in notes.");
        sb.AppendLine();
        sb.AppendLine($"Language: {language}");
        sb.AppendLine($"Title: {title}");
        if (!string.IsNullOrWhiteSpace(author))
            sb.AppendLine($"Author: {author}");

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
