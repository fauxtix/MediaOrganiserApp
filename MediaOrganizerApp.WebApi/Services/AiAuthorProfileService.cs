using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
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
    private readonly IPromptTemplateProvider _prompts;

    public AiAuthorProfileService(IConfiguration config, IPromptTemplateProvider prompts)
    {
        _prompts = prompts;
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing configuration key: OpenAI:ApiKey");

        var model = config["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-4o-mini";

        _chat = new ChatClient(model, apiKey);
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, string language, CancellationToken cancellationToken = default)
    {
        var template = await _prompts.GetTemplateAsync("author-profile.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["name"] = name,
            ["language"] = language
        });

        var messages = new ChatMessage[]
        {
            new UserChatMessage(prompt),
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
