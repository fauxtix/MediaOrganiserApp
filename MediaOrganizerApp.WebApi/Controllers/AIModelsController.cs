using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]

    public class AIModelsController : ControllerBase
    {
        private readonly ILogger<AIModelsController> _logger;
        private readonly IAIModelsRepository _repoAIModels;
        private IMapper _mapper;

        public AIModelsController(ILogger<AIModelsController> logger,
            IAIModelsRepository repoAIModels, 
            IMapper mapper)
        {
            _logger = logger;
            _repoAIModels = repoAIModels;
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
                var aiNames = await _repoAIModels.Query_Async();
                if (aiNames.Any())
                {
                    _mapper.Map<IEnumerable<AIModelVM>>(aiNames);
                    return Ok(aiNames);
                }
                else
                    return Ok(Enumerable.Empty<AIModelVM>());
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
        public async Task<IActionResult> GetAIModelById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var model = await _repoAIModels.Query_ById_Async(id);
                if (model != null)
                {
                    _mapper.Map<AIModelVM>(model);
                    return Ok(model);
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

        [HttpGet("modelname/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelName(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var model = await _repoAIModels.Query_ById_Async(id);
                if (model != null)
                {
                    var modelName = await _repoAIModels.GetModelName(id);
                    return Ok(modelName);
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetModelName)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("modelexist/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AIModelExiste(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var modelExist = await _repoAIModels.RecInUse_Async(id);
                    return Ok(modelExist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (AIModelExiste)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpGet("GetIdAIModel/{description}")]
        public async Task<IActionResult> GetIdAIModel(string description)
        {
            var location = GetControllerActionNames();
            try
            {
                int output = await _repoAIModels.GetIdByDescription_Async(description);
                if (output > 0)
                    return Ok(output);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetIdAIModel)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAIModel([FromBody] AIModelVM aiModelVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (aiModelVM is null)
                {
                    return BadRequest();
                }

                var model   = _mapper.Map < AiModel>(aiModelVM);

                bool DescriptionExist = await _repoAIModels.AIModelExist(model.Id);
                if (DescriptionExist)
                {
                    ModelState.AddModelError("AIModel", "Nome do AIModel já existe na base de dados");
                    return BadRequest(ModelState);
                }

                await _repoAIModels.Insert_Async(model);
                int createdId = _repoAIModels.GetLastId();
                var createdObj =  await _repoAIModels.Query_ById_Async(createdId);
                return CreatedAtAction(nameof(GetAIModelById), new { id = createdId }, createdObj);
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
        public async Task<IActionResult> UpdateStoragePlace(int id, [FromBody] AIModelVM aiModelVM)
        {
            var location = GetControllerActionNames();
            try
            {
                if (aiModelVM == null)
                {
                    _logger.LogWarning($"O AIModel passado como paràmetro, é incorreto.");
                    return BadRequest($"O AIModel é incorreto.");
                }

                if (id != aiModelVM.Id)
                {
                    return BadRequest($"O id ({id}) passado como paràmetro está incorreto");
                }

                var locRec = await _repoAIModels.Query_ById_Async(id);
                if (locRec == null)
                {
                    return NotFound($"O AIModel com o Id {id} não foi encontrado");
                }

                _mapper.Map(aiModelVM, locRec);

                await _repoAIModels.Update_Async(locRec);
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

                var recToDelete = await _repoAIModels.Query_ById_Async(id);
                if (recToDelete == null)
                {
                    return NotFound($"AIModel com o Id {id} não encontrado");
                }

                await _repoAIModels.DeleteById_Async(id);
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
