using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AiController : ControllerBase
    {
        private readonly IAiAuthorProfileService _ai;
        private readonly IAiBookSummaryService _books;
        private readonly IAiBooksService _aiBooks;
        private readonly IAiMediaNotesService _media;
        private readonly IAiMediaMetadataService _mediaMetadata;
        private readonly IAiChatService _chat;
        private readonly ILogger<AiController> _logger;

        // Pasta externa para overrides
        private readonly string _overrideDir = @"C:\NewProjects\MediaOrganizer\MediaOrganizerApp\Prompts_Ollama";

        public AiController(
            IAiAuthorProfileService ai,
            IAiBookSummaryService books,
            IAiBooksService aiBooks,
            IAiMediaNotesService media,
            IAiMediaMetadataService mediaMetadata,
            ILogger<AiController> logger,
            IAiChatService chat)
        {
            _ai = ai;
            _books = books;
            _aiBooks = aiBooks;
            _media = media;
            _mediaMetadata = mediaMetadata;
            _logger = logger;

            Directory.CreateDirectory(_overrideDir); // garante que a pasta existe
            _chat = chat;
        }

        [HttpGet("prompts/readme")]
        public IActionResult GetReadme()
        {
            // Primeiro procura na pasta externa (overrides)
            var overrideReadme = Path.Combine(_overrideDir, "README.md");
            if (System.IO.File.Exists(overrideReadme))
                return Content(System.IO.File.ReadAllText(overrideReadme), "text/plain");

            // Depois procura na pasta default do projeto
            var defaultReadme = Path.Combine(AppContext.BaseDirectory, "Prompts", "README.md");
            if (System.IO.File.Exists(defaultReadme))
                return Content(System.IO.File.ReadAllText(defaultReadme), "text/plain");

            return NotFound();
        }
        // Lista todos os prompts (.txt ou .md) incluindo overrides
        [HttpGet("prompts")]
        public IActionResult GetPromptFiles()
        {
            var defaultDir = Path.Combine(AppContext.BaseDirectory, "Prompts");

            var defaultFiles = Directory.Exists(defaultDir)
                ? Directory.GetFiles(defaultDir, "*.*")
                    .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
                    .Select(Path.GetFileName)
                : Array.Empty<string>();

            var overrideFiles = Directory.Exists(_overrideDir)
                ? Directory.GetFiles(_overrideDir, "*.*")
                    .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
                    .Select(Path.GetFileName)
                : Array.Empty<string>();

            var allFiles = defaultFiles.Union(overrideFiles).ToList();
            return Ok(allFiles);
        }

        // Lê conteúdo de um prompt
        [HttpGet("prompts/{filename}")]
        public IActionResult GetPromptFile(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest("Filename is required.");

            var defaultDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
            var defaultFile = Path.Combine(defaultDir, filename);
            var overrideFile = Path.Combine(_overrideDir, filename);

            string content;

            if (System.IO.File.Exists(overrideFile))
                content = System.IO.File.ReadAllText(overrideFile);
            else if (System.IO.File.Exists(defaultFile))
                content = System.IO.File.ReadAllText(defaultFile);
            else
                return NotFound();

            return Content(content, "text/plain");
        }

        // Grava conteúdo de prompt — grava sempre na pasta externa
        [HttpPut("prompts/{filename}")]
        public IActionResult SavePromptFile(string filename, [FromBody] string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(content))
                    return BadRequest("Filename and content are required.");

                var overrideFile = Path.Combine(_overrideDir, filename);
                System.IO.File.WriteAllText(overrideFile, content);

                _logger.LogInformation("Saved override file: {File}", overrideFile);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {File}", filename);
                return BadRequest("Error saving file.");
            }
        }

        // Remove override e volta a usar default
        [HttpDelete("prompts/{filename}")]
        public IActionResult ResetPromptFile(string filename)
        {
            try
            {
                var overrideFile = Path.Combine(_overrideDir, filename);
                if (System.IO.File.Exists(overrideFile))
                    System.IO.File.Delete(overrideFile);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting file: {File}", filename);
                return BadRequest("Error resetting file.");
            }
        }

        // -----------------------------
        // Os restantes endpoints de AI permanecem iguais
        // -----------------------------

        [HttpPost("author-profile")]
        public async Task<ActionResult<AiAuthorProfileResponseDto>> AuthorProfile(
            [FromBody] AiAuthorProfileRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Field 'name' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var resp = await _ai.GetAuthorProfileAsync(request.Name, language, cancellationToken);
            return Ok(resp);
        }

        [HttpPost("book-summary")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummary(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var resp = await _books.GetBookSummaryAsync(request.Title, request.Author, language, cancellationToken);
            return Ok(resp);
        }

        [HttpPost("book-summary-only")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummaryOnly(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var resp = await _books.GetBookSummaryOnlyAsync(request.Title, request.Author, language, request.ConnectionId, cancellationToken);
            return Ok(resp);
        }

        [HttpGet("author-books")]
        public async Task<ActionResult<List<GoogleBookInfo>>> AuthorBooks(
            string author,
            string language = "pt",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(author))
                return BadRequest("author parameter is required");

            try
            {
                // Call AI books service without passing a maxResults parameter; the
                // service implementation will apply its own defaults and limits.
                var books = await _aiBooks.GetBooksByAuthorAsync(author, language, cancellationToken);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating author books for {Author}", author);
                return StatusCode(500, "Error generating author books");
            }
        }

        [HttpGet("flags")]
        public ActionResult<MediaOrganizerApp.Application.FeatureFlags> GetFlags()
        {
            return Ok(MediaOrganizerApp.WebApi.Services.FeatureFlagsStore.Get());
        }

        [HttpPut("flags")]
        public ActionResult SetFlags([FromBody] MediaOrganizerApp.Application.FeatureFlags flags)
        {
            MediaOrganizerApp.WebApi.Services.FeatureFlagsStore.Set(flags);
            return Ok();
        }

        [HttpPost("media-notes")]
        public async Task<ActionResult<AiMediaNotesResponseDto>> MediaNotes(
            [FromBody] AiMediaNotesRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

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

        [HttpPost("media-metadata")]
        public async Task<ActionResult<AiMediaMetadataResponseDto>> MediaMetadata(
            [FromBody] AiMediaMetadataRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var resp = await _mediaMetadata.GetMediaMetadataAsync(request.Artist, request.Title, language, request.ConnectionId, cancellationToken);
            return Ok(resp);
        }

        [HttpPost("media-edition-year")]
        public async Task<ActionResult<string>> MediaEditionYear(
            [FromBody] MediaEditionYearRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var year = await _mediaMetadata.GetMediaEditionYearAsync(request.Artist, request.Title, language, request.ConnectionId, cancellationToken);
            return Ok(string.IsNullOrWhiteSpace(year) ? "N/A" : year);
        }

        [HttpPost("chat")]
        public async Task Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            Response.ContentType = "text/plain";

            // If Language present, insert a system message so the LLM replies in the requested language.
            if (!string.IsNullOrWhiteSpace(request?.Language))
            {
                request.Messages.Insert(0, new ChatMessageDto
                {
                    Role = "system",
                    Content = $"Please respond in {request.Language}. Keep answers concise and avoid repetition."
                });
            }

            try
            {
                await foreach (var chunk in _chat.StreamChatAsync(
                    request.Messages,
                    request.ConnectionId,
                    cancellationToken))
                {
                    await Response.WriteAsync(chunk, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in Chat endpoint");
                Response.StatusCode = 500;
                try
                {
                    await Response.WriteAsync("Internal server error: " + ex.Message, cancellationToken);
                }
                catch
                {
                    // swallow
                }
            }
        }
    }
}