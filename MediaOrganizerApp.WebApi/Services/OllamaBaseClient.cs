using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Abstract base class for Ollama-backed AI services.
/// Centralises AI settings retrieval (via <see cref="IAiSettingsProvider"/>),
/// system-prompt + task-prompt composition (via <see cref="ISystemPromptService"/>
/// and <see cref="IPromptTemplateProvider"/>), and /api/generate HTTP dispatch.
/// </summary>
public abstract class OllamaBaseClient
{
    protected readonly HttpClient HttpClient;
    protected readonly IAiSettingsProvider AiSettingsProvider;
    protected readonly ISystemPromptService SystemPromptService;
    protected readonly IPromptTemplateProvider Prompts;

    protected OllamaBaseClient(
        HttpClient httpClient,
        IAiSettingsProvider aiSettingsProvider,
        ISystemPromptService systemPromptService,
        IPromptTemplateProvider prompts)
    {
        HttpClient = httpClient;
        AiSettingsProvider = aiSettingsProvider;
        SystemPromptService = systemPromptService;
        Prompts = prompts;
    }

    /// <summary>
    /// Retrieves AI settings (base URL and model name) through the cache-backed provider.
    /// </summary>
    protected Task<(string ModelBaseUrl, string ModelName)> GetAiSettingsAsync()
        => AiSettingsProvider.GetAiSettingsAsync();

    /// <summary>
    /// Builds a combined prompt by prepending the rendered system prompt to a rendered task template.
    /// </summary>
    protected async Task<string> BuildFullPromptAsync(
        string taskTemplateName,
        IReadOnlyDictionary<string, string?> tokens,
        CancellationToken cancellationToken)
    {
        var systemPrompt = await SystemPromptService.GetSystemPrompt(cancellationToken);
        var template = await Prompts.GetTemplateAsync(taskTemplateName, cancellationToken);
        var taskPrompt = PromptTemplateRenderer.Render(template, tokens);
        return $"{systemPrompt}\n\n{taskPrompt}";
    }

    /// <summary>
    /// Sends a prompt to Ollama /api/generate and returns the extracted response text,
    /// or <c>null</c> if the response was empty.
    /// </summary>
    /// <param name="prompt">The full prompt text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="timeoutSeconds">Per-request timeout in seconds (default 80).</param>
    /// <param name="temperature">Sampling temperature (default 0 for deterministic output).</param>
    /// <param name="topP">Top-p sampling (default 1).</param>
    /// <param name="numPredict">Optional max tokens to generate.</param>
    protected async Task<string?> ExecuteGenerateAsync(
        string prompt,
        CancellationToken cancellationToken,
        int timeoutSeconds = 80,
        double temperature = 0,
        double topP = 1,
        int? numPredict = null)
    {
        var (baseUrl, model) = await GetAiSettingsAsync();

        // Build options as a dictionary so we can conditionally include num_predict
        // without anonymous-type boxing issues.
        var options = new Dictionary<string, object>
        {
            ["temperature"] = temperature,
            ["top_p"] = topP
        };
        if (numPredict.HasValue)
            options["num_predict"] = numPredict.Value;

        var payload = new Dictionary<string, object>
        {
            ["model"] = model,
            ["prompt"] = prompt,
            ["stream"] = false,
            ["options"] = options
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
        {
            Content = JsonContent.Create(payload)
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        using var response = await HttpClient.SendAsync(request, timeoutCts.Token);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(timeoutCts.Token);
        var json = Encoding.UTF8.GetString(bytes);

        return OllamaResponseHelper.TryExtractResponseText(json);
    }
}
