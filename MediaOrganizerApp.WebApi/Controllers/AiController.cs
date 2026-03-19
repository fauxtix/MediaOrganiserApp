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
    private readonly ILogger<AiController> _logger;


    public AiController(IAiAuthorProfileService ai, IAiBookSummaryService books, IAiMediaNotesService media, IAiMediaMetadataService mediaMetadata, ILogger<AiController> logger)
    {
        _ai = ai;
        _books = books;
        _media = media;
        _mediaMetadata = mediaMetadata;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os arquivos .txt e .md da pasta Prompts.
    /// </summary>
    [HttpGet("prompts")]
    public IActionResult GetPromptFiles()
    {
        var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
        if (!Directory.Exists(promptsDir))
            return Ok(Array.Empty<string>());
        var files = Directory.GetFiles(promptsDir, "*.txt")
            .Concat(Directory.GetFiles(promptsDir, "*.md"))
            .Select(Path.GetFileName)
            .ToList();
        return Ok(files);
    }

    /// <summary>
    /// Retorna o conteúdo do README.md da pasta Prompts.
    /// </summary>
    [HttpGet("prompts/readme")]
    public IActionResult GetReadme()
    {
        var readmePath = Path.Combine(AppContext.BaseDirectory, "Prompts", "README.md");
        if (!System.IO.File.Exists(readmePath))
            return NotFound();
        var content = System.IO.File.ReadAllText(readmePath);
        return Content(content, "text/plain");
    }

    /// <summary>
    /// Retorna o conteúdo de um arquivo de prompt (.txt ou .md) na pasta Prompts.
    /// </summary>
    [HttpGet("prompts/{filename}")]
    public IActionResult GetPromptFile(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return BadRequest("Filename is required.");
        var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
        var filePath = Path.Combine(promptsDir, filename);
        if (!System.IO.File.Exists(filePath))
            return NotFound();
        var content = System.IO.File.ReadAllText(filePath);
        return Content(content, "text/plain");
    }


    /// <summary>
    /// Salva o conteúdo de um arquivo de prompt (.txt ou .md) na pasta Prompts.
    /// </summary>
    [HttpPut("prompts/{filename}")]
    public IActionResult SavePromptFile(string filename, [FromBody] string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(content))
                return BadRequest("Filename and content are required.");
            var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
            if (!Directory.Exists(promptsDir))
                Directory.CreateDirectory(promptsDir);
            var filePath = Path.Combine(promptsDir, filename);
            System.IO.File.WriteAllText(filePath, content);
            _logger.LogInformation("Saving file: {FileName} with content length: {ContentLength}", filename, content.Length);
            _logger.LogInformation("File path: {FilePath}", filePath);
            return Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", filename);
            return BadRequest("Error saving file.");
        }
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
        try
        {
            var resp = await _media.GetMediaNotesAsync(request.Artist, request.Title, language, request.ConnectionId, cancellationToken);
            return Ok(resp);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating media notes for '{Artist}' - '{Title}'", request.Artist, request.Title);
            return BadRequest("Error generating media notes.");
        }
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
        var resp = await _mediaMetadata.GetMediaMetadataAsync(request.Artist, request.Title, language, request.ConnectionId, cancellationToken);
        return Ok(resp);
    }

    /// <summary>
    /// Generates only the edition year (Ano de Edição) for a media item using AI.
    /// </summary>
    [HttpPost("media-edition-year")]
    public async Task<ActionResult<string>> MediaEditionYear(
        [FromBody] MediaEditionYearRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Field 'title' is required.");
        if (string.IsNullOrWhiteSpace(request.Artist))
            return BadRequest("Field 'artist' is required.");

        var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
        var year = await _mediaMetadata.GetMediaEditionYearAsync(
            request.Artist,
            request.Title,
            language,
            request.ConnectionId,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(year))
            return Ok("N/A");
        return Ok(year);
    }
}
