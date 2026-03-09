using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    [ProducesResponseType(StatusCodes.Status200OK)]

    public class PdfController : ControllerBase
    {
        private readonly ILogger<PdfController> _logger;

        public PdfController(ILogger<PdfController> logger)
        {
            _logger = logger;
        }

        [HttpGet("displaypdf/{physicalPath}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DisplayPdf(string physicalPath)
        {
            var location = GetControllerActionNames();
            try
            {
                if (string.IsNullOrEmpty(physicalPath))
                {
                    return BadRequest();
                }
                if (!System.IO.File.Exists(physicalPath))
                {
                    return NotFound();
                }

                byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                using MemoryStream ms = new MemoryStream(pdfBytes);
                return new FileStreamResult(ms, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (EntradaExiste_BD)");
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
            return StatusCode(500, "Algo de errado ocorreu. Contacte o Administrador");
        }
    }
}
