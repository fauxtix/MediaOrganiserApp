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

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiKey]

    public class PublicationsFormatsController : ControllerBase
    {
        private readonly ILogger<PublicationsFormatsController> _logger;
        private readonly IFormatosRepository _repoFormatos;
        private IMapper _mapper;

        public PublicationsFormatsController(ILogger<PublicationsFormatsController> logger,
            IFormatosRepository repoFormatos, IMapper mapper)
        {
            _logger = logger;
            _repoFormatos = repoFormatos;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var location = GetControllerActionNames();
            try
            {
                var formatos = await _repoFormatos.Query_Async();
                if (formatos.Any())
                {
                    _mapper.Map<IEnumerable<FormatoPublicacaoVM>>(formatos);
                    return Ok(formatos);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetAllAsync)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPublicationFormatsById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                // Adaptar (ver acima)
                var formato = await _repoFormatos.Query_ById_Async(id);
                if (formato != null)
                {
                    _mapper.Map<FormatoPublicacaoVM>(formato);
                    return Ok(formato);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetPublicationFormatsById)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            var location = GetControllerActionNames();
            try
            {
                var formatos = await _repoFormatos.Listar_Async();
                if (formatos.Any())
                {
                    _mapper.Map<List<FormatoPublicacaoVM>>(formatos);
                    return Ok(formatos);
                }
                else
                    return Ok(Enumerable.Empty<FormatoPublicacaoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar_Formato/{descricao}")]
        public async Task<IActionResult> Listar_Formato(string descricao)
        {
            var location = GetControllerActionNames();
            try
            {
                var formatos = await _repoFormatos.Listar_Formato(descricao);
                if (formatos.Any())
                {
                    _mapper.Map<List<FormatoPublicacaoVM>>(formatos);
                    return Ok(formatos);
                }
                else
                    return Ok(Enumerable.Empty<FormatoPublicacaoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetIdFormato/{description}")]
        public async Task<IActionResult> GetIdFormato(string description)
        {
            var location = GetControllerActionNames();
            try
            {
                int output = await _repoFormatos.GetIdByDescription_Async(description);
                if (output > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetIdFormato)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("PublicacoesCatalogadas/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult PublicacoesCatalogadas(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = _repoFormatos.PublicacoesCatalogadas(id);
                if (output.Any())
                    return Ok(output);
                else
                    return Ok(Enumerable.Empty<AutorDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Autores/PublicacoesCatalogadas)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePublicationFormat([FromBody] FormatoPublicacaoVM publicationFormatVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (publicationFormatVM is null)
                {
                    return BadRequest();
                }

                var formato = _mapper.Map<Formato>(publicationFormatVM);

                bool DescriptionExist = await _repoFormatos.FormatoExiste(formato.Id);
                if (DescriptionExist)
                {
                    ModelState.AddModelError("Formato de Publicação", "Nome do Formato já existe na base de dados");
                    return BadRequest(ModelState);
                }

                await _repoFormatos.Insert_Async(formato);
                int createdId = _repoFormatos.GetLastId();
                var createdObj = await GetPublicationFormatsById(createdId);
                return CreatedAtAction(nameof(GetPublicationFormatsById), new { id = createdId }, createdObj);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> UpdatePublisherFormat(int id, [FromBody] FormatoPublicacaoVM formatoDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (formatoDTO == null)
                {
                    _logger.LogWarning($"O Formato passado como paràmetro, é incorreto.");
                    return BadRequest($"O Formato passado como paràmetro, é incorreto.");
                }
                if (id != formatoDTO.Id)
                {
                    return BadRequest($"O id ({id}) passado como paràmetro está incorreto");
                }

                var frmtRec = await _repoFormatos.Query_ById_Async(id);
                if (frmtRec == null)
                {
                    return NotFound($"O Formato com o Id {id} não foi encontrada");
                }

                _mapper.Map(formatoDTO, frmtRec);

                await _repoFormatos.Update_Async(frmtRec);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePublisherFormat(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var publisherFormatToDelete = await _repoFormatos.Query_ById_Async(id);
                if (publisherFormatToDelete == null)
                {
                    return NotFound($"Formato com o Id {id} não encontrado");
                }

                bool formatWithPubs = await _repoFormatos.FormatoComLivros(id);
                if (formatWithPubs)
                {
                    return NotFound($"Formato com o Id {id} tem publicações. Verificar");
                }

                await _repoFormatos.DeleteById_Async(id);
                return NoContent();
            }
            catch (Exception ex)
            {
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
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
    }
}
