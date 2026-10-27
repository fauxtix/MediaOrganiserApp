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
    private const string EmptyCatalogResponse = """{"data":[]}""";

    private sealed class HttpCapture
    {
        public string? Body { get; set; }
        public List<string> RequestBodies { get; } = new();
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
    public async Task TranslateAsync_ReturnsNull_WhenFreeDailyCapExhausted()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: null,
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
            },
            capture,
            freeRequestsRemaining: 0);

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.Null(result);
        Assert.True(string.IsNullOrEmpty(capture.Body), "Nenhum pedido HTTP deve ser enviado quando o teto diário está esgotado.");
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

    [Fact]
    public async Task TranslateAsync_SweepsToCatalogModel_WhenFreeModelReturnsWeakResult()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var strong = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95}"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, strong }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""");

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(0.95, result!.Confidence);
        Assert.Equal(2, capture.RequestBodies.Count);
        Assert.Contains("\"openrouter/free\"", capture.RequestBodies[0]);
        Assert.Contains("\"z-ai/glm-5.2:free\"", capture.RequestBodies[1]);
    }

    [Fact]
    public async Task TranslateAsync_StampsProviderAndModelServedByResponseBody()
    {
        // The router answers with the upstream model, which is never the requested id.
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"model":"z-ai/glm-5.2","choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95}"}}]}""",
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
        Assert.Equal("openrouter", result!.Provider);
        Assert.Equal("z-ai/glm-5.2", result.Model);
    }

    [Fact]
    public async Task TranslateAsync_FallsBackToRequestedModel_WhenResponseBodyOmitsIt()
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
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("openrouter", result!.Provider);
        Assert.Equal(CloudChatSettingsKeys.DefaultModelValue, result.Model);
    }

    [Fact]
    public async Task TranslateAsync_OverwritesProviderAndModelEmittedByTheModel()
    {
        // The response DTO is deserialized straight from the model's own JSON, so a rogue
        // "provider"/"model" in the model output must never be trusted.
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95,\"provider\":\"ollama\",\"model\":\"fake/model\"}"}}]}""",
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
        Assert.Equal("openrouter", result!.Provider);
        Assert.Equal(CloudChatSettingsKeys.DefaultModelValue, result.Model);
    }

    [Fact]
    public async Task TranslateAsync_PublishesTheModelInFlight_ForEveryCandidate()
    {
        var weak = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"Hello, world!"}}]}""",
                Encoding.UTF8, "application/json")
        };
        var strong = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95}"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var progress = new RecordingProgressPublisher();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { weak, strong }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""",
            progress: progress);

        await service.TranslateAsync(Request(), CancellationToken.None);

        var tries = progress.Updates.Where(u => u.Stage == "cloud_try").ToList();
        Assert.Equal(2, tries.Count);
        Assert.Equal(CloudChatSettingsKeys.DefaultModelValue, tries[0].Model);
        Assert.Equal("z-ai/glm-5.2:free", tries[1].Model);
        Assert.All(tries, u => Assert.Equal("openrouter", u.Provider));
        // A cancelled request never returns a body, so the progress channel is the only
        // way for the UI to report which model was being attempted.
        Assert.Equal(CloudChatSettingsKeys.DefaultModelValue, tries[0].Message);

        Assert.Contains(progress.Updates, u => u.Stage == "cloud_empty" && u.Model == CloudChatSettingsKeys.DefaultModelValue);
        Assert.Contains(progress.Updates, u => u.Stage == "cloud_success" && u.Model == "z-ai/glm-5.2:free");
    }

    [Fact]
    public async Task TranslateAsync_PublishesCloudFailed_WhenCandidateFails()
    {
        var failed = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent("""{"error":"rate limited"}""", Encoding.UTF8, "application/json")
        };
        var strong = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"{\"translatedText\":\"Hello, world!\",\"confidence\":0.95}"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var progress = new RecordingProgressPublisher();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { failed, strong }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""",
            progress: progress);

        var result = await service.TranslateAsync(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("z-ai/glm-5.2:free", result!.Model);
        Assert.Contains(progress.Updates, u => u.Stage == "cloud_failed" && u.Model == CloudChatSettingsKeys.DefaultModelValue);
    }

    private static CloudTranslateService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        HttpCapture capture,
        bool internetReachable = true,
        Mock<ICloudChatUsageService>? usage = null,
        int freeRequestsRemaining = 1000,
        string catalogResponse = EmptyCatalogResponse,
        RecordingProgressPublisher? progress = null)
    {
        var repo = new Mock<IAIChatSettingsRepository>();
        repo.Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, string? _, CancellationToken _) =>
                settings.TryGetValue(key, out var v) ? v : null);

        var prompts = new Mock<IPromptTemplateProvider>();
        prompts
            .Setup(x => x.GetTemplateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Translate from {{sourceLanguage}} to {{targetLanguage}}: {{text}}");

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

        var usageMock = usage ?? new Mock<ICloudChatUsageService>();
        usageMock
            .Setup(x => x.GetFreeRequestsRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeRequestsRemaining);

        return new CloudTranslateService(
            factory.Object,
            repo.Object,
            prompts.Object,
            usageMock.Object,
            internet.Object,
            new OpenRouterCatalogService(factory.Object, repo.Object, internet.Object, Mock.Of<ILogger<OpenRouterCatalogService>>()),
            progress ?? new RecordingProgressPublisher(),
            Mock.Of<ILogger<CloudTranslateService>>());
    }

    private sealed class RecordingProgressPublisher : IProgressPublisher
    {
        public List<ProgressUpdateDto> Updates { get; } = new();

        public Task PublishAsync(
            ProgressChannel channel,
            ProgressUpdateDto update,
            string? connectionId,
            CancellationToken cancellationToken = default)
        {
            Updates.Add(update);
            return Task.CompletedTask;
        }

        public Task PublishChunkAsync(
            ProgressChannel channel,
            string chunk,
            string? connectionId,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishDoneAsync(
            ProgressChannel channel,
            string? connectionId,
            string? correlationId = null,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}