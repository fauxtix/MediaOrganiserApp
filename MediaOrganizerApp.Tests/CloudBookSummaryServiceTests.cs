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

public sealed class CloudBookSummaryServiceTests
{
    private const string ApiKey = "sk-test-123";
    private const string CatalogResponse =
        """{"data":[{"id":"meta-llama/llama-3.3-70b-instruct:free","name":"Meta Llama 3.3 70B Instruct (free)"}]}""";

    private static readonly string SummaryJson =
        """{"title":"Os Lusiadas","author":"Luis Vaz de Camoes","summary":"Epos sobre a viagem de Vasco da Gama.","confidence":0.9,"notes":null}""";

    private static readonly string FullMetadataJson =
        """{"title":"Os Lusiadas","author":"Luis Vaz de Camoes","category":"Poesia","publishedDate":"1572","pages":204,"summary":"Epos sobre a viagem de Vasco da Gama a India.","confidence":0.95,"notes":null}""";

    private sealed class HttpCapture
    {
        public string? Body { get; set; }
        public List<string> RequestBodies { get; } = new();
    }

    private static string WrapMessageContent(string json)
        => $"{{\"choices\":[{{\"message\":{{\"role\":\"assistant\",\"content\":\"{EscapeJson(json)}\"}}}}]}}";

    [Fact]
    public async Task GetBookSummaryOnlyAsync_ReturnsNull_WhenNoInternet()
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

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>(),
            capture);

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_ParsesSummary_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(SummaryJson),
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

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Epos sobre a viagem de Vasco da Gama.", result!.Summary);
        Assert.Equal("Os Lusiadas", result.Title);
        Assert.Equal("Luis Vaz de Camoes", result.Author);
        Assert.Contains("\"openai/gpt-4o-mini:free\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetBookSummaryAsync_ParsesFullMetadata_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(FullMetadataJson),
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

        var result = await service.GetBookSummaryAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Epos sobre a viagem de Vasco da Gama a India.", result!.Summary);
        Assert.Equal("Poesia", result.Category);
        Assert.Equal("1572", result.PublishedDate);
        Assert.Equal(204, result.Pages);
        Assert.Equal(0.95, result.Confidence);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_ReturnsNull_OnHttpError()
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
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture);

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_SweepsCatalogFreeModels_WhenFirstAnswerIsWeak()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":""}}]}""",
                Encoding.UTF8, "application/json")
        };
        var rich = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(SummaryJson),
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, rich }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture);

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Epos sobre a viagem de Vasco da Gama.", result!.Summary);
        Assert.Equal(2, capture.RequestBodies.Count);
        Assert.Contains("\"meta-llama/llama-3.3-70b-instruct:free\"", capture.RequestBodies[1]);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_ReturnsNull_WhenFreeDailyCapExhausted()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture,
            freeRequestsRemaining: 0);

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(capture.RequestBodies);
    }

    [Fact]
    public async Task GetBookSummaryOnlyAsync_UsesExtractionModelsOverride()
    {
        var rich = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                WrapMessageContent(SummaryJson),
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { rich }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini",
                [CloudChatSettingsKeys.ExtractionModels] = "anthropic/claude-3.5-sonnet:free"
            },
            capture);

        var result = await service.GetBookSummaryOnlyAsync("Os Lusiadas", "Luis Vaz de Camoes", "pt", null, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("\"anthropic/claude-3.5-sonnet:free\"", capture.RequestBodies[0]);
    }

    private static CloudBookSummaryService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        HttpCapture capture,
        bool internetReachable = true,
        Mock<ICloudChatUsageService>? usage = null,
        int freeRequestsRemaining = 1000)
    {
        var repo = new Mock<IAIChatSettingsRepository>();
        repo.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string? _, CancellationToken _) =>
                settings.TryGetValue(key, out var v) ? v : null);

        var prompts = new Mock<IPromptTemplateProvider>();
        prompts
            .Setup(x => x.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Summarize {{title}} by {{author}} in {{language}}. Return valid JSON only.");

        var httpClient = responses is null
            ? new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("Unexpected HTTP call.")))
            : new HttpClient(new StubHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsolutePath.EndsWith("/models", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(CatalogResponse, Encoding.UTF8, "application/json")
                    };
                }

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

        var usageMock = usage ?? new Mock<ICloudChatUsageService>();
        usageMock
            .Setup(x => x.GetFreeRequestsRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeRequestsRemaining);

        return new CloudBookSummaryService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usageMock.Object,
            internet.Object,
            Mock.Of<IProgressPublisher>(),
            new OpenRouterCatalogService(factory.Object, repo.Object, internet.Object, Mock.Of<ILogger<OpenRouterCatalogService>>()),
            Mock.Of<ILogger<CloudBookSummaryService>>());
    }

    private static string EscapeJson(string json) => json.Replace("\"", "\\\"", StringComparison.Ordinal);

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}