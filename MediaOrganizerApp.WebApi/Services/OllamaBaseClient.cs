using MediaOrganizerApp.Application.Interfaces.Services.AI;
using System.Text;
using System.Text.Json;

namespace MediaOrganizerApp.WebApi.Services;

public abstract class OllamaBaseClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSettingsProvider _aiSettingsProvider;
    private readonly IPromptTemplateProvider _prompts;
    private readonly ISystemPromptService _systemPromptService;

    protected OllamaBaseClient(
        HttpClient httpClient,
        IAiSettingsProvider aiSettingsProvider,
        IPromptTemplateProvider prompts,
        ISystemPromptService systemPromptService)
    {
        _httpClient = httpClient;
        _aiSettingsProvider = aiSettingsProvider;
        _prompts = prompts;
        _systemPromptService = systemPromptService;
    }

    protected HttpClient HttpClient => _httpClient;
    protected IPromptTemplateProvider Prompts => _prompts;

    protected Task<(string BaseUrl, string Model)> GetAiSettingsAsync(CancellationToken cancellationToken = default)
        => _aiSettingsProvider.GetSettingsAsync(cancellationToken);

    protected async Task<string> BuildPromptAsync(
        string templateName,
        IReadOnlyDictionary<string, string?> tokens,
        CancellationToken cancellationToken)
    {
        var systemPrompt = await _systemPromptService.GetSystemPrompt(cancellationToken);
        var template = await _prompts.GetTemplateAsync(templateName, cancellationToken);
        var rendered = PromptTemplateRenderer.Render(template, tokens);

        return $"{systemPrompt}\n\n{rendered}";
    }

    protected async Task<string> BuildPromptWithoutSystemAsync(
        string templateName,
        IReadOnlyDictionary<string, string?> tokens,
        CancellationToken cancellationToken)
    {
        var template = await _prompts.GetTemplateAsync(templateName, cancellationToken);
        return PromptTemplateRenderer.Render(template, tokens);
    }

    protected async Task<string?> GenerateTextAsync(
        string prompt,
        object? options,
        CancellationToken cancellationToken)
    {
        var (baseUrl, model) = await GetAiSettingsAsync(cancellationToken);

        var payload = new
        {
            model,
            prompt,
            stream = false,
            options
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/generate"))
        {
            Content = JsonContent.Create(payload)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var json = Encoding.UTF8.GetString(bytes);

        return OllamaResponseHelper.TryExtractResponseText(json);
    }

    protected async Task<string?> ChatTextAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        var (baseUrl, model) = await GetAiSettingsAsync(cancellationToken);

        var payload = new
        {
            model,
            stream = false,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/chat"))
        {
            Content = JsonContent.Create(payload)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(raw);
        if (doc.RootElement.TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var content))
        {
            return content.GetString();
        }

        return null;
    }
}