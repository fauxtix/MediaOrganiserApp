using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MediaOrganizerApp.Tests;

public sealed class OllamaTranslateServiceTests
{
    private const string FencedJsonResponse = """{"response":"Here is the translation:\n```json\n{\"translatedText\":\"Olá mundo\",\"confidence\":0.91}\n```"}""";
    private const string BacktickTextResponse = """{"response":"`Olá mundo`"}""";

    [Fact]
    public async Task TranslateAsync_Parses_EmbeddedJsonInsideMarkdownFence()
    {
        var service = CreateService(FencedJsonResponse);

        var result = await service.TranslateAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal("Olá mundo", result.TranslatedText);
        Assert.Equal(0.91, result.Confidence, 3);
        Assert.True(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public async Task TranslateAsync_FallsBackToPlainText_WhenResponseIsBacktickWrappedText()
    {
        var service = CreateService(BacktickTextResponse);

        var result = await service.TranslateAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal("Olá mundo", result.TranslatedText);
        Assert.Equal(0.2, result.Confidence, 3);
        Assert.True(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public async Task TranslateAsync_Publishes_WaitingAndResponseStages_InOrder()
    {
        var service = CreateService("""{"response":"{\"translatedText\":\"Olá mundo\",\"confidence\":0.91}"}""", out var progressPublisher);

        await service.TranslateAsync(CreateRequest(), CancellationToken.None);

        var stages = progressPublisher.Invocations
            .Where(i => i.Method.Name == nameof(IProgressPublisher.PublishAsync))
            .Select(i => ((ProgressUpdateDto)i.Arguments[1]).Stage)
            .ToList();

        var requestSentIndex = stages.IndexOf("request_sent");
        var waitingResponseIndex = stages.IndexOf("waiting_response");
        var responseReceivedIndex = stages.IndexOf("response_received");
        var parsingIndex = stages.IndexOf("parsing");

        Assert.True(requestSentIndex >= 0);
        Assert.True(waitingResponseIndex > requestSentIndex);
        Assert.True(responseReceivedIndex > waitingResponseIndex);
        Assert.True(parsingIndex > responseReceivedIndex);
    }

    private static TranslateRequestDto CreateRequest()
        => new()
        {
            Text = "Hello world",
            SourceLanguage = "en",
            TargetLanguage = "pt"
        };

    private static OllamaTranslateService CreateService(string responseContent)
        => CreateService(responseContent, out _);

    private static OllamaTranslateService CreateService(string responseContent, out Mock<IProgressPublisher> progressPublisher)
    {
        var promptProvider = new Mock<IPromptTemplateProvider>();
        promptProvider
            .Setup(x => x.GetTemplateAsync("translate-text.prompt.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Translate {text}");

        var aiSettingsProvider = new Mock<MediaOrganizerApp.WebApi.Services.IAiSettingsProvider>();
        aiSettingsProvider
            .Setup(x => x.GetAiSettingsAsync())
            .ReturnsAsync(("http://localhost:11434", "test-model"));

        var systemPromptService = new Mock<ISystemPromptService>();
        systemPromptService
            .Setup(x => x.GetSystemPrompt(It.IsAny<CancellationToken>()))
            .ReturnsAsync("System prompt");

        progressPublisher = new Mock<IProgressPublisher>();
        progressPublisher
            .Setup(x => x.PublishAsync(It.IsAny<ProgressChannel>(), It.IsAny<ProgressUpdateDto>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpClient = new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        }));

        return new OllamaTranslateService(
            httpClient,
            Mock.Of<ILogger<OllamaTranslateService>>(),
            promptProvider.Object,
            progressPublisher.Object,
            aiSettingsProvider.Object,
            systemPromptService.Object);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(handler(request));
    }
}
