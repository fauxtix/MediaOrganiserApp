using System.Net;
using System.Text;
using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests;

public sealed class OpenRouterCatalogServiceTests
{
    [Fact]
    public async Task GetFreeModelsAsync_FiltersPaidModels_WhenAllowPaidDisabled()
    {
        var service = CreateService(allowPaidModelsSetting: "false");

        var result = await service.GetFreeModelsAsync();

        var ids = result.Select(m => m.Id).ToList();
        Assert.Contains("google/gemma-4-31b-it:free", ids);
        Assert.Contains(CloudChatSettingsKeys.DefaultModelValue, ids);
        Assert.DoesNotContain("anthropic/claude-sonnet-4.5", ids);
    }

    [Fact]
    public async Task GetFreeModelsAsync_FiltersPaidModels_WhenSettingMissing()
    {
        var service = CreateService(allowPaidModelsSetting: null);

        var result = await service.GetFreeModelsAsync();

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain("anthropic/claude-sonnet-4.5", result.Select(m => m.Id));
    }

    [Fact]
    public async Task GetFreeModelsAsync_ReturnsAllModels_WhenAllowPaidEnabled()
    {
        var service = CreateService(allowPaidModelsSetting: "true");

        var result = await service.GetFreeModelsAsync();

        var ids = result.Select(m => m.Id).ToList();
        Assert.Equal(3, result.Count);
        Assert.Contains("google/gemma-4-31b-it:free", ids);
        Assert.Contains(CloudChatSettingsKeys.DefaultModelValue, ids);
        Assert.Contains("anthropic/claude-sonnet-4.5", ids);
    }

    [Fact]
    public async Task GetFreeModelsAsync_MarksFreeFlag()
    {
        var service = CreateService(allowPaidModelsSetting: "true");

        var result = await service.GetFreeModelsAsync();

        var freeModel = result.Single(m => m.Id == "google/gemma-4-31b-it:free");
        Assert.True(freeModel.EhGratuito);

        var router = result.Single(m => m.Id == CloudChatSettingsKeys.DefaultModelValue);
        Assert.True(router.EhGratuito);
        Assert.True(router.EhRouter);

        var paid = result.Single(m => m.Id == "anthropic/claude-sonnet-4.5");
        Assert.False(paid.EhGratuito);
    }

    [Fact]
    public async Task GetFreeModelsAsync_ToleratesMissingProfileAndContext()
    {
        var service = CreateService(allowPaidModelsSetting: null, minimalModel: true);

        var result = await service.GetFreeModelsAsync();

        var model = Assert.Single(result);
        Assert.Equal("google/gemma-4-31b-it:free", model.Id);
        Assert.Equal("Google: Gemma 4 31B IT (free)", model.Nome);
        Assert.Equal(32_768, model.ContextLength);
        Assert.Empty(model.Descricao);
        Assert.Null(model.IntelligenceIndex);
        Assert.Null(model.ReleaseDate);
    }

    private static OpenRouterCatalogService CreateService(
        string? allowPaidModelsSetting,
        bool minimalModel = false)
    {
        var settings = new Mock<IAIChatSettingsRepository>();
        settings
            .Setup(x => x.GetValueAsync(
                CloudChatSettingsKeys.AllowPaidModels, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowPaidModelsSetting);

        var json = minimalModel
            ? /*lang=json*/ """
            {
              "data": [
                { "id": "google/gemma-4-31b-it:free", "name": "Google: Gemma 4 31B IT (free)" }
              ]
            }
            """
            : /*lang=json*/ """
            {
              "data": [
                {
                  "id": "google/gemma-4-31b-it:free",
                  "name": "Google: Gemma 4 31B IT (free)",
                  "description": "Modelo gratuito",
                  "context_length": 131072,
                  "release_date": "2026-06-01T00:00:00Z",
                  "profile": { "intelligence": 58.5, "coding": 72, "agentic": 30 }
                },
                {
                  "id": "openrouter/free",
                  "name": "OpenRouter: Free Router",
                  "context_length": null
                },
                {
                  "id": "anthropic/claude-sonnet-4.5",
                  "name": "Anthropic: Claude Sonnet 4.5",
                  "context_length": 200000
                }
              ]
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var httpClient = new HttpClient(new StubHttpMessageHandler(_ => response));
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient("OpenRouterCatalog")).Returns(httpClient);

        var internet = new Mock<IInternetAvailabilityService>();
        internet.Setup(x => x.IsReachableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        return new OpenRouterCatalogService(
            factory.Object,
            settings.Object,
            internet.Object,
            Mock.Of<ILogger<OpenRouterCatalogService>>());
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}