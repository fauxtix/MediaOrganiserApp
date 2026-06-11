using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiKey]

    public class CategoriasController : ControllerBase
    {
        private readonly ILogger<CategoriasController> _logger;
        private readonly ICategoriasRepository _repoCategorias;
        private readonly ITipoCategoriaRepository _repoTipoCategorias;
        private IMapper _mapper;

        public CategoriasController(ILogger<CategoriasController> logger,
            ICategoriasRepository repoCategorias, IMapper mapper, ITipoCategoriaRepository repoTipoCategorias)
        {
            _logger = logger;
            _repoCategorias = repoCategorias;
            _mapper = mapper;
            _repoTipoCategorias = repoTipoCategorias;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryVM categoriaVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (categoriaVM is null)
                {
                    return BadRequest();
                }

                var categoria = _mapper.Map<Categoria>(categoriaVM);

                bool DescriptionExist = await _repoCategorias.RecInUse_ByDescription_Async(categoria.TipoCategoria, categoria.Descricao);
                if (DescriptionExist)
                {
                    ModelState.AddModelError("Categoria", "Nome da Categoria já existe na base de dados");
                    return BadRequest(ModelState);
                }

               var insertRetCode =await _repoCategorias.Insert_Async(categoria);
                if (insertRetCode == -1) // error
                {
                    return BadRequest("Erro ao criar Categoria. Verifique, p.f.");
                }

                int createdCategoryId = _repoCategorias.GetLastId();
                var createdCategory = await GetCategoriesById(createdCategoryId);
                return CreatedAtAction(nameof(GetCategoriesById), new { id = createdCategoryId }, createdCategory);
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
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryVM categoryDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (categoryDTO is null)
                {
                    _logger.LogWarning($"A Categoria passada como paràmetro é incorreta.");
                    return BadRequest($"A Categoria passada como paràmetro é incorreta.");
                }
                if (id != categoryDTO.Id)
                {
                    return BadRequest($"O id ({id}) passado como paràmetro está incorreto");
                }

                var categoryToUpdate = await _repoCategorias.Query_ById_Async(id);
                if (categoryToUpdate is null)
                {
                    return NotFound($"A Categoria com o Id {id} não foi encontrada");
                }

                _mapper.Map(categoryDTO, categoryToUpdate);
                await _repoCategorias.Update_Async(categoryToUpdate);
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult> DeleteCategory(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var categoryToDelete = await _repoCategorias.Query_ById_Async(id);
                if (categoryToDelete is null)
                {
                    return NotFound($"Categoria com o Id {id} não encontrada");
                }

                await _repoCategorias.DeleteById_Async(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetAllAsync/{tipo:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(int tipo)
        {
            var location = GetControllerActionNames();
            try
            {
                var categorias = await _repoCategorias.GetCategorias_ExtendedAsync(tipo);
                if (categorias.Any())
                {
                    return Ok(categorias);
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
        [HttpGet("AllAsync/{tipo:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategoriesAsync(int tipo)
        {
            var location = GetControllerActionNames();
            try
            {
                var categorias = await _repoCategorias.GetCategoriasAsync(tipo);
                if (categorias.Any())
                {
                    return Ok(categorias);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (AllAsync)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var categorias = await _repoCategorias.Query_ById_Async(id);
                if (categorias != null)
                {
                    _mapper.Map<CategoryVM>(categorias);
                    return Ok(categorias);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetCategoriesById)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetIdCategoria/{description}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIdCategoria(string description)
        {
            var location = GetControllerActionNames();
            try
            {
                int output = await _repoCategorias.GetIdByDescription_Async(description);
                // este método foi alterado, para permitir criação dinâmica
                // de Categoria (opção de Upload); nos serviços que a usam
                // deverá ser testado valor de retorno, quando este é 0 (zero)
                return Ok(output);
                //if (output > 0)
                //    return Ok(output);
                //else
                //    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetIdCategoria)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("MediaCatalogados/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult MediaCatalogados(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = _repoCategorias.MediaCatalogados(id).Result;
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (MediaCatalogados)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetCategoriesWithPublications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoriesWithPublications()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoCategorias.GetCategoriesWithPublications();
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetCategoriesWithPublications)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetCategoriesWithMediaEntries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task< IActionResult> GetCategoriesWithMediaEntries()
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoCategorias.GetCategoriesWithMediaEntries();
                if (output.Any())
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetCategoriesWithMediaListEntries)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("PublicacoesCatalogadas/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult PublicacoesCatalogadas(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = _repoCategorias.PublicacoesCatalogadas(id).Result;
                if (output.Any())
                    return Ok(output);
                else
                    return Ok(Enumerable.Empty<PublicacoesVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Autores/PublicacoesCatalogadas)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("CategoriaComRegistos/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> CategoriaComRegistos(int id, bool fromBooks)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoCategorias.CategoriaComRegistos(id, fromBooks);
                return Ok(output);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (CategoriaComRegistos)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar/{tipo:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Listar(int tipo)
        {
            var location = GetControllerActionNames();
            try
            {
                var categorias = await _repoCategorias.Listar(tipo == 1 ? true : false);
                if (categorias.Any())
                {
                    _mapper.Map<List<CategoryVM>>(categorias);
                    return Ok(categorias);
                }
                else
                    return Ok(Enumerable.Empty<CategoryVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("Listar_Categoria/{descricao}/{pubormedia}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Listar_Categoria(string descricao, bool pubOrMedia)
        {
            var location = GetControllerActionNames();
            try
            {
                var categorias = await _repoCategorias.Listar_Categoria(descricao, (pubOrMedia == true? 1:2));
                if (categorias.Any())
                {
                    _mapper.Map<List<CategoryVM>>(categorias);
                    return Ok(categorias);
                }
                else
                    return Ok(Enumerable.Empty<CategoryVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (Listar)");
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
            return StatusCode(500, "Algo de errado ocorreu na aplicação. Contacte o Administrador, por favor");
        }
    }
}
