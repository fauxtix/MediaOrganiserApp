using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _svc;
        private readonly IMapper _mapper;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(
            IConversationService svc,
            IMapper mapper,
            ILogger<ConversationsController> logger)
        {
            _svc = svc;
            _mapper = mapper;
            _logger = logger;
        }

        private string? TryUserId()
        {
            var value = Request.Headers["X-UserId"].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private bool TryIsAdmin()
        {
            var value = Request.Headers["X-IsAdmin"].ToString();
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetAll(CancellationToken ct)
        {
            var conversations = await _svc.GetAllAsync(TryUserId(), TryIsAdmin(), ct);
            return Ok(_mapper.Map<IEnumerable<ConversationDto>>(conversations));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDto>> Get(string id, CancellationToken ct)
        {
            var conv = await _svc.GetByIdAsync(id, TryUserId(), TryIsAdmin(), ct);
            return conv == null ? NotFound() : Ok(_mapper.Map<ConversationDto>(conv));
        }

        [HttpPost]
        public async Task<ActionResult<ConversationDto>> Create([FromBody] ConversationCreateDto request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest("Conversation payload is required.");

            try
            {
                var conv = _mapper.Map<Conversation>(request);
                var created = await _svc.UpsertAsync(conv, TryUserId(), TryIsAdmin(), ct);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, _mapper.Map<ConversationDto>(created));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, "An error occurred while creating the conversation.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] ConversationUpdateDto request, CancellationToken ct)
        {
            if (request == null) return BadRequest("Conversation payload is required.");
            if (string.IsNullOrWhiteSpace(request.Id) || id != request.Id) return BadRequest();

            try
            {
                var conv = _mapper.Map<Conversation>(request);
                await _svc.UpsertAsync(conv, TryUserId(), TryIsAdmin(), ct);
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
                await _svc.DeleteAsync(id, TryUserId(), TryIsAdmin(), ct);
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