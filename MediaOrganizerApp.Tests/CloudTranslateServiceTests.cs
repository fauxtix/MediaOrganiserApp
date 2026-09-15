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

public sealed class CloudTranslateServiceTests
{
    private const string ApiKey = "sk-test-123";

    private sealed class HttpCapture
    {
        public string? Body { get; set; }
    }

    private static TranslateRequestDto Request() => new()
    {
        SourceLanguage = "pt",
        TargetLanguage = "en",
        Text = "Olá, mundo!"
    };

    [Fact]
    public async Task TranslateAsync_ReturnsNull_WhenNoInternet()
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_ReturnsNull_WhenApiKeyMissing()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>(),
            capture);

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.Null(result);
        Assert.Null(capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_ParsesTranslatedText_FromMessageContent()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95}"}}]}""",
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Hello, world!", result!.TranslatedText);
        Assert.Equal(0.95, result.Confidence);
        Assert.NotNull(capture.Body);
        Assert.Contains("\"openai/gpt-4o-mini:free\"", capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_FallsBackToPlainText_WhenModelReturnsRawTranslation()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Hello, world!", result!.TranslatedText);
    }

    [Fact]
    public async Task TranslateAsync_ReturnsNull_OnHttpError()
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TranslateAsync_ReturnsNull_On429()
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.Null(result);
        usage.Verify(x => x.SyncFreeUsageFromHeadersAsync(50, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TranslateAsync_AddsBearerToken_AndPlatformHeaders()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
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

        await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(capture.Body);
        Assert.Contains("\"openrouter/free\"", capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_UsesFreeVariant_WhenPaidModelConfiguredAndToggleOff()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("\"openai/gpt-4o:free\"", capture.Body);
        Assert.DoesNotContain("\"openai/gpt-4o\"", capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_UsesPaidModel_WhenToggleOn()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
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

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("\"openai/gpt-4o\"", capture.Body);
    }

    [Fact]
    public async Task TranslateAsync_UsesDefaultFree_WhenPaidNonRecommendedModelConfiguredAndToggleOff()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { response }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
            },
            capture);

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("\"openrouter/free\"", capture.Body);
    }

    private static CloudTranslateService CreateService(
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
            .ReturnsAsync("Translate from {{sourceLanguage}} to {{targetLanguage}}: {{text}}");

        var httpClient = responses is null
            ? new HttpClient(new StubHttpMessageHandler(_ => throw new InvalidOperationException("Unexpected HTTP call.")))
            : new HttpClient(new StubHttpMessageHandler(request =>
            {
                if (request.Content is not null)
                    capture.Body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return responses.Count == 0
                    ? throw new InvalidOperationException("No canned responses left.")
                    : responses.Dequeue();
            }));

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var internet = new Mock<IInternetAvailabilityService>();
        internet.Setup(x => x.IsReachableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(internetReachable);

        return new CloudTranslateService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usage?.Object ?? Mock.Of<ICloudChatUsageService>(),
            internet.Object,
            Mock.Of<ILogger<CloudTranslateService>>());
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}