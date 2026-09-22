using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class CloudAuthorProfileServiceTests
{
    private const string ApiKey = "sk-test-123";
    private const string EmptyCatalogResponse = """{"data":[]}""";

    [Fact]
    public async Task GetAuthorProfileAsync_ReturnsNull_WhenNoInternet()
    {
        var capture = new Capture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            internetReachable: false);

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetAuthorProfileAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var capture = new Capture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>(),
            capture);

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetAuthorProfileAsync_ParsesBio_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"bio\":\"Escritor português.\",\"confidence\":0.9}"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new Capture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Escritor português.", result!.Bio);
        Assert.Equal(0.9, result.Confidence);
        Assert.NotNull(capture.Body);
        Assert.Contains("\"openrouter/free\"", capture.Body);
    }

    [Fact]
    public async Task GetAuthorProfileAsync_ReturnsNull_OnHttpError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("""{"error":"boom"}""", Encoding.UTF8, "application/json")
        };

        var capture = new Capture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAuthorProfileAsync_ReturnsNull_WhenFreeDailyCapExhausted()
    {
        var capture = new Capture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
            },
            capture,
            freeRequestsRemaining: 0);

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.Null(result);
        Assert.True(string.IsNullOrEmpty(capture.Body), "Nenhum pedido HTTP deve ser enviado quando o teto diário está esgotado.");
    }

    [Fact]
    public async Task GetAuthorProfileAsync_SweepsToCatalogModel_WhenFreeModelProducesNoBio()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"ainda a pensar"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var strong = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"bio\":\"Nobel da Literatura.\",\"confidence\":0.85}"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new Capture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, strong }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""");

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Nobel da Literatura.", result!.Bio);
        Assert.Equal(2, capture.RequestBodies.Count);
        Assert.Contains("\"openrouter/free\"", capture.RequestBodies[0]);
        Assert.Contains("\"z-ai/glm-5.2:free\"", capture.RequestBodies[1]);
    }

    [Fact]
    public async Task GetAuthorProfileAsync_ReturnsNull_WhenAllCandidatesLackBio()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"ainda a pensar"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var weak2 = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"sem dados"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new Capture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, weak2 }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""");

        var result = await service.GetAuthorProfileAsync("José Saramago", "pt", CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class Capture
    {
        public string? Body { get; set; }
        public List<string> RequestBodies { get; } = new();
    }

    private static CloudAuthorProfileService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        Capture capture,
        bool internetReachable = true,
        int freeRequestsRemaining = 1000,
        string catalogResponse = EmptyCatalogResponse)
    {
        var repo = new Mock<IAIChatSettingsRepository>();
        repo.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string? _, CancellationToken _) =>
                settings.TryGetValue(key, out var v) ? v : null);

        var prompts = new Mock<IPromptTemplateProvider>();
        prompts
            .Setup(x => x.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Profile for {{name}} in {{language}}.");

        var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/models", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(catalogResponse, Encoding.UTF8, "application/json")
                };
            }

            if (responses is null)
                throw new InvalidOperationException("Unexpected HTTP call.");

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

        var usage = new Mock<ICloudChatUsageService>();
        usage
            .Setup(x => x.GetFreeRequestsRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeRequestsRemaining);

        return new CloudAuthorProfileService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usage.Object,
            internet.Object,
            new OpenRouterCatalogService(factory.Object, repo.Object, internet.Object, Mock.Of<ILogger<OpenRouterCatalogService>>()),
            Mock.Of<ILogger<CloudAuthorProfileService>>());
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}