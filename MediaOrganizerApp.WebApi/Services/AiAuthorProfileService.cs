using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Server-side implementation that calls an LLM to generate an author bio/profile.
/// IMPORTANT: This is a fallback. External APIs should be attempted first.
/// </summary>
public sealed class AiAuthorProfileService : IAiAuthorProfileService
{
    private  ChatClient _chat;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ILogger<AiAuthorProfileService> _logger;
    private readonly IAppSettingsRepository _appSettingsRepository;


    public AiAuthorProfileService(IConfiguration config, IPromptTemplateProvider prompts, ILogger<AiAuthorProfileService> logger, IAppSettingsRepository appSettingsRepository)
    {
        _prompts = prompts;
        _logger = logger;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
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


        await CreateChatClientAsync();

        var result = await _chat.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var text = result.Value.Content[0].Text;

        // Use repairer to robustly parse or attempt to repair malformed JSON responses.
        var parsed = await AuthorProfileJsonRepairer.ParseOrRepairAsync(
            text,
            async (repairPrompt) =>
            {
                // Ask the same chat model to repair the malformed output. The repairPrompt is constructed by the helper.
                var repairMessages = new ChatMessage[] { new UserChatMessage(repairPrompt) };
                var repairResult = await _chat.CompleteChatAsync(repairMessages, cancellationToken: cancellationToken);
                return repairResult.Value.Content[0].Text;
            },
            _logger,
            language);

        return parsed;
    }

    private async Task CreateChatClientAsync()
    {
        var settings =  await _appSettingsRepository.GetSettingsAsync();
        var baseUrl = settings.ModelBaseUrl;
        var model = settings.AiModelName;
        _chat = new ChatClient(model, baseUrl);
    }
}
