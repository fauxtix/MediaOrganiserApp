using System.Globalization;
using System.Net;
using System.Text;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests;

public sealed class CloudChatUsageServiceTests
{
    private const string KeyUrl = "https://openrouter.ai/api/v1/key";
    private const string CreditsUrl = "https://openrouter.ai/api/v1/credits";

    [Fact]
    public async Task GetUsageAsync_NoKeyConfigured_ReturnsDefaultsAndCap50()
    {
        var (service, _, _) = CreateService(new Dictionary<string, string?>
            {
                [TodayKey()] = "5"
            });

        var dto = await service.GetUsageAsync();

        Assert.False(dto.ChaveConfigurada);
        Assert.False(dto.PodeUsarModelosPagos);
        Assert.Equal(5, dto.UsadosGratisHoje);
        Assert.Equal(50, dto.TetoDiario);
        Assert.True(dto.TetoAutomatico);
        Assert.Equal(10, dto.PercentagemUsada);
    }

    [Fact]
    public async Task GetUsageAsync_FreeTierWithoutCredits_CannotUsePaidAndCap50()
    {
        var (service, _, _) = CreateService(
            new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = "sk-test"
            },
            handler: request => request.RequestUri!.AbsoluteUri == KeyUrl
                ? Json(KeyPayload(true))
                : Json(CreditsPayload(totalCredits: 0, totalUsage: 0)));

        var dto = await service.GetUsageAsync();

        Assert.True(dto.ChaveConfigurada);
        Assert.True(dto.EhFreeTier);
        Assert.False(dto.PodeUsarModelosPagos);
        Assert.Equal(0, dto.SaldoRestante);
        Assert.Equal(50, dto.TetoDiario);
        Assert.True(dto.TetoAutomatico);
        Assert.Null(dto.Erro);
    }

    [Fact]
    public async Task GetUsageAsync_PaidAccountWithCredits_CanUsePaidAndCap1000()
    {
        var (service, _, _) = CreateService(
            new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = "sk-test"
            },
            handler: request => request.RequestUri!.AbsoluteUri == KeyUrl
                ? Json(KeyPayload(false, limit: 100, limitRemaining: 93, usage: 7,
                    usageDaily: 2, usageWeekly: 5, usageMonthly: 7))
                : Json(CreditsPayload(totalCredits: 25, totalUsage: 7)));

        var dto = await service.GetUsageAsync();

        Assert.True(dto.ChaveConfigurada);
        Assert.False(dto.EhFreeTier);
        Assert.True(dto.PodeUsarModelosPagos);
        Assert.Equal(18, dto.SaldoRestante);
        Assert.Equal(2, dto.UsoHoje);
        Assert.Equal(5, dto.UsoSemana);
        Assert.Equal(7, dto.UsoMes);
        Assert.Equal(100, dto.LimiteKey);
        Assert.Equal(93, dto.LimiteKeyRestante);
        Assert.Equal(1000, dto.TetoDiario);
        Assert.True(dto.TetoAutomatico);
    }

    [Fact]
    public async Task GetUsageAsync_OverrideCap_DisablesAutomaticAndChangesPercent()
    {
        var (service, _, _) = CreateService(
            new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = "sk-test",
                [CloudChatSettingsKeys.FreeDailyCap] = "80",
                [TodayKey()] = "60"
            },
            handler: request => request.RequestUri!.AbsoluteUri == KeyUrl
                ? Json(KeyPayload(true))
                : Json(CreditsPayload(totalCredits: 0, totalUsage: 0)));

        var dto = await service.GetUsageAsync();

        Assert.Equal(80, dto.TetoDiario);
        Assert.False(dto.TetoAutomatico);
        Assert.Equal(75, dto.PercentagemUsada);
    }

    [Fact]
    public async Task RegisterFreeModelUsageAsync_IncrementsTodaysCounter()
    {
        var (service, _, writes) = CreateService(new Dictionary<string, string?>
        {
            [TodayKey()] = "7"
        });

        await service.RegisterFreeModelUsageAsync();

        Assert.Single(writes);
        Assert.Contains((TodayKey(), "8"), writes);
    }

    [Fact]
    public async Task RegisterFreeModelUsageAsync_StartsAtOne_WhenNoCounter()
    {
        var (service, _, writes) = CreateService(new Dictionary<string, string?>());

        await service.RegisterFreeModelUsageAsync();

        Assert.Contains((TodayKey(), "1"), writes);
    }

    [Fact]
    public async Task SyncFreeUsageFromHeadersAsync_SetsCounterFromCapMinusRemaining()
    {
        var (service, _, writes) = CreateService(new Dictionary<string, string?>(),
            _ => throw new InvalidOperationException("no HTTP expected"));

        await service.SyncFreeUsageFromHeadersAsync(cap: 50, remaining: 30);

        Assert.Single(writes);
        Assert.Contains((TodayKey(), "20"), writes);
    }

    [Fact]
    public async Task SyncFreeUsageFromHeadersAsync_ClampsAtZero()
    {
        var (service, _, writes) = CreateService(new Dictionary<string, string?>(),
            _ => throw new InvalidOperationException("no HTTP expected"));

        await service.SyncFreeUsageFromHeadersAsync(cap: 10, remaining: 40);

        Assert.Contains((TodayKey(), "0"), writes);
    }

    private static (CloudChatUsageService Service, Mock<IAIChatSettingsRepository> Settings, List<(string Key, string Value)> Writes) CreateService(
        Dictionary<string, string?> settings,
        Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
    {
        var settingsMock = new Mock<IAIChatSettingsRepository>();
        settingsMock
            .Setup(x => x.GetValueAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string userId, CancellationToken ct)
                => settings.TryGetValue(key, out var value) ? value : null);

        var writes = new List<(string Key, string Value)>();
        settingsMock
            .Setup(x => x.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .Callback((string key, string value, string userId, CancellationToken ct) => writes.Add((key, value)))
            .Returns(Task.CompletedTask);

        var httpClient = new HttpClient(new StubHttpMessageHandler(request => handler!(request)));
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient("OpenRouterCatalog")).Returns(httpClient);

        return (
            new CloudChatUsageService(
                factory.Object,
                settingsMock.Object,
                Mock.Of<ILogger<CloudChatUsageService>>()),
            settingsMock,
            writes);
    }

    private static string TodayKey()
        => "CloudChat:FreeUsage:" + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

    private static HttpResponseMessage Json(string body)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string KeyPayload(
        bool isFreeTier,
        double? limit = null,
        double? limitRemaining = null,
        double usage = 0,
        double usageDaily = 0,
        double usageWeekly = 0,
        double usageMonthly = 0)
        => $$"""
        {
          "data": {
            "is_free_tier": {{(isFreeTier ? "true" : "false")}},
            "limit": {{(limit is null ? "null" : limit.Value.ToString(CultureInfo.InvariantCulture))}},
            "limit_remaining": {{(limitRemaining is null ? "null" : limitRemaining.Value.ToString(CultureInfo.InvariantCulture))}},
            "usage": {{usage.ToString(CultureInfo.InvariantCulture)}},
            "usage_daily": {{usageDaily.ToString(CultureInfo.InvariantCulture)}},
            "usage_weekly": {{usageWeekly.ToString(CultureInfo.InvariantCulture)}},
            "usage_monthly": {{usageMonthly.ToString(CultureInfo.InvariantCulture)}}
          }
        }
        """;

    private static string CreditsPayload(double totalCredits, double totalUsage)
        => $$"""
        {
          "total_credits": {{totalCredits.ToString(CultureInfo.InvariantCulture)}},
          "total_usage": {{totalUsage.ToString(CultureInfo.InvariantCulture)}}
        }
        """;

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}