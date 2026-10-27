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
    private const string CatalogResponse =
        """{"data":[{"id":"z-ai/glm-5.2:free","name":"Z.ai GLM 5.2 (free)"},{"id":"google/gemma-4-31b-it:free","name":"Google Gemma 4 31B IT (free)"},{"id":"nvidia/nemotron-3.5-lightning:free","name":"NVIDIA Nemotron 3.5 Lightning (free)"},{"id":"meta-llama/llama-3.3-70b-instruct:free","name":"Meta Llama 3.3 70B Instruct (free)"}]}""";

    private const string CatalogResponseWithSafetyModel =
        """{"data":[{"id":"nvidia/nemotron-3.5-content-safety:free","name":"NVIDIA Nemotron 3.5 Content Safety (free)"},{"id":"google/gemma-4-31b-it:free","name":"Google Gemma 4 31B IT (free)"}]}""";

    private const string UpstreamRateLimitBody =
        """{"error":{"message":"Provider returned error","code":429,"metadata":{"raw":"qwen/qwen3.8-27b:free is temporarily rate-limited upstream. Please retry shortly.","provider_name":"ModelRun","limit_source":"upstream_provider_shared_pool"}}}""";

    /// <summary>Catalog with a single free model, so the sweep has exactly one candidate.</summary>
    private const string CatalogResponseSingleModel =
        """{"data":[{"id":"openai/gpt-4o-mini:free","name":"OpenAI GPT-4o Mini (free)"}]}""";

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
                """{"choices":[{"message":{"role":"assistant","content":"[{\"id\":\"google-1\",\"title\":\"O Livro\",\"subtitle\":\"Primeira parte\",\"year\":\"1998\",\"publisher\":\"Porto Editora\",\"confidence\":0.9,\"synthesized\":false},{\"title\":\"Segundo\",\"confidence\":0.5}]"}}]}""",
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
        Assert.Equal("google-1", result[0].Id);
        Assert.Equal("O Livro", result[0].Title);
        Assert.Equal("Primeira parte", result[0].Subtitle);
        Assert.Equal("1998", result[0].PublishedDate);
        Assert.Equal("Porto Editora", result[0].Publisher);
        Assert.Equal(0.9, result[0].Confidence);
        Assert.False(result[0].Synthesized);
        Assert.Equal("Segundo", result[1].Title);
        Assert.Equal(0.5, result[1].Confidence);
        Assert.NotNull(capture.Body);
        Assert.Contains("\"openai/gpt-4o-mini:free\"", capture.Body);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_StripsFakeThumbnailUrls()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"Com Capa Real\",\"thumbnail\":\"https://books.google.com/books/content?id=abc\"},{\"title\":\"Capa Falsa\",\"thumbnail\":\"https://example.com/cover.jpg\"}]"}}]}""",
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

        var result = await service.GetAuthorBooksAsync("Camilla Läckberg", "pt");

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("https://books.google.com/books/content?id=abc", result[0].Thumbnail);
        Assert.Null(result[1].Thumbnail);
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
    public async Task GetAuthorBooksAsync_ReturnsNull_WhenFreeDailyCapExhausted()
    {
        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-3.5-turbo"
            },
            capture,
            freeRequestsRemaining: 0);

        var result = await service.GetAuthorBooksAsync("Camões", "pt");

        Assert.Null(result);
        Assert.Empty(capture.RequestBodies);
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
        public async Task GetAuthorBooksAsync_UsesLiveCatalogFreeModels_WhenPaidModelsDisabled()
        {
            var withBooks = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"The Quiet American\"}]"}}]}""",
                    Encoding.UTF8, "application/json")
            };

            var capture = new HttpCapture();
            var service = CreateService(
                responses: new Queue<HttpResponseMessage>(new[]
                {
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                            Encoding.UTF8, "application/json")
                    },
                    withBooks,
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                            Encoding.UTF8, "application/json")
                    }
                }),
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
            Assert.Contains("\"z-ai/glm-5.2:free\"", capture.RequestBodies[0]);
            Assert.Contains("\"google/gemma-4-31b-it:free\"", capture.RequestBodies[1]);
        }

        [Fact]
        public async Task GetAuthorBooksAsync_PreferredModelReturnsIncomplete_StillSweepsKnownGood_RatherThanRouter()
        {
            var empty = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[]"}}]}""",
                    Encoding.UTF8, "application/json")
            };
            var glmWeak = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"Só Um\"}]"}}]}""",
                    Encoding.UTF8, "application/json")
            };
            var gemmaRich = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"Um\"},{\"title\":\"Dois\"},{\"title\":\"Três\"},{\"title\":\"Quatro\"},{\"title\":\"Cinco\"}]"}}]}""",
                    Encoding.UTF8, "application/json")
            };

            var capture = new HttpCapture();
            var service = CreateService(
                responses: new Queue<HttpResponseMessage>(new[] { glmWeak, gemmaRich, empty }),
                settings: new Dictionary<string, string?>
                {
                    [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
                },
                capture);

            var result = await service.GetAuthorBooksAsync("Rui Tavares", "pt");

            Assert.NotNull(result);
            Assert.Equal(5, result!.Count);
            Assert.Equal("Quatro", result[3].Title);
            Assert.Contains("\"z-ai/glm-5.2:free\"", capture.RequestBodies[0]);
            Assert.Contains("\"google/gemma-4-31b-it:free\"", capture.RequestBodies[1]);
            Assert.DoesNotContain("\"openrouter/free\"", capture.RequestBodies);
        }

    [Fact]
    public async Task GetAuthorBooksAsync_RunsPaidRecommendedModelAlone_WhenToggleOn()
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
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini",
                [CloudChatSettingsKeys.AllowPaidModels] = "true"
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Autor Nenhum", "pt");

        Assert.NotNull(result);
        Assert.Empty(result!);
        Assert.Single(capture.RequestBodies);
        Assert.Contains("\"openai/gpt-4o-mini\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_UsesFreeVariant_WhenPaidRecommendedModelConfiguredAndToggleOff()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"O Livro\"},{\"title\":\"Segundo\"}]"}}]}""",
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

        var result = await service.GetAuthorBooksAsync("José Saramago", "pt");

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Contains("\"openai/gpt-4o:free\"", capture.RequestBodies[0]);
        Assert.DoesNotContain("\"openai/gpt-4o\"", capture.RequestBodies[0]);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ReadsAnswerFromReasoningField_WhenContentIsEmpty()
    {
        var reasoning = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":null,"reasoning":"[{\"title\":\"O Raciocinado\"}]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { reasoning }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.AllowPaidModels] = "true"
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Autor Raciocinante", "pt");

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("O Raciocinado", result[0].Title);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_SkipsSafetyClassifierAnswers_AndKeepsSweeping()
    {
        // A guard/free-router model answering with a content-safety classification
        // ("User Safety: safe;") is a model artifact, not a bibliography.
        var safetyOnly = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"User Safety: safe;"}}]}""",
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
            responses: new Queue<HttpResponseMessage>(new[] { safetyOnly, withBooks }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "pt");

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("O Livro", result[0].Title);
        // The safety artifact did not end the sweep: a second model was tried.
        Assert.True(capture.RequestBodies.Count >= 2);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_StripsSafetyLinesBeforeParsingJson()
    {
        var safetyThenBooks = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"User Safety: safe;\n[{\"title\":\"O Livro\"}]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { safetyThenBooks }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "pt");

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("O Livro", result[0].Title);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_RetriesSameModel_OnceOnUpstreamRateLimit()
    {
        var upstream429 = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(UpstreamRateLimitBody, Encoding.UTF8, "application/json")
        };
        var withBooks = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"O Livro\"},{\"title\":\"Segundo\"}]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { upstream429, withBooks }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture,
            upstreamRetryDelay: TimeSpan.Zero);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "pt");

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        // Both requests went to the SAME model: the retry happened before moving on.
        Assert.Equal(2, capture.RequestBodies.Count);
        Assert.Contains("\"openai/gpt-4o-mini:free\"", capture.RequestBodies[0]);
        Assert.Contains("\"openai/gpt-4o-mini:free\"", capture.RequestBodies[1]);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_DoesNotRetry_AccountLevelRateLimit()
    {
        var account429 = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(
                """{"error":{"message":"Rate limit exceeded","code":429,"metadata":{"limit_source":"key"}}}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { account429, account429 }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey,
                [CloudChatSettingsKeys.DefaultModel] = "openai/gpt-4o-mini"
            },
            capture,
            catalogResponse: CatalogResponseSingleModel,
            upstreamRetryDelay: TimeSpan.Zero);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "pt");

        Assert.Null(result);
        Assert.Single(capture.RequestBodies);
    }

    [Fact]
    public async Task GetAuthorBooksAsync_ExcludesSafetyClassifierModels_FromLiveCatalog()
    {
        var withBooks = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"choices":[{"message":{"role":"assistant","content":"[{\"title\":\"O Livro\"}]"}}]}""",
                Encoding.UTF8, "application/json")
        };

        var capture = new HttpCapture();
        var service = CreateService(
            responses: new Queue<HttpResponseMessage>(new[] { withBooks }),
            settings: new Dictionary<string, string?>
            {
                [CloudChatSettingsKeys.OpenRouterApiKey] = ApiKey
            },
            capture,
            catalogResponse: CatalogResponseWithSafetyModel);

        var result = await service.GetAuthorBooksAsync("Agatha Christie", "pt");

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.DoesNotContain("content-safety", string.Join("\n", capture.RequestBodies));
        Assert.Contains("google/gemma-4-31b-it:free", capture.RequestBodies[0]);
    }

    private static CloudAuthorBooksService CreateService(
        Queue<HttpResponseMessage>? responses,
        Dictionary<string, string?> settings,
        HttpCapture capture,
        Mock<ICloudChatUsageService>? usageMock = null,
        int freeRequestsRemaining = 1000,
        string catalogResponse = CatalogResponse,
        TimeSpan? upstreamRetryDelay = null)
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
                if (request.RequestUri?.AbsolutePath.EndsWith("/models", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(catalogResponse, Encoding.UTF8, "application/json")
                    };
                }

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

        var effectiveUsage = usageMock ?? new Mock<ICloudChatUsageService>();
        effectiveUsage
            .Setup(x => x.GetFreeRequestsRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeRequestsRemaining);

        return new CloudAuthorBooksService(
            factory.Object,
            repo.Object,
            prompts.Object,
            effectiveUsage.Object,
            internet.Object,
            Mock.Of<IProgressPublisher>(),
            new OpenRouterCatalogService(factory.Object, repo.Object, internet.Object, Mock.Of<ILogger<OpenRouterCatalogService>>()),
            Mock.Of<ILogger<CloudAuthorBooksService>>(),
            upstreamRetryDelay);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}