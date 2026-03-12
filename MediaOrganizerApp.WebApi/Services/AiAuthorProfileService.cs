using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Server-side implementation that calls an LLM to generate an author bio/profile.
/// IMPORTANT: This is a fallback. External APIs should be attempted first.
/// </summary>
public sealed class AiAuthorProfileService : IAiAuthorProfileService
{
    private readonly ChatClient _chat;

    public AiAuthorProfileService(IConfiguration config)
    {
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing configuration key: OpenAI:ApiKey");

        var model = config["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-4o-mini";

        _chat = new ChatClient(model, apiKey);
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, CancellationToken cancellationToken = default)
    {
        // Defensive prompt because external metadata APIs already failed.
        // We want useful output but we do NOT want invented facts.
        var system =
            """
            You generate short author profiles.

            Requirements:
            - If you are not confident about facts, say so in Notes and lower Confidence.
            - Do NOT invent exact dates, awards, or long bibliographies.
            - If the name is ambiguous (multiple candidates), mention ambiguity in Notes.
            - Output MUST be valid JSON matching:
              { "bio": string|null, "photoUrl": string|null, "confidence": number|null, "notes": string|null }
            """;

        var user =
            $"""
            Author name: {name}

            Write a concise bio (2-6 sentences) in Portuguese (pt-PT).
            """;

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(system),
            new UserChatMessage(user),
        };

        var result = await _chat.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var text = result.Value.Content[0].Text;

        // Robustness: if the model doesn't return JSON, still use the text as Bio.
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
}
