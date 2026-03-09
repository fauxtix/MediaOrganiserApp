using MediaOrganizerApp.WebApi.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    [ProducesResponseType(StatusCodes.Status200OK)]

    public class WikipediaController : ControllerBase
    {
        private readonly ILogger<WikipediaController> _logger;
        private readonly HttpClient _httpClient;
        public WikipediaController(HttpClient httpClient, ILogger<WikipediaController> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [HttpGet("GetWikiInfo/searchQuery")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWikiInfo(string searchQuery)
        {
            string WikiUrl = $"https://en.wikipedia.org/w/rest.php/v1/search/page?q={searchQuery}";
            var location = GetControllerActionNames();
            try
            {
                var output = await _httpClient.GetAsync(WikiUrl);
                if (output is not null)
                    return Ok(output);
                else
                    return NotFound($"Critério de pesquisa ({searchQuery})não devolveu qualquer resultado.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetWikiInfo)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
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

    public class WikiModel
    {
        public string id { get; set; }
        public string key { get; set; }
        public string title { get; set; }
        public string excerpt { get; set; }
        public string description { get; set; }
        public Thumbnail thumbnail { get; set; }
    }

    public class Thumbnail
    {
        public string mimetype { get; set; }
        public string size { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string duration { get; set; }
        public string url { get; set; }
    }
}
