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


    public class PublishersController : ControllerBase
    {
        private readonly ILogger<PublishersController> _logger;
        private readonly IEditorasRepository _repoEditoras;
        private IMapper _mapper;

        public PublishersController(ILogger<PublishersController> logger,
            IEditorasRepository repoEditoras, IMapper mapper)
        {
            _logger = logger;
            _repoEditoras = repoEditoras;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var location = GetControllerActionNames();
            try
            {
                var editoras = await _repoEditoras.Query_Async();
                if (editoras.Any())
                {
                    _mapper.Map<IEnumerable<PublisherVM>>(editoras);
                    return Ok(editoras);
                }
                else
                    return Ok(Enumerable.Empty<PublisherVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetAllAsync)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublishersById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var editora = await _repoEditoras.Query_ById_Async(id);
                if (editora != null)
                {
                    _mapper.Map<PublisherVM>(editora);
                    return Ok(editora);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetPublishersById)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            var location = GetControllerActionNames();
            try
            {
                var editoras = await _repoEditoras.Listar_Async();
                if (editoras.Any())
                {
                    _mapper.Map<List<PublisherVM>>(editoras);
                    return Ok(editoras);
                }

                else
                    return Ok(Enumerable.Empty<PublisherVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("listar_editoraListar/{descricao}")]
        public async Task<IActionResult> Listar_Editora(string descricao)
        {
            var location = GetControllerActionNames();
            try
            {
                var editoras = await _repoEditoras.Listar_Editora(descricao);
                if (editoras.Any())
                {
                    _mapper.Map<List<PublisherVM>>(editoras);
                    return Ok(editoras);
                }

                else
                    return Ok(Enumerable.Empty<PublisherVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar_Editora)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetIdEditora/{description}")]
        public async Task<IActionResult> GetIdEditora(string description)
        {
            var location = GetControllerActionNames();
            try
            {
                int output = await _repoEditoras.GetIdByDescription_Async(description);
                if (output > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetIdEditora)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("EditorasSemLivros")]
        public async Task<IActionResult> EditorasSemLivros()
        {
            var location = GetControllerActionNames();
            try
            {
                    var editoras = await _repoEditoras.EditorasSemLivros();
                    if (editoras.Any())
                    {
                        _mapper.Map<List<PublisherVM>, List<AutorDTO>>(editoras);
                        return Ok(editoras);
                    }
                    else
                        return Ok(Enumerable.Empty<AutorDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (EditorasSemLivros)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("EditoraComLivros/{id:int}")]
        public async Task<IActionResult> EditoraComLivros(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                    bool output = await _repoEditoras.EditoraComLivros(id);
                    return Ok(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (EditoraComLivros)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("PublicacoesCatalogadas/{id:int}")]
        public IActionResult PublicacoesCatalogadas(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                var output = _repoEditoras.PublicacoesCatalogadas(id);
                if (output.Any())
                    return Ok(output);
                else
                    return Ok(Enumerable.Empty<PublisherVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Editoras/PublicacoesCatalogadas)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePublisher([FromBody] PublisherVM publisherVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (publisherVM is null)
                {
                    return BadRequest();
                }

                Editora editora = new();
                _mapper.Map(publisherVM, editora);

                bool DescriptionExist = await _repoEditoras.EditoraExiste(editora.Id);
                if (DescriptionExist)
                {
                    ModelState.AddModelError("Editora", "Nome da Editora já existe na base de dados");
                    return BadRequest(ModelState);
                }

                await _repoEditoras.Insert_Async(editora);
                int createdId = _repoEditoras.GetLastId();
                var createdObj = await GetPublishersById(createdId);
                return CreatedAtAction(nameof(GetPublishersById), new { id = createdId }, createdObj);
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
        public async Task<IActionResult> UpdatePublisher(int id, [FromBody] PublisherVM editoraDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (editoraDTO == null)
                {
                    _logger.LogWarning($"A Editora passada como paràmetro, é incorreta.");
                    return BadRequest($"A Editora passada como paràmetro, é incorreta.");
                }
                if (id != editoraDTO.Id)
                {
                    return BadRequest($"O id ({id}) passado como paràmetro está incorreto");
                }

                var publisherToUpdate = await _repoEditoras.Query_ById_Async(id);
                if (publisherToUpdate == null)
                {
                    return NotFound($"A Editora com o Id {id} não foi encontrada");
                }

                _mapper.Map(editoraDTO, publisherToUpdate);

                await _repoEditoras.Update_Async(publisherToUpdate);
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
        public async Task<ActionResult> DeletePublisher(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var publisherToDelete = await _repoEditoras.Query_ById_Async(id);
                if (publisherToDelete == null)
                {
                    return NotFound($"Editora com o Id {id} não encontrada");
                }

                await _repoEditoras.DeleteById_Async(id);
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
            return StatusCode(500, $"Something went wrong. Please contact the Administrator: {message}");
        }


    }
}
