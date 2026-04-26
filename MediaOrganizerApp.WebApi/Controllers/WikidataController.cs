using MediaOrganizerApp.Application.Interfaces.Services.WikiData;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WikidataController : ControllerBase
    {
        private readonly IWikidataService _service;
        private readonly ILogger<WikidataController> _logger;

        public WikidataController(IWikidataService service, ILogger<WikidataController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("books-by-author/{author}/{lang}")]
        public async Task<IActionResult> GetBooksByAuthor(string author, string lang = "en")
        {
            try
            {
                var result = await _service.GetBooksByAuthorAsync(author, lang);

                if (result == null || !result.Any())
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wikidata error");
                return StatusCode(500, "Error retrieving data");
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
