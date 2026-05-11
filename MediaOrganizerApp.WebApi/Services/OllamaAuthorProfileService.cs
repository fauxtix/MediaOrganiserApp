using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Local AI provider implementation using Ollama.
/// This is intended to make AI fallback testable without paid hosted APIs.
/// </summary>
public sealed class OllamaAuthorProfileService : OllamaBaseClient, IAiAuthorProfileService
{
    private readonly ILogger<OllamaAuthorProfileService> _logger;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaAuthorProfileService(
        HttpClient httpClient,
        IPromptTemplateProvider prompts,
        ILogger<OllamaAuthorProfileService> logger,
        IProgressPublisher progressPublisher,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService)
        : base(httpClient, aiSettingsProvider, systemPromptService, prompts)
    {
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(
        string name,
        string language,
        string? connectionId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Preparing author profile prompt...", cancellationToken);
        var fullPrompt = await BuildFullPromptAsync("author-profile.prompt.txt", new Dictionary<string, string?>
        {
            ["name"] = name,
            ["language"] = language
        }, cancellationToken);

        await PublishAsync(connectionId, correlationId, "request_sent", 40, "Sending author profile prompt to AI model...", cancellationToken);
        var text = await ExecuteGenerateAsync(fullPrompt, cancellationToken);
        await PublishAsync(connectionId, correlationId, "parsing", 75, "Parsing author profile response...", cancellationToken);

        var parsed = await AuthorProfileJsonRepairer.ParseOrRepairAsync(
            text,
            async repairPrompt =>
            {
                await PublishAsync(connectionId, correlationId, "repairing", 85, "Repairing malformed author profile JSON...", cancellationToken);
                return await ExecuteGenerateAsync(repairPrompt, cancellationToken);
            },
            _logger,
            language);

        await PublishAsync(connectionId, correlationId, "completed", 100, "Author profile completed.", cancellationToken);
        return parsed;
    }

    private Task PublishAsync(
        string? connectionId,
        string? correlationId,
        string stage,
        int percent,
        string message,
        CancellationToken cancellationToken,
        string level = "info")
        => _progressPublisher.PublishAsync(
            ProgressChannel.BooksInfo,
            new ProgressUpdateDto
            {
                CorrelationId = correlationId,
                Stage = stage,
                Percent = percent,
                Message = message,
                Level = level,
                Service = nameof(OllamaAuthorProfileService),
                Operation = "author-profile"
            },
            connectionId,
            cancellationToken);
}
