using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests
{
    public class ChatFeedbackControllerTests
    {
        private static ChatFeedbackController Build(Mock<IChatFeedbackRepository> repo, bool withHeaders = false, string? userId = null)
        {
            var logger = new Mock<ILogger<ChatFeedbackController>>();
            var controller = new ChatFeedbackController(repo.Object, logger.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            if (withHeaders && !string.IsNullOrWhiteSpace(userId))
            {
                controller.Request.Headers["X-UserId"] = userId;
            }

            return controller;
        }

        private static ChatFeedbackDto Valid()
        {
            return new ChatFeedbackDto
            {
                Interface = "openrouter",
                ConversationId = "conv-1",
                MessageIndex = 2,
                PromptText = "O que é x?",
                ResponseText = "Resposta do assistente.",
                ModelName = "openrouter/free",
                Rating = 1
            };
        }

        [Fact]
        public async Task Upsert_WithValidBody_ReturnsOk_AndSetsUserId()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo, withHeaders: true, userId: "user-1");

            var body = Valid();
            var result = await controller.Upsert(body, CancellationToken.None);

            Assert.IsType<OkResult>(result);
            Assert.Equal("user-1", body.UserId);
            repo.Verify(r => r.UpsertAsync(body, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Upsert_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo);

            var result = await controller.Upsert(Valid(), CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, bad.StatusCode);
            repo.Verify(r => r.UpsertAsync(It.IsAny<ChatFeedbackDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Upsert_WithInvalidInterface_ReturnsBadRequest()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo, withHeaders: true, userId: "user-1");

            var body = Valid();
            body.Interface = "anthropic";
            var result = await controller.Upsert(body, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Upsert_WithInvalidRating_ReturnsBadRequest()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo, withHeaders: true, userId: "user-1");

            var body = Valid();
            body.Rating = 5;
            var result = await controller.Upsert(body, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Upsert_RatingZeroIsAllowed_PersistsClearedRating()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo, withHeaders: true, userId: "user-1");

            var body = Valid();
            body.Rating = 0;
            var result = await controller.Upsert(body, CancellationToken.None);

            Assert.IsType<OkResult>(result);
            repo.Verify(r => r.UpsertAsync(body, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByConversation_ReturnsOk_WithRatings()
        {
            var expected = new List<ChatFeedbackDto>
            {
                new ChatFeedbackDto { Interface = "openrouter", MessageIndex = 2, Rating = 1, ResponseText = "x" }
            };
            var repo = new Mock<IChatFeedbackRepository>();
            repo.Setup(r => r.GetByConversationAsync("user-1", "openrouter", "conv-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = Build(repo, withHeaders: true, userId: "user-1");
            var result = await controller.GetByConversation("openrouter", "conv-1", CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<List<ChatFeedbackDto>>(ok.Value);
            Assert.Single(body);
            Assert.Equal(1, body[0].Rating);
        }

        [Fact]
        public async Task GetByConversation_WithoutUserIdHeader_ReturnsBadRequest()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            var controller = Build(repo);

            var result = await controller.GetByConversation("openrouter", "conv-1", CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, bad.StatusCode);
        }

        [Fact]
        public async Task Upsert_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IChatFeedbackRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<ChatFeedbackDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var controller = Build(repo, withHeaders: true, userId: "user-1");
            var result = await controller.Upsert(Valid(), CancellationToken.None);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        }
    }
}