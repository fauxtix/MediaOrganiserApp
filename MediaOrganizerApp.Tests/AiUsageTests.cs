using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.WebApi.Controllers;
using MediaOrganizerApp.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests
{
    public class AiUsageControllerTests
    {
        private static AiUsageController Build(Mock<IAiUsageLogRepository> repo, bool withHeaders = false, string? userId = null, bool isAdmin = false)
        {
            var logger = new Mock<ILogger<AiUsageController>>();
            var controller = new AiUsageController(repo.Object, logger.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            if (withHeaders)
            {
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    controller.Request.Headers["X-UserId"] = userId;
                }
                controller.Request.Headers["X-IsAdmin"] = isAdmin ? "true" : "false";
            }

            return controller;
        }

        [Fact]
        public async Task GetSummary_ReturnsOk_WithSummary()
        {
            var expected = new AiUsageSummaryDto
            {
                TotalGeral = 42,
                Ultimos7 = 7,
                UsosOpenRouter = 30,
                UsosOllama = 12,
                UsosFallback = 9,
                PorOperacao = { new AiUsageOperacaoDto { Operacao = "translate", Contagem = 42 } },
                UltimosRegistos = { new AiUsageRegistoDto { Id = 1, Operacao = "translate", Provider = "openrouter" } }
            };
            var repo = new Mock<IAiUsageLogRepository>();
            repo.Setup(r => r.GetSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = Build(repo, withHeaders: true, userId: "user-1", isAdmin: true);
            var result = await controller.GetSummary(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<AiUsageSummaryDto>(ok.Value);
            Assert.Equal(42, body.TotalGeral);
            Assert.Equal("translate", Assert.Single(body.PorOperacao).Operacao);
        }

        [Fact]
        public async Task GetSummary_ForwardsUserIdAndIsAdminFromHeaders()
        {
            var repo = new Mock<IAiUsageLogRepository>();
            repo.Setup(r => r.GetSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiUsageSummaryDto());

            var controller = Build(repo, withHeaders: true, userId: "u-42", isAdmin: true);
            await controller.GetSummary(CancellationToken.None);

            repo.Verify(r => r.GetSummaryAsync("u-42", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSummary_WithoutHeaders_UsesNullUserIdAndFalseAdmin()
        {
            var repo = new Mock<IAiUsageLogRepository>();
            repo.Setup(r => r.GetSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiUsageSummaryDto());

            var controller = Build(repo);
            await controller.GetSummary(CancellationToken.None);

            repo.Verify(r => r.GetSummaryAsync(null, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSummary_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IAiUsageLogRepository>();
            repo.Setup(r => r.GetSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var controller = Build(repo);
            var result = await controller.GetSummary(CancellationToken.None);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        }
    }

    public class AiUsageRecorderServiceTests
    {
        private static AiUsageRecorderService Build(out Mock<IAiUsageLogRepository> repo)
        {
            repo = new Mock<IAiUsageLogRepository>();
            return new AiUsageRecorderService(repo.Object, new Mock<ILogger<AiUsageRecorderService>>().Object);
        }

        [Fact]
        public async Task RecordAsync_LocalProvider_SetsEraFallbackAndCloudFallbackScenario()
        {
            var service = Build(out var repo);
            await service.RecordAsync("u-1", "media-notes", "ollama", durationMs: 123, contexto: "Artist - Title");

            repo.Verify(r => r.InsertAsync(
                It.Is<AiUsageLog>(l =>
                    l.UserId == "u-1" &&
                    l.Operacao == "media-notes" &&
                    l.Provider == "ollama" &&
                    l.EraFallback &&
                    l.Scenario == "cloud-fallback" &&
                    l.Contexto == "Artist - Title" &&
                    Math.Abs(l.DurationMs - 123) < 0.001 &&
                    l.Sucesso),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RecordAsync_CloudProvider_KeepsProvidedScenario()
        {
            var service = Build(out var repo);
            await service.RecordAsync("u-1", "book-summary", "openrouter", scenario: "googlebooks", sucesso: true);

            repo.Verify(r => r.InsertAsync(
                It.Is<AiUsageLog>(l =>
                    l.Provider == "openrouter" &&
                    !l.EraFallback &&
                    l.Scenario == "googlebooks"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RecordAsync_WithoutScenario_UsesDirectForCloud()
        {
            var service = Build(out var repo);
            await service.RecordAsync(null, "translate", "openrouter", sucesso: true);

            repo.Verify(r => r.InsertAsync(
                It.Is<AiUsageLog>(l => l.Scenario == "direct" && l.UserId == null),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RecordAsync_WhenRepoThrows_DoesNotPropagate()
        {
            var service = Build(out var repo);
            repo.Setup(r => r.InsertAsync(It.IsAny<AiUsageLog>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db down"));

            // Must not throw even though the repository failed.
            await service.RecordAsync("u-1", "translate", "ollama");
        }
    }
}