using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/ai-usage")]
    public class AiUsageController : ControllerBase
    {
        private readonly IAiUsageLogRepository _usage;
        private readonly ILogger<AiUsageController> _logger;

        public AiUsageController(
            IAiUsageLogRepository usage,
            ILogger<AiUsageController> logger)
        {
            _usage = usage;
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

        /// <summary>
        /// Aggregate summary of internal (non-chat) AI usage by operation and
        /// provider. Non-admin callers only see their own records.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AiUsageSummaryDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var summary = await _usage.GetSummaryAsync(TryUserId(), TryIsAdmin(), ct);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resumo de uso interno de IA");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Algo de errado ocorreu ao obter o resumo de uso de IA. Contacte o Administrador.");
            }
        }
    }
}