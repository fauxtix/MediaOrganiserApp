using System.Globalization;
using System.Text.Json;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;

namespace MediaOrganizerApp.WebApi.Services.CloudChat;

/// <summary>
/// Gathers the OpenRouter usage state for the admin dashboard and maintains the
/// local daily counter of free-model requests.
/// </summary>
public sealed class CloudChatUsageService : ICloudChatUsageService
{
    private const string KeyUrl = "https://openrouter.ai/api/v1/key";
    private const string CreditsUrl = "https://openrouter.ai/api/v1/credits";
    private const string FreeUsagePrefix = "CloudChat:FreeUsage:";
    private const int FreeCapWithoutCredits = 50;
    private const int FreeCapWithCredits = 1000;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAIChatSettingsRepository _settingsRepository;
    private readonly ILogger<CloudChatUsageService> _logger;

    public CloudChatUsageService(
        IHttpClientFactory httpClientFactory,
        IAIChatSettingsRepository settingsRepository,
        ILogger<CloudChatUsageService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<CloudChatUsageDto> GetUsageAsync(CancellationToken ct = default)
    {
        var dto = new CloudChatUsageDto
        {
            TetoAutomatico = true,
            ResetUTC = NextUtcMidnight()
        };

        var apiKey = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, ct);
        dto.ChaveConfigurada = !string.IsNullOrWhiteSpace(apiKey);
        dto.UsadosGratisHoje = await GetFreeModelUsageTodayAsync(ct);

        if (dto.ChaveConfigurada)
        {
            var client = _httpClientFactory.CreateClient("OpenRouterCatalog");

            if (!await TryLoadKeyInfoAsync(client, apiKey!, dto, ct))
                dto.Erro = "Não foi possível consultar o estado da chave no OpenRouter.";

            await TryLoadCreditsAsync(client, apiKey!, dto, ct);
        }

        dto.TetoDiario = await ResolveDailyCapAsync(dto, ct);
        dto.PercentagemUsada = dto.TetoDiario > 0
            ? Math.Min(100, Math.Round(dto.UsadosGratisHoje * 100d / dto.TetoDiario, 1))
            : 0;

        dto.PodeUsarModelosPagos = dto.ChaveConfigurada
            && !dto.EhFreeTier
            && (dto.SaldoRestante.GetValueOrDefault() + dto.LimiteKeyRestante.GetValueOrDefault()) > 0;

        return dto;
    }

    public async Task RegisterFreeModelUsageAsync(CancellationToken ct = default)
    {
        try
        {
            var chave = TodayKey();
            var atual = await _settingsRepository.GetValueAsync(chave, null, ct);
            var novo = (int.TryParse(atual, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0) + 1;
            await _settingsRepository.SetValueAsync(chave, novo.ToString(CultureInfo.InvariantCulture), null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudChat] Falha a registar requisição gratuita.");
        }
    }

    public async Task SyncFreeUsageFromHeadersAsync(int cap, int remaining, CancellationToken ct = default)
    {
        try
        {
            var used = Math.Max(0, cap - remaining);
            await _settingsRepository.SetValueAsync(TodayKey(), used.ToString(CultureInfo.InvariantCulture), null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudChat] Falha a sincronizar contador de uso gratuito.");
        }
    }

    public async Task<int> GetFreeRequestsRemainingAsync(CancellationToken ct = default)
    {
        try
        {
            var used = await GetFreeModelUsageTodayAsync(ct);
            var cap = await ResolveDailyCapWithoutNetworkAsync(ct);
            return Math.Max(0, cap - used);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudChat] Falha a calcular pedidos gratuitos restantes; a assumir 0.");
            return 0;
        }
    }

    private async Task<bool> TryLoadKeyInfoAsync(
        HttpClient client, string apiKey, CloudChatUsageDto dto, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, KeyUrl);
            request.Headers.Authorization = new("Bearer", apiKey);
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("data", out var data))
                return false;

            dto.EhFreeTier = GetBool(data, "is_free_tier") ?? false;
            dto.LimiteKey = GetDouble(data, "limit");
            dto.LimiteKeyRestante = GetDouble(data, "limit_remaining");
            dto.Usados = GetDouble(data, "usage");
            dto.UsoHoje = GetDouble(data, "usage_daily");
            dto.UsoSemana = GetDouble(data, "usage_weekly");
            dto.UsoMes = GetDouble(data, "usage_monthly");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[CloudChat] Falha a consultar /key.");
            return false;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[CloudChat] Resposta /key inválida.");
            return false;
        }
    }

    private async Task TryLoadCreditsAsync(
        HttpClient client, string apiKey, CloudChatUsageDto dto, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, CreditsUrl);
            request.Headers.Authorization = new("Bearer", apiKey);
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("total_credits", out var credits))
                return;

            dto.TotalCredits = GetDoubleElement(credits);
            dto.SaldoRestante = dto.TotalCredits.GetValueOrDefault()
                - (GetDoubleElementOrDefault(doc.RootElement, "total_usage") ?? 0);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[CloudChat] Falha a consultar /credits.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[CloudChat] Resposta /credits inválida.");
        }
    }

    private async Task<int> ResolveDailyCapAsync(CloudChatUsageDto dto, CancellationToken ct)
    {
        var overrideRaw = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.FreeDailyCap, null, ct);
        if (int.TryParse(overrideRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var customCap) && customCap > 0)
        {
            dto.TetoAutomatico = false;
            return customCap;
        }

        var everPurchased = dto.TotalCredits.GetValueOrDefault() > 0
            || dto.Usados.GetValueOrDefault() > 0
            || dto.UsoHoje.GetValueOrDefault() > 0
            || dto.UsoSemana.GetValueOrDefault() > 0
            || dto.UsoMes.GetValueOrDefault() > 0;

        try
        {
            await _settingsRepository.SetValueAsync(
                CloudChatSettingsKeys.EverPurchasedCredits,
                everPurchased.ToString(CultureInfo.InvariantCulture),
                null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CloudChat] Falha a persistir snapshot de créditos comprados.");
        }

        return everPurchased ? FreeCapWithCredits : FreeCapWithoutCredits;
    }

    private async Task<int> ResolveDailyCapWithoutNetworkAsync(CancellationToken ct)
    {
        var overrideRaw = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.FreeDailyCap, null, ct);
        if (int.TryParse(overrideRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var customCap) && customCap > 0)
            return customCap;

        var snapshotRaw = await _settingsRepository.GetValueAsync(CloudChatSettingsKeys.EverPurchasedCredits, null, ct);
        var everPurchased = bool.TryParse(snapshotRaw, out var bought) && bought;
        return everPurchased ? FreeCapWithCredits : FreeCapWithoutCredits;
    }

    private async Task<int> GetFreeModelUsageTodayAsync(CancellationToken ct)
    {
        var raw = await _settingsRepository.GetValueAsync(TodayKey(), null, ct);
        return int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    private static string TodayKey()
        => FreeUsagePrefix + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    private static DateTime NextUtcMidnight()
    {
        var now = DateTime.UtcNow;
        return now.Date.AddDays(1);
    }

    private static bool? GetBool(JsonElement obj, string name)
    {
        if (obj.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.True)
            return true;
        if (obj.TryGetProperty(name, out prop) && prop.ValueKind == JsonValueKind.False)
            return false;
        return null;
    }

    private static double? GetDouble(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var prop) ? GetDoubleElement(prop) : null;

    private static double? GetDoubleElementOrDefault(JsonElement obj, string name)
        => obj.TryGetProperty(name, out var prop) ? GetDoubleElement(prop) : null;

    private static double? GetDoubleElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var value))
            return value;
        return null;
    }
}