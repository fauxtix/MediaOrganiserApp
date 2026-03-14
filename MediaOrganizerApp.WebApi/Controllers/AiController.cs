using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System;

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
    private readonly IAiBookSummaryService _books;
    private readonly IAiMediaNotesService _media;
    private readonly IAiMediaMetadataService _mediaMetadata;

    
    public AiController(IAiAuthorProfileService ai, IAiBookSummaryService books, IAiMediaNotesService media, IAiMediaMetadataService mediaMetadata)
    {
        _ai = ai;
        _books = books;
        _media = media;
        _mediaMetadata = mediaMetadata;
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

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var resp = await _ai.GetAuthorProfileAsync(request.Name, language, cancellationToken);
        return Ok(resp);
    }

    /// <summary>
    /// Generates a book summary/description using AI. Intended as fallback only when external metadata APIs lack a description.
    /// </summary>
    [HttpPost("book-summary")]
    public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummary(
        [FromBody] AiBookSummaryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Field 'title' is required.");

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var resp = await _books.GetBookSummaryAsync(request.Title, request.Author, language, cancellationToken);
        return Ok(resp);
    }

    /// <summary>
    /// Generates ONLY a book summary/description using AI (lighter prompt). Intended when external APIs provided metadata but no description.
    /// </summary>
    [HttpPost("book-summary-only")]
    public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummaryOnly(
        [FromBody] AiBookSummaryRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Field 'title' is required.");

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var resp = await _books.GetBookSummaryOnlyAsync(request.Title, request.Author, language, cancellationToken);
        return Ok(resp);
    }

    /// <summary>
    /// Generates media "notes" (liner notes / awards / highlights). Intended as fallback when external metadata APIs return empty notes.
    /// </summary>
    [HttpPost("media-notes")]
    public async Task<ActionResult<AiMediaNotesResponseDto>> MediaNotes(
        [FromBody] AiMediaNotesRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Field 'title' is required.");

        if (string.IsNullOrWhiteSpace(request.Artist))
            return BadRequest("Field 'artist' is required.");

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var resp = await _media.GetMediaNotesAsync(request.Artist, request.Title, language, cancellationToken);
        return Ok(resp);
    }

    /// <summary>
    /// Generates media metadata (DVD/movie fields and/or album fields) using AI. Intended as fallback when external metadata APIs return no result.
    /// </summary>
    [HttpPost("media-metadata")]
    public async Task<ActionResult<AiMediaMetadataResponseDto>> MediaMetadata(
        [FromBody] AiMediaMetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Field 'title' is required.");

        if (string.IsNullOrWhiteSpace(request.Artist))
            return BadRequest("Field 'artist' is required.");

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var resp = await _mediaMetadata.GetMediaMetadataAsync(request.Artist, request.Title, language, cancellationToken);
        return Ok(resp);
    }
}
