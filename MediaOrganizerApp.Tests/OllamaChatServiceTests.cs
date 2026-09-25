using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class OllamaChatServiceTests
{
    [Fact]
    public async Task StreamChatAsync_PreservesWhitespaceAndLineBreaks_BetweenTokens()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"Oh"},"done":false}""",
            """{"message":{"role":"assistant","content":","},"done":false}""",
            """{"message":{"role":"assistant","content":" this"},"done":false}""",
            """{"message":{"role":"assistant","content":" is"},"done":false}""",
            """{"message":{"role":"assistant","content":" challenging"},"done":false}""",
            """{"message":{"role":"assistant","content":".\n\n"},"done":false}""",
            """{"message":{"role":"assistant","content":"Eric"},"done":true}"""
        });

        var service = CreateService(ndjson);

        var text = await CollectTextAsync(service);

        Assert.Equal("Oh, this is challenging.\n\nEric", text);
    }

    [Fact]
    public async Task StreamChatAsync_PreservesReasoningSpacing_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"","thinking":"Let"},"done":false}""",
            """{"message":{"role":"assistant","content":"","thinking":" me"},"done":false}""",
            """{"message":{"role":"assistant","content":"","thinking":" think"},"done":false}""",
            """{"message":{"role":"assistant","content":"Hello"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Let me think", string.Concat(chunks.Select(c => c.Reasoning)));
        Assert.Equal("Hello", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_DiscardsThinking_WhenReasoningDisabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"","thinking":"Hello"},"done":false}""",
            """{"message":{"role":"assistant","content":"","thinking":" world"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: false);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task StreamChatAsync_WhitespaceContentDoesNotHideThinking_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":" ","thinking":"Let"},"done":false}""",
            """{"message":{"role":"assistant","content":"\n","thinking":" me"},"done":false}""",
            """{"message":{"role":"assistant","content":"","thinking":" think"},"done":false}""",
            """{"message":{"role":"assistant","content":"Hello"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Let me think", string.Concat(chunks.Where(c => !string.IsNullOrEmpty(c.Reasoning)).Select(c => c.Reasoning)));
        Assert.Equal(" \nHello", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_ContentStillWins_WhenModelHasNoReasoning()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"Hi"},"done":false}""",
            """{"message":{"role":"assistant","content":" there"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Hi there", string.Concat(chunks.Select(c => c.Text)));
        Assert.All(chunks, c => Assert.True(string.IsNullOrEmpty(c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_ReadsReasoningContentField_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"","reasoning_content":"Let"},"done":false}""",
            """{"message":{"role":"assistant","content":"","reasoning_content":" me"},"done":false}""",
            """{"message":{"role":"assistant","content":"","reasoning_content":" think"},"done":false}""",
            """{"message":{"role":"assistant","content":"Hello"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Let me think", string.Concat(chunks.Select(c => c.Reasoning)));
        Assert.Equal("Hello", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_CapturesReasoning_WhenOnlyInFinalDoneMessage()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":""},"done":false}""",
            """{"message":{"role":"assistant","content":""},"done":false}""",
            """{"message":{"role":"assistant","content":"Answer"},"thinking":"final thinking only","done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Answer", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal("final thinking only", string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_DoesNotDoubleEmitReasoning_WhenThinkingPresentEverywhere()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"","thinking":"inc. thinking"},"done":false}""",
            """{"message":{"role":"assistant","content":"Done"},"thinking":"final thinking only","done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Done", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal("inc. thinkingfinal thinking only", string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_CapturesInlineThinking_AcrossChunks_WhenReasoningEnabled()
    {
        // Qwen3 sometimes streams the thinking block INLINE inside `content`
        // wrapped in `\u003Cthink\u003E`/`\u003C/think\u003E` instead of the
        // `thinking` field. The block can span several NDJSON tokens.
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003ELet"},"done":false}""",
            """{"message":{"role":"assistant","content":" me"},"done":false}""",
            """{"message":{"role":"assistant","content":" think"},"done":false}""",
            """{"message":{"role":"assistant","content":"\u003C/think\u003EHello"},"done":false}""",
            """{"message":{"role":"assistant","content":" world"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Let me think", string.Concat(chunks.Select(c => c.Reasoning)));
        Assert.Equal("Hello world", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_DiscardsInlineThinking_WhenReasoningDisabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003ESecret reasoning"},"done":false}""",
            """{"message":{"role":"assistant","content":" never"},"done":false}""",
            """{"message":{"role":"assistant","content":"\u003C/think\u003EVisible"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: false);

        var text = await CollectTextAsync(service);

        Assert.Equal("Visible", text);
    }

    [Fact]
    public async Task StreamChatAsync_HandlesNakedInlineThinkingMarkers_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":" thinkingDo"},"done":false}""",
            """{"message":{"role":"assistant","content":" good"},"done":false}""",
            """{"message":{"role":"assistant","content":" responseDone"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Do good", string.Concat(chunks.Select(c => c.Reasoning)));
        Assert.Equal("Done", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_InlineThinking_FlushesAtFinalDone_WhenBlockNeverClosed()
    {
        // Model streams the opening marker and thinking content but the stream
        // ends (done:true) before the closing marker arrives: the pending inline
        // reasoning must still be surfaced, never lost.
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003EAlmost"},"done":false}""",
            """{"message":{"role":"assistant","content":" there"},"done":false}""",
            """{"message":{"role":"assistant","content":""},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("Almost there", string.Concat(chunks.Select(c => c.Reasoning)));
        Assert.Equal(string.Empty, string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_UnclosedInlineThinking_DoesNotLeakIntoText_WhenReasoningDisabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003EForgotten"},"done":false}""",
            """{"message":{"role":"assistant","content":" thinking"},"done":false}""",
            """{"message":{"role":"assistant","content":""},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: false);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Empty(chunks);
    }

    [Fact]
    public async Task StreamChatAsync_HandlesInlineMarkerSplitAcrossChunks_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"Before\u003Cthi"},"done":false}""",
            """{"message":{"role":"assistant","content":"nk\u003Einside"},"done":false}""",
            """{"message":{"role":"assistant","content":"\u003C/thi"},"done":false}""",
            """{"message":{"role":"assistant","content":"nk\u003EAfter"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("BeforeAfter", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal("inside", string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_DropsOrphanInlineCloseMarker_WhenReasoningEnabled()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"Before\u003C/think\u003EAfter"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("BeforeAfter", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal(string.Empty, string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_FlushesInlineReasoningAtEndOfStream_WhenDoneMarkerIsMissing()
    {
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003EAlmost"},"done":false}""",
            """{"message":{"role":"assistant","content":" there"},"done":false}""",
            """{"message":{"role":"assistant","content":""},"done":false}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal(string.Empty, string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal("Almost there", string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_KeepsNakedResponseTextOutsideThinking()
    {
        var ndjson = """{"message":{"role":"assistant","content":"The response is ready"},"done":true}""";

        var service = CreateService(ndjson, reasoningEnabled: true);

        var text = await CollectTextAsync(service);

        Assert.Equal("The response is ready", text);
    }

    [Fact]
    public async Task StreamChatAsync_HandlesMixedCaseInlineMarkers_WhenReasoningEnabled()
    {
        var ndjson = """{"message":{"role":"assistant","content":"\u003CTHINK\u003Ereason\u003C/THINK\u003Eanswer"},"done":true}""";

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("answer", string.Concat(chunks.Select(c => c.Text)));
        Assert.Equal("reason", string.Concat(chunks.Select(c => c.Reasoning)));
    }

    [Fact]
    public async Task StreamChatAsync_FieldThinkingWins_WhenInlineMarkersAlsoPresent()
    {
        // When a single chunk carries BOTH a `thinking` field AND inline markers,
        // the field-based reasoning must take priority over the inline-extracted
        // block.
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"\u003Cthink\u003Einline\u003C/think\u003EText","thinking":"fieldexplicit"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var chunks = new List<AiChatChunk>();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            chunks.Add(chunk);

        Assert.Equal("fieldexplicit", string.Concat(chunks.Select(c => c.Reasoning)));
        // The inline block (including its markers) must not leak into the answer.
        Assert.Equal("Text", string.Concat(chunks.Select(c => c.Text)));
    }

    [Fact]
    public async Task StreamChatAsync_InlineBlock_StreamsReasoningBetweenAnswerPieces()
    {
        // Answer text before the thinking block AND after it, with the block
        // itself captured as reasoning (mixed-model behaviour).
        var ndjson = string.Join("\n", new[]
        {
            """{"message":{"role":"assistant","content":"Sure"},"done":false}""",
            """{"message":{"role":"assistant","content":"\u003Cthink\u003Echecking"},"done":false}""",
            """{"message":{"role":"assistant","content":" facts"},"done":false}""",
            """{"message":{"role":"assistant","content":"\u003C/think\u003E,"},"done":false}""",
            """{"message":{"role":"assistant","content":" the answer is yes"},"done":true}"""
        });

        var service = CreateService(ndjson, reasoningEnabled: true);

        var text = string.Empty;
        var reasoningText = new StringBuilder();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
        {
            text += chunk.Text;
            reasoningText.Append(chunk.Reasoning);
        }

        Assert.Equal("checking facts", reasoningText.ToString());
        Assert.Equal("Sure, the answer is yes", text);
    }

    [Fact]
    public async Task StreamChatAsync_SendsThinkField_WhenModelSupportsThinking()
    {
        var ndjson = """{"message":{"role":"assistant","content":"Hi"},"done":true}""";
        string? requestBody = null;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ndjson, Encoding.UTF8, "application/x-ndjson")
        };

        var aiSettingsProvider = new Mock<MediaOrganizerApp.WebApi.Services.IAiSettingsProvider>();
        aiSettingsProvider
            .Setup(x => x.GetAiSettingsAsync())
            .ReturnsAsync(("http://localhost:11434", "qwen3:1.7b"));

        var progressPublisher = new Mock<IProgressPublisher>();
        progressPublisher
            .Setup(x => x.PublishAsync(It.IsAny<ProgressChannel>(), It.IsAny<ProgressUpdateDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var settingsRepository = new Mock<IAIChatSettingsRepository>();
        settingsRepository
            .Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("true");

        var httpClient = new HttpClient(new StubHttpMessageHandler(req =>
        {
            requestBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return response;
        }));

        var service = new OllamaChatService(
            httpClient,
            Mock.Of<ILogger<OllamaChatService>>(),
            aiSettingsProvider.Object,
            progressPublisher.Object,
            settingsRepository.Object);

        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
        { }

        Assert.NotNull(requestBody);
        using var doc = System.Text.Json.JsonDocument.Parse(requestBody);
        Assert.True(doc.RootElement.TryGetProperty("think", out var think));
        Assert.True(think.GetBoolean());
    }

    [Fact]
    public async Task StreamChatAsync_OmitsThinkField_WhenModelDoesNotSupportThinking()
    {
        var ndjson = """{"message":{"role":"assistant","content":"Hi"},"done":true}""";
        string? requestBody = null;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ndjson, Encoding.UTF8, "application/x-ndjson")
        };

        var aiSettingsProvider = new Mock<MediaOrganizerApp.WebApi.Services.IAiSettingsProvider>();
        aiSettingsProvider
            .Setup(x => x.GetAiSettingsAsync())
            .ReturnsAsync(("http://localhost:11434", "llama3.2:latest"));

        var progressPublisher = new Mock<IProgressPublisher>();
        progressPublisher
            .Setup(x => x.PublishAsync(It.IsAny<ProgressChannel>(), It.IsAny<ProgressUpdateDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var settingsRepository = new Mock<IAIChatSettingsRepository>();
        settingsRepository
            .Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("true");

        var httpClient = new HttpClient(new StubHttpMessageHandler(req =>
        {
            requestBody = req.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return response;
        }));

        var service = new OllamaChatService(
            httpClient,
            Mock.Of<ILogger<OllamaChatService>>(),
            aiSettingsProvider.Object,
            progressPublisher.Object,
            settingsRepository.Object);

        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
        { }

        Assert.NotNull(requestBody);
        using var doc = System.Text.Json.JsonDocument.Parse(requestBody);
        Assert.False(doc.RootElement.TryGetProperty("think", out _));
    }

    private static async Task<string> CollectTextAsync(OllamaChatService service)
    {
        var text = new StringBuilder();
        await foreach (var chunk in service.StreamChatAsync(new List<ChatMessageDto>(), null, null, CancellationToken.None))
            text.Append(chunk.Text);
        return text.ToString();
    }

    private static OllamaChatService CreateService(string ndjsonResponse, bool reasoningEnabled = false)
    {
        var aiSettingsProvider = new Mock<MediaOrganizerApp.WebApi.Services.IAiSettingsProvider>();
        aiSettingsProvider
            .Setup(x => x.GetAiSettingsAsync())
            .ReturnsAsync(("http://localhost:11434", "test-model"));

        var progressPublisher = new Mock<IProgressPublisher>();
        progressPublisher
            .Setup(x => x.PublishAsync(It.IsAny<ProgressChannel>(), It.IsAny<ProgressUpdateDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var settingsRepository = new Mock<IAIChatSettingsRepository>();
        settingsRepository
            .Setup(x => x.GetValueAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reasoningEnabled ? "true" : (string?)null);

        var httpClient = new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ndjsonResponse, Encoding.UTF8, "application/x-ndjson")
        }));

        return new OllamaChatService(
            httpClient,
            Mock.Of<ILogger<OllamaChatService>>(),
            aiSettingsProvider.Object,
            progressPublisher.Object,
            settingsRepository.Object);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}
