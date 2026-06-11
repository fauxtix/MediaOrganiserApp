using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[ApiKey]

    public class StoragePlacesController : ControllerBase
    {
        private readonly ILogger<StoragePlacesController> _logger;
        private readonly ILocalArquivoRepository _repoLocaisArquivo;
        private IMapper _mapper;

        public StoragePlacesController(ILogger<StoragePlacesController> logger,
            ILocalArquivoRepository repoLocaisArquivo, IMapper mapper)
        {
            _logger = logger;
            _repoLocaisArquivo = repoLocaisArquivo;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            var location = GetControllerActionNames();
            try
            {
                var locais = await _repoLocaisArquivo.GetAllAsync();
                if (locais.Any())
                {
                    _mapper.Map<IEnumerable<LocalArquivoVM>>(locais);
                    return Ok(locais);
                }
                else
                    return Ok(Enumerable.Empty<LocalArquivoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (LocaisArquivo / GetAllAsync)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStoragePlaceById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var local = await _repoLocaisArquivo.Query_ById_Async(id);
                if (local != null)
                {
                    _mapper.Map<LocalArquivoVM>(local);
                    return Ok(local);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetStoragePlaceById)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar")]
        public async Task<IActionResult> Listar()
        {
            var location = GetControllerActionNames();
            try
            {
                var locais = await _repoLocaisArquivo.Listar_Async();
                if (locais.Any())
                {
                    _mapper.Map<List<LocalArquivoVM>>(locais);
                    return Ok(locais);
                }

                else
                    return Ok(Enumerable.Empty<LocalArquivoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("Listar_LocalArquivo/{descricao}")]
        public async Task<IActionResult> Listar_LocalArquivo(string descricao)
        {
            var location = GetControllerActionNames();
            try
            {
                var locais = await _repoLocaisArquivo.Listar_LocalArquivo(descricao);
                if (locais.Any())
                {
                    _mapper.Map<List<LocalArquivoVM>>(locais);
                    return Ok(locais);
                }

                else
                    return Ok(Enumerable.Empty<LocalArquivoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar_LocalArquivo)");
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
                var output = _repoLocaisArquivo.PublicacoesCatalogadas(id).Result;
                if (output.Any())
                    return Ok(output);
                else
                    return Ok(Enumerable.Empty<LocalArquivoVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Locais de Arquivo/PublicacoesCatalogadas)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetIdLocalArquivo/{description}")]
        public async Task<IActionResult> GetIdLocalArquivo(string description)
        {
            var location = GetControllerActionNames();
            try
            {
                int output = await _repoLocaisArquivo.GetIdByDescription_Async(description);
                if (output > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetIdLocalArquivo)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStoragePlace([FromBody] LocalArquivoVM localArquivoVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (localArquivoVM is null)
                {
                    return BadRequest();
                }

                 var local = _mapper.Map<LocalArquivo>(localArquivoVM);

                // !!! não faz sentido pesquisar por ID, uma vez que vamos criar um registo
                // ver como está implementado no controlador 'Média', onde se pesquisa por Título
                // Nesta e noutras entidades como esta, deveria pesquisar-se por 'Descrição
                // O próprio nome da variável, indica que a descrição não existe (...)
                bool DescriptionExist = await _repoLocaisArquivo.LocalArquivoExiste(local.Id);
                if (DescriptionExist)
                {
                    ModelState.AddModelError("Local de Arquivo", "Nome do Local de Arquivo já existe na base de dados");
                    return BadRequest(ModelState);
                }

                await _repoLocaisArquivo.Insert_Async(local);
                int createdId = _repoLocaisArquivo.GetLastId();
                var createdObj = await GetStoragePlaceById(createdId);
                return CreatedAtAction(nameof(GetStoragePlaceById), new { id = createdId }, createdObj);
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
        public async Task<IActionResult> UpdateStoragePlace(int id, [FromBody] LocalArquivoVM localArquivoVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (localArquivoVM == null)
                {
                    _logger.LogWarning($"O Local de Arquivo passado como paràmetro, é incorreto.");
                    return BadRequest($"O Local de Arquivo é incorreto.");
                }

                if (id != localArquivoVM.Id)
                {
                    return BadRequest($"O id ({id}) passado como paràmetro está incorreto");
                }

                var locRec = await _repoLocaisArquivo.Query_ById_Async(id);
                if (locRec == null)
                {
                    return NotFound($"O Local de Arquivo com o Id {id} não foi encontrado");
                }

                _mapper.Map(localArquivoVM, locRec);

                await _repoLocaisArquivo.Update_Async(locRec);
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

        public async Task<ActionResult> DeleteStoragePlace(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var recToDelete = await _repoLocaisArquivo.Query_ById_Async(id);
                if (recToDelete == null)
                {
                    return NotFound($"Local de Arquivo com o Id {id} não encontrado");
                }

                await _repoLocaisArquivo.DeleteById_Async(id);
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

