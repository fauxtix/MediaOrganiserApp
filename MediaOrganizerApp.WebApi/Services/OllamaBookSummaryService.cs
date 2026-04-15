using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class OllamaBookSummaryService : IAiBookSummaryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaBookSummaryService> _logger;
    private string _baseUrl;
    private string _model;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IPromptTemplateProvider _prompts;
    private readonly IHubContext<GoogleBooksProgressHub> _hubContext;

    public OllamaBookSummaryService(HttpClient httpClient,
                                    ILogger<OllamaBookSummaryService> logger,
                                    IPromptTemplateProvider prompts,
                                    IHubContext<GoogleBooksProgressHub> hubContext,
                                    IAppSettingsRepository appSettingsRepository)
    {
        _httpClient = httpClient;
        _logger = logger;
        _prompts = prompts;
        _hubContext = hubContext;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
    {
        return await GetBookSummaryAsync(title, author, language, null, cancellationToken);
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryAsync(
        string title,
        string? author,
        string language,
        string? connectionId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 1/3: Preparing book summary prompt...", cancellationToken);

        var template = await _prompts.GetTemplateAsync("book-summary-full.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 2/3: Sending prompt to AI model...", cancellationToken);

        var result = await ExecuteOllamaAsync(prompt, connectionId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 3/3: Processing AI response...", cancellationToken);

        return result;
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
        string title,
        string? author,
        string language,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync("book-summary-only.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        // Ensure cancellationToken is passed correctly
        return await ExecuteOllamaAsync(prompt, null, cancellationToken);
    }

    public async Task<AiBookSummaryResponseDto> GetBookSummaryOnlyAsync(
            string title,
            string? author,
            string language,
            string? connectionId,
             CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 1/3: Preparing AI prompt...", cancellationToken);

        var template = await _prompts.GetTemplateAsync("book-summary-only.prompt.txt", cancellationToken);
        var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
        {
            ["language"] = language,
            ["title"] = title,
            ["author"] = string.IsNullOrWhiteSpace(author) ? "(none)" : author
        });

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Step 2/3: Sending prompt to AI model...", cancellationToken);


        // Ensure cancellationToken is passed correctly
        return await ExecuteOllamaAsync(prompt, connectionId, cancellationToken);
    }


    private async Task<AiBookSummaryResponseDto> ExecuteOllamaAsync(string prompt, string? connectionId, CancellationToken cancellationToken)
    {
        void LogProgress(string msg) => _logger.LogInformation($"Progress for client {connectionId}: {msg}");

        var (modelBaseUrl, modelName) = await GetAiSettings();
        _baseUrl = modelBaseUrl;
        _model = modelName;

        var payload = new
        {
            model = _model,
            prompt,
            stream = false
        };

        try
        {

            if (_hubContext != null && !string.IsNullOrWhiteSpace(prompt) && !string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Sending prompt to AI model...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Sending prompt to AI model...");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/generate"))
            {
                Content = JsonContent.Create(payload)
            };

            // Progress: waiting for AI response
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                LogProgress("Waiting for AI response...");
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Waiting for AI response...");
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI - Book summary request failed. Status={StatusCode}. Body={Body}", (int)response.StatusCode, errorBody);
                return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Failed to generate book summary." };
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var json = Encoding.UTF8.GetString(bytes);

            var text = TryExtractOllamaResponseText(json, _logger);
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("AI - Empty response received from Ollama.");
                return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Empty LLM response." };
            }

            var parsed = TryParseJsonContract(text);
            if (parsed is not null)
            {
                bool summaryValid = !string.IsNullOrWhiteSpace(parsed.Summary)
                        && parsed.Summary.IndexOf(parsed.Author ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                        && parsed.Summary.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

                bool confidenceLow = parsed.Confidence < 0.5;
                if (summaryValid && !confidenceLow)
                {
                    parsed.Warnings = string.Empty;
                }
                else
                {
                    parsed.Warnings = $"AI Summary may not be accurate for '{parsed.Author}' - '{parsed.Title}'.";
                }
                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    LogProgress("Done!");
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    if (!string.IsNullOrWhiteSpace(parsed.Warnings))
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, parsed.Warnings);
                }
                return parsed;
            }

            var extractedJson = TryExtractFirstJsonObject(text);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                parsed = TryParseJsonContract(extractedJson);

                if (parsed is not null)
                {
                    // Validate summary: must mention author and title, and confidence should be reasonable
                    bool summaryValid = !string.IsNullOrWhiteSpace(parsed.Summary)
                        && parsed.Summary.IndexOf(parsed.Author ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0
                        && parsed.Summary.IndexOf(parsed.Title ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;

                    bool confidenceLow = parsed.Confidence < 0.5;

                    if (summaryValid && !confidenceLow)
                    {
                        parsed.Warnings = string.Empty;
                    }
                    else
                    {
                        parsed.Warnings = $"AI summary may not be accurate for '{parsed.Author}' - '{parsed.Title}'.";
                    }
                    if (!string.IsNullOrWhiteSpace(connectionId))
                    {
                        LogProgress("Done!");
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                    }
                    return parsed;
                }

                if (!string.IsNullOrWhiteSpace(connectionId))
                {
                    LogProgress("Done!");
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", connectionId, "Done!");
                }
                string summary = "";
                var summaryMatch = System.Text.RegularExpressions.Regex.Match(text, "\"notes\"\\s*:\\s*\"(.*?)\"");

                if (summaryMatch.Success)
                {
                    summary = summaryMatch.Groups[1].Value;
                    return new AiBookSummaryResponseDto
                    {
                        Warnings = $"LLM output may not be accurate for '{parsed.Author}' - '{parsed.Title}'. Raw: {text.Trim()}",
                        Summary = string.Empty,
                        Confidence = 0,
                        Notes = summary
                    };
                }
                else
                {
                    _logger.LogWarning($"Could not recover notes from AI response: {text}");
                    return new AiBookSummaryResponseDto
                    {
                        Notes = string.Empty,
                        Confidence = 0,
                        Warnings = "AI response could not be parsed. No notes available."
                    };
                }
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
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = "Request canceled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI - Error generating book summary using Ollama.");
            return new AiBookSummaryResponseDto { Summary = string.Empty, Confidence = 0, Notes = ex.Message };
        }
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

    private static string? TryExtractOllamaResponseText(string json, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var response))
            {
                var responseText = response.GetString();

                // Check if the response itself is a stringified JSON object
                if (!string.IsNullOrWhiteSpace(responseText))
                {
                    try
                    {
                        using var innerDoc = JsonDocument.Parse(responseText);
                        return innerDoc.RootElement.ToString();
                    }
                    catch (JsonException ex)
                    {
                        // Log the malformed JSON for debugging
                        logger.LogWarning(ex, "Malformed stringified JSON in response: {ResponseText}", responseText);
                        return responseText; // Return raw text if parsing fails
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            // Log the malformed JSON for debugging
            logger.LogError(ex, "Malformed JSON encountered: {Json}", json);
            return null;
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
            catch (JsonException ex)
            {
                // Log each malformed line for debugging
                logger.LogWarning(ex, "Malformed NDJSON line encountered: {Line}", line);
            }
        }

        var merged = sb.ToString();
        return string.IsNullOrWhiteSpace(merged) ? null : merged;
    }
    private async Task<(string modelBaseUrl, string modelName)> GetAiSettings()
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        return (settings.ModelBaseUrl, settings.AiModelName);
    }

}
