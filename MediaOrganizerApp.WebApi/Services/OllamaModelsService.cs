using System.Text.Json;
using System.Text.Json.Serialization;
using MediaOrganizerApp.Application.DTOs;

namespace MediaOrganizerApp.WebApi.Services;

/// <summary>
/// Ollama-backed implementation of <see cref="IOllamaModelsService"/>.
/// Uses the shared Ollama HttpClient plus the dynamically-resolved base URL from
/// <see cref="IAiSettingsProvider"/> so the configured model server is honoured.
/// </summary>
public sealed class OllamaModelsService : IOllamaModelsService
{
    private readonly HttpClient _httpClient;
    private readonly IAiSettingsProvider _aiSettingsProvider;
    private readonly ILogger<OllamaModelsService> _logger;

    public OllamaModelsService(
        HttpClient httpClient,
        IAiSettingsProvider aiSettingsProvider,
        ILogger<OllamaModelsService> logger)
    {
        _httpClient = httpClient;
        _aiSettingsProvider = aiSettingsProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<AIModelVM>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var (baseUrl, _) = await _aiSettingsProvider.GetAiSettingsAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(baseUrl), "/api/tags"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var payload = JsonSerializer.Deserialize<OllamaTagsResponse>(json, JsonOptions);

        if (payload?.Models is null || payload.Models.Count == 0)
            return Enumerable.Empty<AIModelVM>();

        return payload.Models
            .Select((m, i) => new AIModelVM { Id = i + 1, Descricao = m.Name ?? m.Model ?? string.Empty })
            .Where(m => !string.IsNullOrWhiteSpace(m.Descricao))
            .ToList();
    }

    public async Task<bool> PullModelAsync(string name, CancellationToken cancellationToken = default)
    {
        var (baseUrl, _) = await _aiSettingsProvider.GetAiSettingsAsync();
        var payload = new { name, stream = false };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(baseUrl), "/api/pull"))
        {
            Content = JsonContent.Create(payload)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ollama pull failed for {Model}: {Status} - {Error}", name, response.StatusCode, error);
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteModelAsync(string name, CancellationToken cancellationToken = default)
    {
        var (baseUrl, _) = await _aiSettingsProvider.GetAiSettingsAsync();
        var payload = new { name };

        using var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(new Uri(baseUrl), "/api/delete"))
        {
            Content = JsonContent.Create(payload)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Ollama delete failed for {Model}: {Status} - {Error}", name, response.StatusCode, error);
            return false;
        }

        return true;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class OllamaTagsResponse
    {
        public List<OllamaModelInfo>? Models { get; set; }
    }

    private sealed class OllamaModelInfo
    {
        public string? Name { get; set; }
        public string? Model { get; set; }
    }
}
