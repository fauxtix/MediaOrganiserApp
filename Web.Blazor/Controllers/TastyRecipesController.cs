using Dapper;
using MediaOrganizerApp.Application.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Blazor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TastyRecipesController : ControllerBase
    {
        private readonly ILogger<TastyRecipesController> _logger;
        public TastyRecipesController(ILogger<TastyRecipesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTags()
        {
            var location = GetControllerActionNames();
            _logger.LogInformation($"GetTags - {location}");
            using (var connection = ConnectionManager.GetConnection())
            {
                var query = "SELECT * FROM ResultTag";
                var output = await connection.QueryAsync<IEnumerable<RecipeTags>>(query);
                return Ok(output);
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
            return StatusCode(500, "Algo de errado ocorreu. Contacte o Administrador");
        }
    }
}
