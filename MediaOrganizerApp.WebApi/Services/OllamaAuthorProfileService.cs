using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Local AI provider implementation using Ollama.
/// This is intended to make AI fallback testable without paid hosted APIs.
/// </summary>
public sealed class OllamaAuthorProfileService : IAiAuthorProfileService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;

    public OllamaAuthorProfileService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = config["Ai:Ollama:Model"] ?? "llama3.2:1b";
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, CancellationToken cancellationToken = default)
    {
        var systemPrompt =
            """
            You generate short author profiles.

            Requirements:
            - If you are not confident about facts, say so in Notes and lower Confidence.
            - Do NOT invent exact dates, awards, or long bibliographies.
            - If the name is ambiguous (multiple candidates), mention ambiguity in Notes.
            - If you find more than one plausible candidate, do NOT attempt to combine them into a single profile; instead, indicate the ambiguity in Notes and lower Confidence.
            - Output MUST be valid JSON matching:
              { "bio": string|null, "confidence": number|null, "notes": string|null }
            """;

        var userPrompt =
            $"""
            Author name: {name}

            Write a concise bio (2-6 sentences) in Portuguese (pt-PT).
            """;

        // Ollama /api/generate expects a single prompt string.
        var prompt = $"{systemPrompt}\n\n{userPrompt}";

        var payload = new OllamaGenerateRequest
        {
            Model = _model,
            Prompt = prompt,
            Stream = false
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/api/generate")
        {
            Content = JsonContent.Create(payload)
        };

        using var resp = await _httpClient.SendAsync(req, cancellationToken);

        resp.EnsureSuccessStatusCode();

        // Ollama may return either a single JSON object or NDJSON (one JSON object per line).
        // Reading bytes + decoding defensively avoids DecoderFallbackException when the response contains
        // unexpected encodings.
        var rawBytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);
        var raw = DecodeUtf8Lossy(rawBytes);
        var text = TryExtractOllamaResponseText(raw);

        // Same robustness behavior as OpenAI implementation.
        try
        {
            var parsed = JsonSerializer.Deserialize<AiAuthorProfileResponseDto>(
                text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return parsed ?? new AiAuthorProfileResponseDto
            {
                Bio = text,
                Confidence = 0.2,
                Notes = "Non-JSON response; treated as Bio."
            };
        }
        catch
        {
            return new AiAuthorProfileResponseDto
            {
                Bio = text,
                Confidence = 0.2,
                Notes = "Model response was not valid JSON."
            };
        }
    }

    private sealed class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; }
    }

 private static string TryExtractOllamaResponseText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Fast path: single JSON object.
        try
        {
            var single = JsonSerializer.Deserialize<OllamaGenerateResponse>(
                raw,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrWhiteSpace(single?.Response))
                return single.Response!;
        }
        catch
        {
            // Ignore and attempt NDJSON parsing.
        }

        // NDJSON path: parse each line as JSON and concatenate the response fields.
        var sb = new StringBuilder();
        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var item = JsonSerializer.Deserialize<OllamaGenerateResponse>(
                    line,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrWhiteSpace(item?.Response))
                    sb.Append(item.Response);
            }
            catch
            {
                // Ignore non-JSON lines.
            }
        }

        return sb.ToString();
    }

    private static string DecodeUtf8Lossy(byte[] bytes)
        => Encoding.UTF8.GetString(bytes);

    private sealed class OllamaGenerateResponse
    {
        public string? Response { get; set; }
    }
}
