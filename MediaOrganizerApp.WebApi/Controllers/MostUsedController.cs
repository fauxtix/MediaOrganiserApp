using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiKey]

    public class MostUsedController : ControllerBase
    {
        private readonly ILogger<MostUsedController> _logger;
        private readonly IMostUsedRepository _repoMostUsed;

        public MostUsedController(ILogger<MostUsedController> logger, IMostUsedRepository repoMostUsed)
        {
            _repoMostUsed = repoMostUsed;
            _logger = logger;
        }

        [HttpPost("getmostused/{query}")]
        public async Task<IActionResult> GetMostUsed(QueryMostUsed query)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoMostUsed.GetOpcaoMaisUsada(query.Campo, query.Tabela, query.Chave, query.Descricao, query.TabelaAusar);
                if (output.Any())
                {
                    return Content(output);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetMostUsed)");
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
