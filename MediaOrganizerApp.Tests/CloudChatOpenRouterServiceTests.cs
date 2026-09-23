using MediaOrganizerApp.Application.Configurations;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Exceptions;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class CloudChatHelperTests
{
    [Fact]
    public void ParseOpenRouterChunk_ExtractsContentAndModel()
    {
        var chunk = ChatChunkParser.ParseOpenRouterChunk(
            """{"choices":[{"delta":{"content":"Olá"}}],"model":"nvidia/foo"}""");

        Assert.NotNull(chunk);
        Assert.Equal("Olá", chunk!.Text);
        Assert.Equal("nvidia/foo", chunk.Model);
        Assert.Null(chunk.Reasoning);
    }

    [Fact]
    public void ParseOpenRouterChunk_ExtractsReasoning()
    {
        var chunk = ChatChunkParser.ParseOpenRouterChunk(
            """{"choices":[{"delta":{"reasoning_content":"pensar"}}]}""");

        Assert.NotNull(chunk);
        Assert.Equal("pensar", chunk!.Reasoning);
        Assert.Equal(string.Empty, chunk.Text);
    }

    [Fact]
    public void ParseOpenRouterChunk_ExtractsUsage()
    {
        var chunk = ChatChunkParser.ParseOpenRouterChunk(
            """{"choices":[{"delta":{}}],"usage":{"prompt_tokens":10,"completion_tokens":3,"total_tokens":13}}""");

        Assert.NotNull(chunk);
        Assert.Equal(10, chunk!.PromptTokens);
        Assert.Equal(3, chunk.CompletionTokens);
        Assert.Equal(13, chunk.TotalTokens);
    }

    [Fact]
    public void ParseOpenRouterChunk_ReturnsNull_ForJunkLine()
    {
        Assert.Null(ChatChunkParser.ParseOpenRouterChunk("not json"));
        Assert.Null(ChatChunkParser.ParseOpenRouterChunk("{}"));
    }

    [Fact]
    public void EstimateTokens_RoughlyThreeCharsPerToken()
    {
        Assert.Equal(4, CloudResponseMetrics.EstimateTokens("abcdefghijkl"));
        Assert.Equal(1, CloudResponseMetrics.EstimateTokens("a"));
        Assert.Equal(0, CloudResponseMetrics.EstimateTokens(null));
    }

    [Fact]
    public void SmartFallbackDescription_TruncatesLongFirstLine()
    {
        var description = CloudResponseMetrics.SmartFallbackDescription(new string('x', 200));

        Assert.StartsWith("xxx", description);
        Assert.EndsWith("...", description);
        Assert.True(description.Length <= 84);
    }

    [Fact]
    public void ComputeTokensPerSecond_GenerationWindowBased()
    {
        // 200 tokens de saída em janela de geração de 8 s (10 s total, 2 s TTFT)
        Assert.Equal(25.0, CloudResponseMetrics.ComputeTokensPerSecond(200, 10.0, 2.0));
        Assert.Equal(12.5, CloudResponseMetrics.ComputeTokensPerSecond(100, 10.0, 2.0));

        // sem tokens de saída -> sem taxa
        Assert.Null(CloudResponseMetrics.ComputeTokensPerSecond(0, 10.0, 2.0));

        // streams sub-segundo (cache/burst) -> sem taxa
        Assert.Null(CloudResponseMetrics.ComputeTokensPerSecond(100, 0.5, 0.2));

        // janela de geração nula/negativa (TTFT >= total) -> sem taxa
        Assert.Null(CloudResponseMetrics.ComputeTokensPerSecond(50, 1.2, 1.5));
        Assert.Null(CloudResponseMetrics.ComputeTokensPerSecond(50, 1.0, 1.0));
    }
}

public sealed class OpenRouterChatServiceTests
{
    private const string ApiKey = "sk-test-123";
    private const string EmptyCatalogResponse = """{"data":[]}""";

    [Fact]
    public async Task StreamChatAsync_StreamsTextChunks()
    {
        var responses = SseQueue(
            """{"choices":[{"delta":{"content":"Olá"}}],"model":"nvidia/foo"}""",
            """{"choices":[{"delta":{"content":" mundo"}}]}""",
            """{"choices":[{"delta":{}}],"usage":{"prompt_tokens":10,"completion_tokens":3,"total_tokens":13}}""");

        var service = CreateService(responses, out _);

        var chunks = new List<CloudChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(CreatePayload("nvidia/foo"), "user-1", "pt"))
            chunks.Add(chunk);

        Assert.Equal("Olá mundo", string.Concat(chunks.Select(c => c.Text)));
        Assert.True(chunks.Any(c => c.PromptTokens == 10 && c.CompletionTokens == 3));
    }

    [Fact]
    public async Task StreamChatAsync_Throws_WhenApiKeyMissing()
    {
        var service = CreateService(SseQueue("ignored"), out _, apiKey: null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in service.StreamChatAsync(CreatePayload("nvidia/foo"), null, "pt")) { }
        });

        Assert.Contains("OpenRouter", exception.Message);
    }

    [Fact]
    public async Task StreamChatAsync_Throws_WhenFreeDailyCapExhausted()
    {
        var service = CreateService(SseQueue("ignored"), out _, freeRequestsRemaining: 0);

        var exception = await Assert.ThrowsAsync<CloudProviderTransientException>(async () =>
        {
            await foreach (var _ in service.StreamChatAsync(CreatePayload("google/gemma-4-31b-it:free"), null, "pt")) { }
        });

        Assert.Contains("modelos gratuitos", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StreamChatAsync_FallsBackToOpenRouterFree_On404()
    {
        var responses = new Queue<HttpResponseMessage>(
            new[]
            {
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{\"error\":\"not found\"}", Encoding.UTF8, "application/json")
                },
                SseResponse(
                    """{"choices":[{"delta":{"content":"ok"}}],"model":"openrouter/free"}""")
            });

        var service = CreateService(responses, out var requestBodies);
        var payload = CreatePayload("google/gemma-4-31b-it:free");

        await foreach (var _ in service.StreamChatAsync(payload, null, "pt")) { }

        Assert.Equal(2, requestBodies.Count);
        Assert.Contains("\"google/gemma-4-31b-it:free\"", requestBodies[0]);
        Assert.Contains("\"openrouter/free\"", requestBodies[1]);

        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task StreamChatAsync_RetriesWithoutReasoning_WhenModelRejectsIt()
    {
        var responses = new Queue<HttpResponseMessage>(
            new[]
            {
                new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"error\":\"reasoning not supported\"}", Encoding.UTF8, "application/json")
                },
                SseResponse("""{"choices":[{"delta":{"content":"ok"}}],"model":"nvidia/foo"}""")
            });

        var service = CreateService(responses, out var requestBodies);
        var payload = CreatePayload("nvidia/foo");
        payload.EnableReasoning = true;

        await foreach (var _ in service.StreamChatAsync(payload, null, "pt")) { }

        Assert.Equal(2, requestBodies.Count);
        Assert.Contains("\"reasoning\"", requestBodies[0]);
        Assert.DoesNotContain("\"reasoning\"", requestBodies[1]);

        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task StreamChatAsync_RecordsResponse_WithServedModelDifference()
    {
        var responses = SseQueue(
            """{"choices":[{"delta":{"content":"final"}}],"model":"openrouter/free"}""",
            """{"choices":[{"delta":{}}],"usage":{"prompt_tokens":5,"completion_tokens":5,"total_tokens":10}}""");

        var benchmark = new Mock<IAIBenchmarkRepository>();
        benchmark
            .Setup(x => x.GetOrCreatePromptIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);
        benchmark
            .Setup(x => x.CreateResponseAsync(It.IsAny<AIResponse>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(responses, out _, benchmarkMock: benchmark);

        await foreach (var _ in service.StreamChatAsync(CreatePayload("google/gemma-4-31b-it:free"), "user-7", "en")) { }

        benchmark.Verify(
            x => x.GetOrCreatePromptIdAsync(
                "Olá", "user-7", It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
            Times.Once);

        benchmark.Verify(
            x => x.CreateResponseAsync(
                It.Is<AIResponse>(r => r.NomeModelo == "google/gemma-4-31b-it:free"
                    && r.ModeloResolvido == "openrouter/free"
                    && r.UserId == "user-7"
                    && r.TamanhoTokens == 5
                    && r.TextoResposta == "final"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task StreamChatAsync_DowngradesPaidModel_WhenToggleOff()
    {
        var responses = SseQueue(
            """{"choices":[{"delta":{"content":"ok"}}],"model":"openrouter/free"}""",
            """{"choices":[{"delta":{}}],"usage":{"prompt_tokens":1,"completion_tokens":1,"total_tokens":2}}""");

        var service = CreateService(responses, out var requestBodies);
        var payload = CreatePayload("nvidia/foo");

        await foreach (var _ in service.StreamChatAsync(payload, null, "pt")) { }

        Assert.Single(requestBodies);
        Assert.Contains("\"openrouter/free\"", requestBodies[0]);
        Assert.DoesNotContain("\"nvidia/foo\"", requestBodies[0]);

        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task StreamChatAsync_SweepsToCatalogModel_WhenRouterProducesNoText()
    {
        var responses = new Queue<HttpResponseMessage>(
            new[]
            {
                SseResponse(
                    """{"choices":[{"delta":{}}],"usage":{"prompt_tokens":10,"completion_tokens":0,"total_tokens":10}}""" +
                    "\ndata: [DONE]\n"),
                SseResponse(
                    """{"choices":[{"delta":{"content":"ok"}}],"model":"z-ai/glm-5.2:free"}""" +
                    "\ndata: [DONE]\n")
            });

        var service = CreateService(
            responses,
            out var requestBodies,
            catalogResponse: """{"data":[{"id":"z-ai/glm-5.2:free","name":"GLM 5.2"}]}""");
        var payload = CreatePayload("openrouter/free");

        var chunks = new List<CloudChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(payload, null, "pt"))
            chunks.Add(chunk);

        Assert.Equal("ok", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal(2, requestBodies.Count);
        Assert.Contains("\"openrouter/free\"", requestBodies[0]);
        Assert.Contains("\"z-ai/glm-5.2:free\"", requestBodies[1]);

        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task StreamChatAsync_YieldsChunksWhileStreamIsStillOpen()
    {
        var beforeGate =
            """data: {"choices":[{"delta":{"content":"Olá"}}],"model":"nvidia/foo"}""" + "\n";
        var afterGate =
            """data: {"choices":[{"delta":{"content":" mundo"}}]}""" + "\n" +
            """data: {"choices":[{"delta":{}}],"usage":{"prompt_tokens":10,"completion_tokens":3,"total_tokens":13}}""" + "\n" +
            "data: [DONE]\n";

        var stream = new GatedSseStream(beforeGate, afterGate);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(stream) { Headers = { { "Content-Type", "text/event-stream" } } }
        };

        var service = CreateService(new Queue<HttpResponseMessage>(new[] { response }), out _);

        var firstChunk = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var consume = Task.Run(async () =>
        {
            var text = new StringBuilder();
            await foreach (var chunk in service.StreamChatAsync(CreatePayload("openrouter/free"), "user-1", "pt"))
            {
                text.Append(chunk.Text);
                if (text.Length > 0 && !firstChunk.Task.IsCompleted)
                    firstChunk.TrySetResult(text.ToString());
            }
        });

        // Sem o fix, o serviço faz ReadAsStringAsync e bloqueia até ao fim do stream, pelo
        // que o primeiro chunk nunca chega antes do Release(). Com o fix, o streaming é
        // incremental: o primeiro chunk chega ainda com o stream aberto.
        var received = await firstChunk.Task.WaitAsync(TimeSpan.FromSeconds(3));

        Assert.Equal("Olá", received);
        Assert.False(stream.IsReleased);

        stream.Release();
        await consume.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.True(stream.IsReleased);

        response.Dispose();
    }

    private static CloudChatPayload CreatePayload(string model) => new()
    {
        Model = model,
        Temperature = 0.7,
        MaxTokens = 2048,
        Messages = new List<CloudChatMessage>
        {
            new() { Role = "user", Content = "Olá" }
        }
    };

    private static Queue<HttpResponseMessage> SseQueue(params string[] rawLines)
    {
        var sse = string.Join("\n", rawLines.Select(l => "data: " + l)) + "\ndata: [DONE]\n";
        return new Queue<HttpResponseMessage>(new[] { SseResponse(sse) });
    }

    private static HttpResponseMessage SseResponse(string sse)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sse));
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(stream)
            {
                Headers = { { "Content-Type", "text/event-stream" } }
            }
        };
    }

    private static OpenRouterChatService CreateService(
        Queue<HttpResponseMessage> responses,
        out List<string> requestBodies,
        string? apiKey = ApiKey,
        Mock<IAIBenchmarkRepository>? benchmarkMock = null,
        int freeRequestsRemaining = 1000,
        string catalogResponse = EmptyCatalogResponse)
    {
        requestBodies = new List<string>();
        var bodies = requestBodies;

        var settings = new Mock<IAIChatSettingsRepository>();
        settings
            .Setup(x => x.GetValueAsync(CloudChatSettingsKeys.OpenRouterApiKey, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKey);

        var benchmark = benchmarkMock ?? new Mock<IAIBenchmarkRepository>();
        if (benchmarkMock is null)
        {
            benchmark
                .Setup(x => x.GetOrCreatePromptIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            benchmark
                .Setup(x => x.CreateResponseAsync(It.IsAny<AIResponse>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        var httpClient = new HttpClient(new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/models", StringComparison.OrdinalIgnoreCase) == true)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(catalogResponse, Encoding.UTF8, "application/json")
                };
            }

            if (responses.Count == 0)
                throw new InvalidOperationException("No canned responses left.");

            if (request.Content is not null)
                bodies.Add(request.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            return responses.Dequeue();
        }));

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var internet = new Mock<IInternetAvailabilityService>();
        internet.Setup(x => x.IsReachableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var usageMock = new Mock<ICloudChatUsageService>();
        usageMock
            .Setup(x => x.GetFreeRequestsRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeRequestsRemaining);

        return new OpenRouterChatService(
            factory.Object,
            settings.Object,
            benchmark.Object,
            usageMock.Object,
            internet.Object,
            new OpenRouterCatalogService(factory.Object, settings.Object, internet.Object, Mock.Of<ILogger<OpenRouterCatalogService>>()),
            Mock.Of<ILogger<OpenRouterChatService>>());
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }

    /// <summary>
    /// Stream SSE simulando um servidor lento: devolve imediatamente o primeiro segmento e
    /// bloqueia antes de expor o restante até <see cref="Release"/> ser chamado. Simula o
    /// streaming incremental real do OpenRouter (TTFT antes do fim do stream).
    /// </summary>
    private sealed class GatedSseStream : Stream
    {
        private readonly byte[][] _segments;
        private readonly ManualResetEventSlim _gate = new(false);
        private int _segmentIndex;
        private int _offset;

        public bool IsReleased => _gate.IsSet;

        public GatedSseStream(string beforeGate, string afterGate)
        {
            _segments = new[]
            {
                Encoding.UTF8.GetBytes(beforeGate),
                Encoding.UTF8.GetBytes(afterGate)
            };
        }

        public void Release() => _gate.Set();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer.AsMemory(offset, count)).AsTask().GetAwaiter().GetResult();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                if (_segmentIndex >= _segments.Length)
                    return 0;

                var current = _segments[_segmentIndex];

                if (_offset >= current.Length)
                {
                    // O último segmento não tem gate; os anteriores exigem Release().
                    if (_segmentIndex == _segments.Length - 1 || _gate.IsSet)
                    {
                        _segmentIndex++;
                        _offset = 0;
                        continue;
                    }

                    _gate.Wait(cancellationToken);
                    _segmentIndex++;
                    _offset = 0;
                    continue;
                }

                var toCopy = Math.Min(buffer.Length, current.Length - _offset);
                current.AsMemory(_offset, toCopy).CopyTo(buffer[..toCopy]);
                _offset += toCopy;
                return toCopy;
            }
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}