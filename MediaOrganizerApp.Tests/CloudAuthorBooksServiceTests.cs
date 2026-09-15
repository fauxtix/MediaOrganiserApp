using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class CloudAuthorBooksServiceTests
{
    private const string ApiKey = "sk-test-123";

    private sealed class HttpCapture
    {
        public string? Body { get; set; }
        public List<string> RequestBodies { get; } = new();
        public HttpRequestHeaders? Headers { get; set; }
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ReturnsNull_WhenToggleOff()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.AuthorBooksUseCloud] = "false",
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetAuthorBooksAsync("José Saramago", "pt");

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>(),
            capture);

        var result = await service.GetAuthorBooksAsync("Eça de Queirós", "pt");

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ParsesBooks_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"O Livro\",\"year\":\"1998\",\"publisher\":\"Porto Editora\"},{\"title\":\"Segundo\",\"subtitle\":\"continuação\"}]"}}]}""",
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

        var result = await service.GetAuthorBooksAsync("José Saramago", "pt");

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("O Livro", result[0].Title);
        Assert.Equal("1998", result[0].PublishedDate);
        Assert.Equal("Segundo", result[1].Title);
        Assert.NotNull(capture.Body);
        Assert.Contains("\"openai/gpt-4o-mini\"", capture.Body);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_UsesExtractionModelsOverride_OverRecommendedDefault()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini",
                [CloudChatSettingsKeys.ExtractionModels] = "anthropic/claude-3.5-sonnet:free"
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "en");

        Assert.NotNull(result);
        Assert.Empty(result!);
        Assert.Contains("\"anthropic/claude-3.5-sonnet:free\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_AddsBearerToken_AndPlatformHeaders()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
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

        await service.GetAuthorBooksAsync("Graciliano Ramos", "pt");

        Assert.NotNull(capture.Headers);
        Assert.Equal(ApiKey, capture.Headers!.Authorization!.Parameter);
        Assert.True(capture.Headers.Contains("HTTP-Referer"));
        Assert.True(capture.Headers.Contains("X-Title"));
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ReturnsNull_OnHttpError()
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

        var result = await service.GetAuthorBooksAsync("Machado de Assis", "pt");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ReturnsNull_On429()
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
            usage);

        var result = await service.GetAuthorBooksAsync("Camões", "pt");

        Assert.Null(result);
        usage.Verify(x => x.SyncFreeUsageFromHeadersAsync(50, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_RegistersFreeUsage_WhenModelIsFree()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var usage = new Mock<ICloudChatUsageService>();
        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo",
                [CloudChatSettingsKeys.AllowPaidModels] = "true"
            },
            capture,
            usage);

        var result = await service.GetAuthorBooksAsync("Fernando Pessoa", "pt");

        Assert.NotNull(result);
        Assert.Contains("\"openrouter/free\"", capture.RequestBodies[0]);
        usage.Verify(x => x.RegisterFreeModelUsageAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_FallsBackToRecommendedModels_WhenFreeModelReturnsEmpty()
    {
        var empty = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var withBooks = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"O Livro\"}]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { empty, withBooks }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo",
                [CloudChatSettingsKeys.AllowPaidModels] = "true"
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Dion Fortune", "pt");

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("O Livro", result[0].Title);
        Assert.Contains("\"openrouter/free\"", capture.RequestBodies[0]);
Assert.Contains("\"openai/gpt-4o\"", capture.RequestBodies[1]);
        }

        [Fact]
        public async Task GetAuthorBooksAsync_UsesFreeVariants_WhenPaidModelsDisabled()
        {
            var empty = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                    Encoding.UTF8, "application/json")
            };
            var withBooks = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"The Quiet American\"}]"}}]}""",
                    Encoding.UTF8, "application/json")
            };

            var capture = new HttpCapture();
            var service = CreateService(
                responses: new Queue<HttpResponseMessage>(new[] { empty, withBooks }),
                settings: new Dictionary<string, string?>
                {
                    [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                    [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
                },
                capture);

            var result = await service.GetAuthorBooksAsync("Graham Greene", "en");

            Assert.NotNull(result);
            Assert.Single(result!);
            Assert.Equal("The Quiet American", result[0].Title);
            Assert.Contains("\"openrouter/free\"", capture.RequestBodies[0]);
            Assert.Contains("\"openai/gpt-4o:free\"", capture.RequestBodies[1]);
        }

    [Fact]
    public async Task GetAuthorBooksAsync_DoesNotRetryRecommended_WhenResolvedModelIsRecommendedPaid()
    {
        var empty = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { empty }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Autor Nenhum", "pt");

        Assert.NotNull(result);
        Assert.Empty(result!);
        Assert.Single(capture.RequestBodies);
    }

    private static CloudAuthorBooksService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        HttpCapture capture,
        Mock<ICloudChatUsageService>? usageMock = null)
    {
        var repo = new Mock<IAIChatSettingsRepository>();
        repo.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string? _, CancellationToken _) =>
                settings.TryGetValue(key, out var v) ? v : null);

        var prompts = new Mock<IPromptTemplateProvider>();
        prompts
            .Setup(x => x.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("For the author {{author}} return books. Language: {{language}}.");

        var httpClient = responses is null
            ? new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("Unexpected HTTP call.")))
            : new HttpClient(new StubHttpMessageHandler(request =>
            {
                capture.Headers = request.Headers;
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
        internet.Setup(x => x.IsReachableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        return new CloudAuthorBooksService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usageMock?.Object ?? Mock.Of<ICloudChatUsageService>(),
            internet.Object,
            Mock.Of<IProgressPublisher>(),
            Mock.Of<ILogger<CloudAuthorBooksService>>());
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}