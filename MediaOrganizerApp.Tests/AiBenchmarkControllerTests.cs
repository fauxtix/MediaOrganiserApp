using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests
{
    public class AiBenchmarkControllerTests
    {
        private static AiBenchmarkController Build(Mock<IAIBenchmarkRepository> repo, bool withHeaders = false, string? userId = null, bool isAdmin = false)
        {
            var logger = new Mock<ILogger<AiBenchmarkController>>();
            var controller = new AiBenchmarkController(repo.Object, logger.Object);
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
            var expected = new AiBenchmarkSummaryDto
            {
                TotalRespostas = 42,
                Ultimos7Dias = 7,
                LatenciaMediaSegundos = 1.5,
                TokensPorSegundoMedio = 22.3,
                TamanhoTokensMedio = 512,
                ModelosMaisUsados = { new ModeloUsageContagem { Modelo = "openrouter/free", Contagem = 30 } }
            };
            var repo = new Mock<IAIBenchmarkRepository>();
            repo.Setup(r => r.GetBenchmarkSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = Build(repo, withHeaders: true, userId: "user-1", isAdmin: true);
            var result = await controller.GetSummary(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<AiBenchmarkSummaryDto>(ok.Value);
            Assert.Equal(42, body.TotalRespostas);
            Assert.Equal("openrouter/free", Assert.Single(body.ModelosMaisUsados).Modelo);
        }

        [Fact]
        public async Task GetSummary_ForwardsUserIdAndIsAdminFromHeaders()
        {
            var repo = new Mock<IAIBenchmarkRepository>();
            repo.Setup(r => r.GetBenchmarkSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiBenchmarkSummaryDto());

            var controller = Build(repo, withHeaders: true, userId: "u-42", isAdmin: true);
            await controller.GetSummary(CancellationToken.None);

            repo.Verify(r => r.GetBenchmarkSummaryAsync("u-42", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSummary_WithoutHeaders_UsesNullUserIdAndFalseAdmin()
        {
            var repo = new Mock<IAIBenchmarkRepository>();
            repo.Setup(r => r.GetBenchmarkSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AiBenchmarkSummaryDto());

            var controller = Build(repo);
            await controller.GetSummary(CancellationToken.None);

            repo.Verify(r => r.GetBenchmarkSummaryAsync(null, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSummary_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IAIBenchmarkRepository>();
            repo.Setup(r => r.GetBenchmarkSummaryAsync(It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var controller = Build(repo);
            var result = await controller.GetSummary(CancellationToken.None);

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        }
    }
}