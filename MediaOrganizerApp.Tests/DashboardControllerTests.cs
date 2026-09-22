using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Shared.DTOs;
using MediaOrganizerApp.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaOrganizerApp.Tests
{
    public class DashboardControllerTests
    {
        private static DashboardController Build(
            Mock<IStatsRepository> repoStats,
            params Mock[] _)
        {
            var logger = new Mock<ILogger<AuthorsController>>();
            var mapper = new Mock<IMapper>();

            var controller = new DashboardController(
                logger.Object,
                mapper.Object,
                new Mock<IAutoresRepository>().Object,
                new Mock<IEditorasRepository>().Object,
                new Mock<IFormatosRepository>().Object,
                new Mock<IFormatos_MediaRepository>().Object,
                new Mock<ICategoriasRepository>().Object,
                new Mock<ILocalArquivoRepository>().Object,
                new Mock<IMediaRepository>().Object,
                repoStats.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Dashboard",
                    ActionName = "Health"
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetHealth_ReturnsOk_WithCounters()
        {
            var repo = new Mock<IStatsRepository>();
            repo.Setup(r => r.GetCountFromPubsWithoutCover()).ReturnsAsync(11);
            repo.Setup(r => r.GetCountFromPubsWithoutPages()).ReturnsAsync(22);
            repo.Setup(r => r.GetCountFromAuthorsWithoutPubs()).ReturnsAsync(3);
            repo.Setup(r => r.GetCountFromEditorasWithoutPubs()).ReturnsAsync(4);
            repo.Setup(r => r.GetCountFromGenerosWithoutPubs()).ReturnsAsync(5);
            repo.Setup(r => r.GetCountFromFormatosWithoutPubs()).ReturnsAsync(6);
            repo.Setup(r => r.GetCountFromLocaisArquivoWithoutPubs()).ReturnsAsync(7);
            repo.Setup(r => r.GetTitulosDuplicados())
                .ReturnsAsync(new List<StatRecord> { new(), new() });

            var controller = Build(repo);
            var result = await controller.GetHealth();

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<DashboardHealthDto>(ok.Value);
            Assert.Equal(11, body.PubsSemCapa);
            Assert.Equal(22, body.PubsSemPaginas);
            Assert.Equal(3, body.AutoresSemPublicacoes);
            Assert.Equal(4, body.EditorasSemPublicacoes);
            Assert.Equal(5, body.CategoriasSemPublicacoes);
            Assert.Equal(6, body.FormatosSemPublicacoes);
            Assert.Equal(7, body.LocaisArquivoSemPublicacoes);
            Assert.Equal(2, body.TitulosDuplicados);
        }

        [Fact]
        public async Task GetHealth_TitulosDuplicadosNull_ReturnsZero()
        {
            var repo = new Mock<IStatsRepository>();
            repo.Setup(r => r.GetCountFromPubsWithoutCover()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromPubsWithoutPages()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromAuthorsWithoutPubs()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromEditorasWithoutPubs()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromGenerosWithoutPubs()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromFormatosWithoutPubs()).ReturnsAsync(0);
            repo.Setup(r => r.GetCountFromLocaisArquivoWithoutPubs()).ReturnsAsync(0);
            repo.Setup(r => r.GetTitulosDuplicados()).ReturnsAsync((List<StatRecord>?)null);

            var controller = Build(repo);
            var result = await controller.GetHealth();

            var ok = Assert.IsType<OkObjectResult>(result);
            var body = Assert.IsType<DashboardHealthDto>(ok.Value);
            Assert.Equal(0, body.TitulosDuplicados);
        }

        [Fact]
        public async Task GetHealth_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IStatsRepository>();
            repo.Setup(r => r.GetCountFromPubsWithoutCover()).ThrowsAsync(new InvalidOperationException("boom"));

            var controller = Build(repo);
            var result = await controller.GetHealth();

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, ((ObjectResult)result).StatusCode);
        }
    }
}