using Microsoft.AspNetCore.Mvc;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _svc;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(IConversationService svc, ILogger<ConversationsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetAll(CancellationToken ct) =>
            Ok(await _svc.GetAllAsync(ct));

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> Get(string id, CancellationToken ct)
        {
            var conv = await _svc.GetByIdAsync(id, ct);
            return conv == null ? NotFound() : Ok(conv);
        }

        [HttpPost]
        public async Task<ActionResult<Conversation>> Create([FromBody] Conversation conv, CancellationToken ct)
        {
            if (conv == null)
                return BadRequest("Conversation payload is required.");

            try
            {
                var created = await _svc.UpsertAsync(conv, ct);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, "An error occurred while creating the conversation.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] Conversation conv, CancellationToken ct)
        {
            if (conv == null) return BadRequest("Conversation payload is required.");
            if (id != conv.Id) return BadRequest();

            try
            {
                await _svc.UpsertAsync(conv, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation {ConversationId}", id);
                return StatusCode(500, "An error occurred while updating the conversation.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id, CancellationToken ct)
        {
            try
            {
                await _svc.DeleteAsync(id, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
                return StatusCode(500, "An error occurred while deleting the conversation.");
            }
        }
    }
}
