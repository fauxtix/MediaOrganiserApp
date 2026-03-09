using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.QueryEnums;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiKey]

    [ProducesResponseType(StatusCodes.Status200OK)]

    public class DashboardController : ControllerBase
    {
        private readonly ILogger<AuthorsController> _logger;
        private IMapper _mapper;

        private readonly IAutoresRepository _repoAutores;
        private readonly IEditorasRepository _repoEditoras;
        private readonly IFormatosRepository _repoFormatos;
        private readonly IFormatos_MediaRepository _repoFormatos_Media;
        private readonly ICategoriasRepository _repoCategorias;
        private readonly IMediaRepository _repoMedia;
        private readonly ILocalArquivoRepository _repoLocaisArquivo;

        public DashboardController(ILogger<AuthorsController> logger, IMapper mapper,
            IAutoresRepository repoAutores, IEditorasRepository repoEditoras,
            IFormatosRepository repoFormatos, IFormatos_MediaRepository repoFormatos_Media,
            ICategoriasRepository repoCategorias, ILocalArquivoRepository repoLocaisArquivo, IMediaRepository repoMedia)
        {
            _logger = logger;
            _mapper = mapper;

            _repoAutores = repoAutores;
            _repoEditoras = repoEditoras;
            _repoFormatos = repoFormatos;
            _repoFormatos_Media = repoFormatos_Media;
            _repoCategorias = repoCategorias;
            _repoLocaisArquivo = repoLocaisArquivo;
            _repoMedia = repoMedia;
        }

        [HttpGet("{option}")]
        public async Task<IActionResult> GetEntitiesSummary(string option)
        {
            var location = GetControllerActionNames();
            try
            {
                QueryOptions opcao = (QueryOptions)Enum.Parse(typeof(QueryOptions), option);
                var entitySummary = await PerformCount(opcao);

                if (entitySummary.Any())
                {
                    return Ok(entitySummary);
                }
                else
                    return Ok(Enumerable.Empty<SummaryDataDTO>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no Api {location}");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private async Task<IEnumerable<SummaryDataDTO>> PerformCount(QueryOptions opcao)
        {
            IEnumerable<SummaryData> summaryData = null;
            switch (opcao)
            {
                case QueryOptions.Autor:
                    summaryData = await _repoAutores.GetCount();
                    break;
                case QueryOptions.Editora:
                    summaryData = await _repoEditoras.GetCount();
                    break;
                case QueryOptions.Genero:
                    summaryData = await _repoCategorias.GetCount();
                    break;
                case QueryOptions.Genero_Media:
                    summaryData = await _repoMedia.GetCount();
                    break;
                case QueryOptions.Formato:
                    summaryData = await _repoFormatos.GetCount();
                    break;
                case QueryOptions.Formato_Media:
                    summaryData = await _repoFormatos_Media.GetCount();
                    break;
                case QueryOptions.Arquivo:
                    summaryData = await _repoLocaisArquivo.GetCount();
                    break;
            }
            var result = _mapper.Map<IEnumerable<SummaryDataDTO>>(summaryData);
            return result;
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, $"Something went wrong. Please contact the Administrator: {message}");
        }
    }
}

