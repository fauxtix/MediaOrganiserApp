using System.Text.Json;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// Proxies the OpenRouter /models endpoint. By default it exposes only the
/// free models (suffixed ":free" plus the "openrouter/free" router); when the
/// admin setting <see cref="CloudChatSettingsKeys.AllowPaidModels"/> is enabled,
/// the whole catalog (paid models included) is returned.
/// </summary>
public sealed class OpenRouterCatalogService
{
    private const string ModelsUrl = "https://openrouter.ai/api/v1/models";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly IInternetAvailabilityService _internet;
    private readonly ILogger<OpenRouterCatalogService> _logger;

    public OpenRouterCatalogService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        IInternetAvailabilityService internet,
        ILogger<OpenRouterCatalogService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _internet = internet;
        _logger = logger;
    }

    public async Task<List<OpenRouterCatalogModel>> GetFreeModelsAsync(CancellationToken ct = default)
    {
        if (!await _internet.IsReachableAsync(ct))
        {
            _logger.LogDebug("[OpenRouter] Sem acesso à internet; catálogo indisponível.");
            return new List<OpenRouterCatalogModel>();
        }

        var allowPaidModels = bool.TryParse(
            await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.AllowPaidModels, null, ct),
            out var allowPaid) && allowPaid;

        var client = _httpClientFactory.CreateClient("OpenRouterCatalog");
        try
        {
            var json = await client.GetStringAsync(ModelsUrl, ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data))
                return new List<OpenRouterCatalogModel>();

            var result = new List<OpenRouterCatalogModel>();
            foreach (var item in data.EnumerateArray())
            {
                var id = GetString(item, "id");
                if (string.IsNullOrWhiteSpace(id) || !IsAllowed(id, allowPaidModels))
                    continue;

                if (!item.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String)
                    continue;

                var contextLength = GetInt(item, "context_length") ?? 32_768;
                var profile = item.TryGetProperty("profile", out var profileElement) ? profileElement : default;

                result.Add(new OpenRouterCatalogModel
                {
                    Id = id,
                    Nome = nameProp.GetString() ?? id,
                    Descricao = GetString(item, "description") ?? string.Empty,
                    ContextLength = contextLength,
                    EhRouter = string.Equals(id, CloudChatSettingsKeys.DefaultModelValue, StringComparison.OrdinalIgnoreCase),
                    EhGratuito = CloudChatSettingsKeys.IsFreeModel(id),
                    ReleaseDate = TryParseDate(GetString(item, "release_date")),
                    IntelligenceIndex = TryGetDouble(profile, "intelligence"),
                    CodingIndex = TryGetDouble(profile, "coding"),
                    AgenticIndex = TryGetDouble(profile, "agentic")
                });
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[OpenRouter] Falha ao obter catálogo de modelos.");
            return new List<OpenRouterCatalogModel>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[OpenRouter] Resposta do catálogo inválida.");
            return new List<OpenRouterCatalogModel>();
        }
    }

    private static bool IsAllowed(string id, bool allowPaidModels)
        => allowPaidModels || CloudChatSettingsKeys.IsFreeModel(id);

    private static string? GetString(JsonElement obj, string name)
    {
        if (obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString();
        return null;
    }

    private static int? GetInt(JsonElement obj, string name)
    {
        if (obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetInt32(out var value))
                return value;
        }
        return null;
    }

    private static double? TryGetDouble(JsonElement obj, string name)
    {
        if (obj.ValueKind != JsonValueKind.Object) return null;
        if (obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetDouble(out var value))
                return value;
        }
        return null;
    }

    private static DateTime? TryParseDate(string? value)
    {
        if (DateTimeOffset.TryParse(value, out var parsed))
            return parsed.UtcDateTime;
        return null;
    }
}