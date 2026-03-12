using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Controllers;

/// <summary>
/// AI endpoints (server-side).
/// Protected by ApiKeyMiddleware (request header "ApiKey").
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AiController : ControllerBase
{
    private readonly IAiAuthorProfileService _ai;

    
    public AiController(IAiAuthorProfileService ai)
    {
        _ai = ai;
    }

    /// <summary>
    /// Generates an author profile/bio using AI.
    /// IMPORTANT: Intended as fallback only when external metadata APIs fail (for now ;) )>
    /// </summary>
    [HttpPost("author-profile")]
    public async Task<ActionResult<AiAuthorProfileResponseDto>> AuthorProfile(
        [FromBody] AiAuthorProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Field 'name' is required.");

        var resp = await _ai.GetAuthorProfileAsync(request.Name, cancellationToken);
        return Ok(resp);
    }
}
