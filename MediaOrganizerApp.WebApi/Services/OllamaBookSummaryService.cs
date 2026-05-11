using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaBookSummaryService : OllamaBaseClient, IAiBookSummaryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<OllamaBookSummaryService> _logger;
    private readonly IProgressPublisher _progressPublisher;

    public OllamaBookSummaryService(
        HttpClient httpClient,
        ILogger<OllamaBookSummaryService> logger,
        IPromptTemplateProvider prompts,
        IProgressPublisher progressPublisher,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService)
        : base(httpClient, aiSettingsProvider, systemPromptService, prompts)
    {
        _logger = logger;
        _progressPublisher = progressPublisher;
    }

    public Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
        => GetBookSummaryAsync(title, author, language, null, cancellationToken);

    public async Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        await PublishAsync(connectionId, null, "preparing_prompt", 5, "Step 1/3: Preparing book summary prompt...", cancellationToken);
        var fullPrompt = await BuildFullPromptAsync("book-summary-full.prompt.txt", new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        }, cancellationToken);

        await PublishAsync(connectionId, null, "request_sent", 35, "Step 2/3: Sending prompt to AI model...", cancellationToken);
        var result = await ExecuteOllamaAsync(fullPrompt, connectionId, null, cancellationToken);
        await PublishAsync(connectionId, null, "parsing", 85, "Step 3/3: Processing AI response...", cancellationToken);
        return result;
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
    {
        var template = await Prompts.GetTemplateAsync("book-summary-only.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        return await ExecuteOllamaAsync(prompt, null, null, cancellationToken);
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        await PublishAsync(connectionId, correlationId, "preparing_prompt", 5, "Step 1/3: Preparing AI prompt...", cancellationToken);

        var template = await Prompts.GetTemplateAsync("book-summary-only.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        await PublishAsync(connectionId, correlationId, "request_sent", 35, "Step 2/3: Sending prompt to AI model...", cancellationToken);
        return await ExecuteOllamaAsync(prompt, connectionId, correlationId, cancellationToken);
    }

    private async Task<AiBookSummaryResponseDto> ExecuteOllamaAsync(
        string prompt,
        string? connectionId,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation("Progress for client {ConnectionId}: {Message}", connectionId, msg);

        try
        {
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                LogProgress("Sending prompt to AI model...");
                await PublishAsync(connectionId, correlationId, "request_sent", 45, "Sending prompt to AI model...", cancellationToken);
            }

            LogProgress("Waiting for AI response...");
            await PublishAsync(connectionId, correlationId, "waiting_response", 65, "Waiting for AI response...", cancellationToken);

            var text = await ExecuteGenerateAsync(prompt, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("AI - Empty response received from Ollama.");
                return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Empty LLM response." };
            }

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                var summaryValid = !string.IsNullOrWhiteSpace(parsed.Summary)
                    && parsed.Summary.IndexOf(parsed.Author ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                    && parsed.Summary.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

                var confidenceLow = parsed.Confidence < 0.5;
                parsed.Warnings = summaryValid && !confidenceLow
                    ? string.Empty
                    : $"AI Summary may not be accurate for '{parsed.Author}' - '{parsed.Title}'.";

                LogProgress("Done!");
                await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                    await PublishAsync(connectionId, correlationId, "warning", 100, parsed.Warnings, cancellationToken, "warning");
                return parsed;
            }

            var extractedJson = OllamaResponseHelper.TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);
                if (parsed is not null)
                {
                    var summaryValid = !string.IsNullOrWhiteSpace(parsed.Summary)
                        && parsed.Summary.IndexOf(parsed.Author ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                        && parsed.Summary.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

                    var confidenceLow = parsed.Confidence < 0.5;
                    parsed.Warnings = summaryValid && !confidenceLow
                        ? string.Empty
                        : $"AI summary may not be accurate for '{parsed.Author}' - '{parsed.Title}'.";

                    LogProgress("Done!");
                    await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                    return parsed;
                }

                LogProgress("Done!");
                await PublishAsync(connectionId, correlationId, "completed", 100, "Done!", cancellationToken);
                var summaryMatch = System.Text.RegularExpressions.Regex.Match(text, "\"notes\"\\s*:\\s*\"(.*?)\"");
                if (summaryMatch.Success)
                {
                    var summary = summaryMatch.Groups[1].Value;
                    return new AiBookSummaryResponseDto
                    {
                        Warnings = $"LLM output may not be accurate. Raw: {text.Trim()}",
                        Summary = string.Empty,
                        Confidence = 0,
                        Notes = summary
                    };
                }

                _logger.LogWarning("Could not recover notes from AI response: {Text}", text);
                return new AiBookSummaryResponseDto
                {
                    Notes = string.Empty,
                    Confidence = 0,
                    Warnings = "AI response could not be parsed. No notes available."
                };
            }

            return new AiBookSummaryResponseDto
            {
                Notes = string.Empty,
                Confidence = 0,
                Warnings = "AI response could not be parsed. No summary available."
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI - Book summary request was canceled by the caller (timeout/user cancel). Prompt={Prompt}", prompt);
            await PublishAsync(connectionId, correlationId, "cancelled", 100, "Request canceled.", cancellationToken, "warning");
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Request canceled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary using Ollama.");
            await PublishAsync(connectionId, correlationId, "failed", 100, ex.Message, cancellationToken, "error");
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = ex.Message };
        }
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
                Service = nameof(OllamaBookSummaryService),
                Operation = "book-summary"
            },
            connectionId,
            cancellationToken);

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
}
