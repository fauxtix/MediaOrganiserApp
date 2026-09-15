using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class CloudMediaServiceTests
{
    private const string ApiKey = "sk-test-123";

    private static readonly string NotesJson =
        """[{"artist":"Fleetwood Mac","title":"Rumours","notes":"Notas grandiosas sobre o álbum.","confidence":0.9}]""";

    private static readonly string MetadataJson =
        """[{"artist":"Opeth","title":"Blackwater Park","year":"2001","notes":"Álbum seminal de death metal progressivo.","confidence":0.9}]""";

    private sealed class HttpCapture
    {
        public string? Body { get; set; }
        public List<string> RequestBodies { get; } = new();
    }

    private static string WrapMessageContent(string json)
        => $"{{\"choices\":[{{\"message\":{{\"role\":\"assistant\",\"content\":\"{EscapeJson(json)}\"}}}}]}}";

    [Fact]
    public async Task GetMediaNotesAsync_ReturnsNull_WhenNoInternet()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            internetReachable: false);

        var result = await service.GetMediaNotesAsync("Fleetwood Mac", "Rumours", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetMediaNotesAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>(),
            capture);

        var result = await service.GetMediaNotesAsync("Fleetwood Mac", "Rumours", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetMediaNotesAsync_ParsesNotes_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(NotesJson),
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture);

        var result = await service.GetMediaNotesAsync("Fleetwood Mac", "Rumours", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Notas grandiosas sobre o álbum.", result!.Notes);
        Assert.Equal("Fleetwood Mac", result.Artist);
        Assert.NotNull(capture.Body);
        Assert.Contains("\"openai/gpt-4o-mini\"", capture.Body);
    }

    [Fact]
    public async Task GetMediaNotesAsync_ReturnsNull_OnHttpError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("""{"error":"boom"}""", Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetMediaNotesAsync("Fleetwood Mac", "Rumours", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMediaMetadataAsync_ParsesMetadata_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(MetadataJson),
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture);

        var result = await service.GetMediaMetadataAsync("Opeth", "Blackwater Park", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("2001", result!.Year);
        Assert.Equal("Álbum seminal de death metal progressivo.", result.Notes);
    }

    [Fact]
    public async Task GetMediaEditionYearAsync_ReturnsYear_WhenModelFindsIt()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"The year is 2001."}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetMediaEditionYearAsync("Opeth", "Blackwater Park", "pt", null, null, CancellationToken.None);

        Assert.Equal("2001", result);
    }

    [Fact]
    public async Task GetMediaEditionYearAsync_ReturnsNA_WhenModelCannotFindIt()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"N/A"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetMediaEditionYearAsync("Autor Desconhecido", "Single Raro", "pt", null, null, CancellationToken.None);

        Assert.Equal("N/A", result);
    }

    [Fact]
    public async Task GetMediaEditionYearAsync_ReturnsNull_OnHttpError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("""{"error":"rate limited"}""", Encoding.UTF8, "application/json")
        };
        response.Headers.Add("X-RateLimit-Limit", "50");
        response.Headers.Add("X-RateLimit-Remaining", "0");

        var usage = new Mock<ICloudChatUsageService>();
        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
            },
            capture,
            usage: usage);

        var result = await service.GetMediaEditionYearAsync("Opeth", "Blackwater Park", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        usage.Verify(x => x.SyncFreeUsageFromHeadersAsync(50, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMediaEditionYearAsync_UsesFreeVariant_WhenPaidModelConfiguredAndToggleOff()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"The year is 2001."}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o"
            },
            capture);

        var result = await service.GetMediaEditionYearAsync("Opeth", "Blackwater Park", "pt", null, null, CancellationToken.None);

        Assert.Equal("2001", result);
        Assert.Contains("\"openai/gpt-4o:free\"", capture.RequestBodies[0]);
        Assert.DoesNotContain("\"openai/gpt-4o\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetMediaEditionYearAsync_UsesPaidModel_WhenToggleOn()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"The year is 2001."}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o",
                [CloudChatSettingsKeys.AllowPaidModels] = "true"
            },
            capture);

        var result = await service.GetMediaEditionYearAsync("Opeth", "Blackwater Park", "pt", null, null, CancellationToken.None);

        Assert.Equal("2001", result);
        Assert.Contains("\"openai/gpt-4o\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetMediaNotesAsync_UsesExtractionModelsOverride_OverRecommendedDefault()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var rich = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(NotesJson),
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, rich }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini",
                [CloudChatSettingsKeys.ExtractionModels] = "anthropic/claude-3.5-sonnet:free"
            },
            capture);

        var result = await service.GetMediaNotesAsync("Fleetwood Mac", "Rumours", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Notas grandiosas sobre o álbum.", result!.Notes);
        Assert.Contains("\"anthropic/claude-3.5-sonnet:free\"", capture.RequestBodies[0]);
        Assert.Contains("\"openai/gpt-4o:free\"", capture.RequestBodies[1]);
    }

    private static CloudMediaService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        HttpCapture capture,
        bool internetReachable = true,
        Mock<ICloudChatUsageService>? usage = null)
    {
        var repo = new Mock<IAIChatSettingsRepository>();
        repo.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string? _, CancellationToken _) =>
                settings.TryGetValue(key, out var v) ? v : null);

        var prompts = new Mock<IPromptTemplateProvider>();
        prompts
            .Setup(x => x.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Notes for {{title}} by {{artist}}. Language: {{language}}. {{additionalInfo}}");

        var httpClient = responses is null
            ? new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("Unexpected HTTP call.")))
            : new HttpClient(new StubHttpMessageHandler(request =>
            {
                if (request.Content is not null)
                {
                    capture.Body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    capture.RequestBodies.Add(capture.Body);
                }
                return responses.Count == 0
                    ? throw new InvalidOperationException("No canned responses left.")
                    : responses.Dequeue();
            }));

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var internet = new Mock<IInternetAvailabilityService>();
        internet.Setup(x => x.IsReachableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(internetReachable);

        return new CloudMediaService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usage?.Object ?? Mock.Of<ICloudChatUsageService>(),
            internet.Object,
            Mock.Of<IProgressPublisher>(),
            Mock.Of<ILogger<CloudMediaService>>());
    }

    private static string EscapeJson(string json) => json.Replace("\"", "\\\"", StringComparison.Ordinal);

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}