using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Shared.DTOs;
using MediaOrganizerApp.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiKey]


    public class StatsController : ControllerBase
    {
        private readonly ILogger<StatsController> _logger;
        private readonly IStatsRepository _repoStats;
        private IMapper _mapper;

        public StatsController(IStatsRepository repoStats, ILogger<StatsController> logger,
            IMapper mapper)
        {
            _logger = logger;
            _repoStats = repoStats;
            _mapper = mapper;
        }

        [HttpGet("GetTitulosDuplicados")]
        public async Task<IActionResult> GetTitulosDuplicados()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoStats.GetTitulosDuplicados();
                if (output.Any())
                {
                    return Ok(output);
                }

                else
                    return Ok(new List<StatRecord>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetTitulosDuplicados)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetCountFromTableNotClassified/{tabela}/{campoGrupo}/{campoAux}/{stringToCheck}")]
        public async Task<IActionResult> GetCountFromTableNotClassified(string sTabela, string sCampoGrupo, string sCampoAux, string sToCheck)
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromTableNotClassified(sTabela, sCampoGrupo, sCampoAux, sToCheck);
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromTableNotClassified)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetCountFromPubsWithoutPages")]
        public async Task<IActionResult> GetCountFromPubsWithoutPages()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromPubsWithoutPages();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromPubsWithoutPages)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetCountFromPubsWithoutCover")]

        public async Task<IActionResult> GetCountFromPubsWithoutCover()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromPubsWithoutCover();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromPubsWithoutCover)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetCountFromAuthorsWithoutPubs")]

        public async Task<IActionResult> GetCountFromAuthorsWithoutPubs()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromAuthorsWithoutPubs();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromPubsWithoutCover)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetCountFromGenerosWithoutPubs")]
        public async Task<IActionResult> GetCountFromGenerosWithoutPubs()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromGenerosWithoutPubs();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromGenerosWithoutPubs)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetCountFromEditorasWithoutPubs")]
        public async Task<IActionResult> GetCountFromEditorasWithoutPubs()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromEditorasWithoutPubs();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromEditorasWithoutPubs)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetCountFromFormatosWithoutPubs")]
        public async Task<IActionResult> GetCountFromFormatosWithoutPubs()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromFormatosWithoutPubs();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromFormatosWithoutPubs)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetCountFromLocaisArquivoWithoutPubs")]
        public async Task<IActionResult> GetCountFromLocaisArquivoWithoutPubs()
        {
            var location = GetControllerActionNames();
            try
            {
                var count = await _repoStats.GetCountFromLocaisArquivoWithoutPubs();
                if (count > 0)
                {
                    return Ok(count);
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetCountFromLocaisArquivoWithoutPubs)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetStatEntities")]
        public async Task<IActionResult> GetStatEntities()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoStats.GetStatEntities();
                if (output.Any())
                {
                    return Ok(new List<StatRecord>());
                }

                else
                    return Ok(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api Stats (GetStatEntities)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
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
