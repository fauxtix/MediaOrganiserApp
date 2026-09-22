using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/ai-benchmark")]
    public class AiBenchmarkController : ControllerBase
    {
        private readonly IAIBenchmarkRepository _benchmark;
        private readonly ILogger<AiBenchmarkController> _logger;

        public AiBenchmarkController(
            IAIBenchmarkRepository benchmark,
            ILogger<AiBenchmarkController> logger)
        {
            _benchmark = benchmark;
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
        /// Aggregate summary of recorded AI responses (tokens, latency, models).
        /// Non-admin callers only see their own records.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AiBenchmarkSummaryDto>> GetSummary(CancellationToken ct)
        {
            try
            {
                var summary = await _benchmark.GetBenchmarkSummaryAsync(TryUserId(), TryIsAdmin(), ct);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resumo de benchmark de IA");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Algo de errado ocorreu ao obter o resumo de benchmark. Contacte o Administrador.");
            }
        }
    }
}