using MediaOrganizerApp.Domain.Entities.Author_Api_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleBooksController : ControllerBase
    {
        private string _googleBooksUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleBooksController> _logger;

        public GoogleBooksController(HttpClient httpClient, ILogger<GoogleBooksController> logger)
        {
            _googleBooksUrl = "https://www.googleapis.com/books/v1/volumes";
            _httpClient = httpClient;
            _logger = logger;
        }

        [HttpGet("GetBookInfo/book")]
        public async Task<IActionResult> GetBookInfo(string book)
        {
            var location = GetControllerActionNames();
            _googleBooksUrl = _googleBooksUrl + "?" + book;
            try
            {
                var output = await _httpClient.GetFromJsonAsync<GoogleBook>(_googleBooksUrl);
                if (output is not null)
                    return Ok(output);
                else
                    return NotFound($"Critério de pesquisa ({book})não devolveu qualquer resultado.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetBookInfo)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }
        [HttpGet("GetBookByAuthor/author")]
        public async Task<IActionResult> GetBookByAuthor(string author)
        {
            var location = GetControllerActionNames();
            _googleBooksUrl = $"{_googleBooksUrl } ?q = author:";
            try
            {
                var output = await _httpClient.GetFromJsonAsync<GoogleBook>(_googleBooksUrl);
                if (output is not null)
                    return Ok(output);
                else
                    return NotFound($"Critério de pesquisa ({author})não devolveu qualquer resultado.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetBookInfo)");
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
}
