using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]

    public class AIModelsController : ControllerBase
    {
        private readonly ILogger<AIModelsController> _logger;
        private readonly IOllamaModelsService _ollama;

        public AIModelsController(ILogger<AIModelsController> logger, IOllamaModelsService ollama)
        {
            _logger = logger;
            _ollama = ollama;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                var models = await _ollama.GetModelsAsync(cancellationToken);
                return Ok(models ?? Enumerable.Empty<AIModelVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (AIModels / GetAllAsync)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAIModelById(int id, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                var model = await ResolveByNameByIndexAsync(id, cancellationToken);
                if (model != null)
                    return Ok(model);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (GetAIModelById)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("modelname/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelName(int id, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                var model = await ResolveByNameByIndexAsync(id, cancellationToken);
                if (model != null)
                    return Ok(model.Descricao);
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
        public async Task<IActionResult> AIModelExiste(int id, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                var model = await ResolveByNameByIndexAsync(id, cancellationToken);
                return Ok(model != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no Api (AIModelExiste)");
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpGet("GetIdAIModel/{description}")]
        public async Task<IActionResult> GetIdAIModel(string description, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                var models = (await _ollama.GetModelsAsync(cancellationToken))?.ToList() ?? new List<AIModelVM>();
                var model = models.FirstOrDefault(m =>
                    string.Equals(m.Descricao, description, StringComparison.OrdinalIgnoreCase));
                if (model != null)
                    return Ok(model.Id);
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
        public async Task<IActionResult> CreateAIModel([FromBody] AIModelVM aiModelVM, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                if (aiModelVM is null || string.IsNullOrWhiteSpace(aiModelVM.Descricao))
                {
                    return BadRequest("A descrição (nome) do modelo é obrigatória.");
                }

                var pulled = await _ollama.PullModelAsync(aiModelVM.Descricao.Trim(), cancellationToken);
                if (!pulled)
                    return BadRequest($"Não foi possível descarregar o modelo '{aiModelVM.Descricao}' do Ollama.");

                var model = (await _ollama.GetModelsAsync(cancellationToken))?.FirstOrDefault(m =>
                    string.Equals(m.Descricao, aiModelVM.Descricao.Trim(), StringComparison.OrdinalIgnoreCase));

                if (model == null)
                    return CreatedAtAction(nameof(GetAllAsync), new { });

                return CreatedAtAction(nameof(GetAIModelById), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateStoragePlace(int id, [FromBody] AIModelVM? aiModelVM)
        {
            // Os modelos do Ollama não se renomeiam/editem. A gestão é feita via pull (novo) e delete (apagar).
            return BadRequest("A edição de modelos não é suportada. Use 'novo' para descarregar e 'apagar' para remover um modelo do Ollama.");
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteStoragePlace(int id, CancellationToken cancellationToken)
        {
            var location = GetControllerActionNames();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var model = await ResolveByNameByIndexAsync(id, cancellationToken);
                if (model == null)
                {
                    return NotFound($"Modelo com o Id {id} não foi encontrado.");
                }

                var deleted = await _ollama.DeleteModelAsync(model.Descricao, cancellationToken);
                if (!deleted)
                    return BadRequest($"Não foi possível remover o modelo '{model.Descricao}' do Ollama.");

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Os modelos do Ollama não têm um identificador numérico persistido; o Id corresponde
        /// à posição (1-based) na lista devolvida por /api/tags. Este método resolve o Id para o modelo.
        /// </summary>
        private async Task<AIModelVM?> ResolveByNameByIndexAsync(int id, CancellationToken cancellationToken)
        {
            var models = (await _ollama.GetModelsAsync(cancellationToken))?.ToList() ?? new List<AIModelVM>();
            return models.FirstOrDefault(m => m.Id == id);
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
