using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/chat-feedback")]
    public class ChatFeedbackController : ControllerBase
    {
        private static readonly HashSet<string> AllowedInterfaces =
            new(StringComparer.OrdinalIgnoreCase) { "openrouter", "ollama" };

        private readonly IChatFeedbackRepository _feedback;
        private readonly ILogger<ChatFeedbackController> _logger;

        public ChatFeedbackController(
            IChatFeedbackRepository feedback,
            ILogger<ChatFeedbackController> logger)
        {
            _feedback = feedback;
            _logger = logger;
        }

        private string? TryUserId()
        {
            var value = Request.Headers["X-UserId"].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// Records (or removes, Rating = 0) a like/dislike on an assistant response.
        /// Requires X-UserId; rows are always bound to that user (data isolation).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upsert([FromBody] ChatFeedbackDto body, CancellationToken ct)
        {
            var userId = TryUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Falta o header X-UserId.");

            if (body is null || !AllowedInterfaces.Contains(body.Interface))
                return BadRequest("Interface inválida. Valores válidos: openrouter, ollama.");

            if (body.Rating != 1 && body.Rating != -1 && body.Rating != 0)
                return BadRequest("Rating inválido. Valores válidos: 1, -1, 0.");

            if (string.IsNullOrWhiteSpace(body.ResponseText))
                return BadRequest("ResponseText não pode estar vazio.");

            try
            {
                body.UserId = userId;

                if (string.IsNullOrWhiteSpace(body.ConversationId))
                    body.ConversationId = null;

                await _feedback.UpsertAsync(body, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registar avaliação de chat.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Algo de errado ocorreu ao guardar a avaliação. Contacte o Administrador.");
            }
        }

        /// <summary>
        /// Returns the ratings of a conversation for the current user.
        /// </summary>
        [HttpGet("by-conversation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ChatFeedbackDto>>> GetByConversation(
            [FromQuery] string @interface,
            [FromQuery] string? conversationId,
            CancellationToken ct)
        {
            var userId = TryUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("Falta o header X-UserId.");

            if (!AllowedInterfaces.Contains(@interface))
                return BadRequest("Interface inválida. Valores válidos: openrouter, ollama.");

            try
            {
                var list = await _feedback.GetByConversationAsync(userId, @interface, conversationId, ct);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter avaliações de chat.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Algo de errado ocorreu ao obter as avaliações. Contacte o Administrador.");
            }
        }
    }
}