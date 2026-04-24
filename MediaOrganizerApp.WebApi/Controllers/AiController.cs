using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
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
        private readonly IPromptTemplateProvider _promptProvider;
        private readonly ILogger<AiController> _logger;
        // Translate is implemented as a separate service resolved per-request to avoid changing constructor signature

        public AiController(
            IAiAuthorProfileService ai,
            IAiBookSummaryService books,
            IAiBooksService aiBooks,
            IAiMediaNotesService media,
            IAiMediaMetadataService mediaMetadata,
            ILogger<AiController> logger,
            IAiChatService chat,
            IPromptTemplateProvider promptProvider)
        {
            _ai = ai;
            _books = books;
            _aiBooks = aiBooks;
            _media = media;
            _mediaMetadata = mediaMetadata;
            _logger = logger;
            _chat = chat;
            _promptProvider = promptProvider;
        }

        private static string MapLanguageCodeToName(string? language)
        {
            if (string.IsNullOrWhiteSpace(language)) return language ?? string.Empty;
            var code = language.Split(new[] { '-', '_' })[0].ToLowerInvariant();
            return code switch
            {
                "pt" => "Portuguese",
                "es" => "Spanish",
                "fr" => "French",
                "en" => "English",
                _ => language
            };
        }

        // Note: Hub-based background chat start/cancel endpoints removed. Chat streaming is handled
        // directly by the /chat endpoint which streams to the HTTP response.

        [HttpGet("prompts/readme")]
        public async Task<IActionResult> GetReadme()
        {
            var readme = Path.Combine(AppContext.BaseDirectory, "Prompts", "README.md");
            if (System.IO.File.Exists(readme))
                return Content(await System.IO.File.ReadAllTextAsync(readme), "text/plain");

            return NotFound();
        }

        [HttpGet("prompts")]
        public IActionResult GetPromptFiles()
        {
            var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");

            var files = Directory.Exists(promptsDir)
                ? Directory.GetFiles(promptsDir, "*.*")
                    .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName)
                    .ToList()
                : new List<string>();

            return Ok(files);
        }

        [HttpGet("prompts/{filename}")]
        public async Task<IActionResult> GetPromptFile(string filename)
        {
            if (!IsValidPromptFilename(filename))
                return BadRequest("Invalid filename.");

            var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
            var file = Path.GetFullPath(Path.Combine(promptsDir, filename));

            // Accept files in the promptsDir or its subfolders — ensure file is under promptsDir folder.
            var promptRootFull = Path.GetFullPath(promptsDir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!file.StartsWith(promptRootFull, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid filename.");

            if (!System.IO.File.Exists(file))
                return NotFound();

            return Content(await System.IO.File.ReadAllTextAsync(file), "text/plain");
        }

        [HttpPut("prompts/{filename}")]
        public async Task<IActionResult> SavePromptFile(string filename, [FromBody] string content)
        {
            try
            {
                if (!IsValidPromptFilename(filename) || string.IsNullOrWhiteSpace(content))
                    return BadRequest("Filename and content are required.");

                var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
                Directory.CreateDirectory(promptsDir);
                var file = Path.GetFullPath(Path.Combine(promptsDir, filename));

                if (!file.StartsWith(Path.GetFullPath(promptsDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid filename.");

                await System.IO.File.WriteAllTextAsync(file, content);
                _promptProvider.Invalidate(filename);

                _logger.LogInformation("Saved prompt file: {File}", file);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {File}", filename);
                return BadRequest("Error saving file.");
            }
        }

        [HttpPost("translate")]
        public async Task<ActionResult<TranslateResponseDto>> Translate([FromBody] TranslateRequestDto request, CancellationToken cancellationToken)
        {
            // Controller always returns a TranslateResponseDto so the UI can rely on a stable payload.
            if (request is null)
            {
                return Ok(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "Request body is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Ok(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "Field 'text' is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.SourceLanguage))
            {
                return Ok(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "Field 'sourceLanguage' is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.TargetLanguage))
            {
                return Ok(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "Field 'targetLanguage' is required."
                });
            }

            try
            {
                var translateService = HttpContext.RequestServices.GetService(typeof(ITranslateService)) as ITranslateService;
                if (translateService == null)
                {
                    _logger.LogError("Translation service not available");
                    return Ok(new TranslateResponseDto
                    {
                        TranslatedText = string.Empty,
                        Confidence = 0,
                        ErrorMessage = "Translation service not available."
                    });
                }

                var resp = await translateService.TranslateAsync(request, cancellationToken);

                if (resp == null)
                {
                    // Service returned no object - map to DTO with error message
                    return Ok(new TranslateResponseDto
                    {
                        TranslatedText = string.Empty,
                        Confidence = 0,
                        ErrorMessage = "Translation service returned no response."
                    });
                }

                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text");
                return Ok(new TranslateResponseDto
                {
                    TranslatedText = string.Empty,
                    Confidence = 0,
                    ErrorMessage = "Error translating text: " + ex.Message
                });
            }
        }

        [HttpDelete("prompts/{filename}")]
        public IActionResult ResetPromptFile(string filename)
        {
            try
            {
                if (!IsValidPromptFilename(filename))
                    return BadRequest("Invalid filename.");

                var promptsDir = Path.Combine(AppContext.BaseDirectory, "Prompts");
                var file = Path.GetFullPath(Path.Combine(promptsDir, filename));

                if (!file.StartsWith(Path.GetFullPath(promptsDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid filename.");

                if (System.IO.File.Exists(file))
                    System.IO.File.Delete(file);

                _promptProvider.Invalidate(filename);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting file: {File}", filename);
                return BadRequest("Error resetting file.");
            }
        }

        private static bool IsValidPromptFilename(string? filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // Reject any path separators or parent-directory traversal attempts.
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            if (filename.Contains("..", StringComparison.Ordinal))
                return false;

            return true;
        }

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
                var books = await _aiBooks.GetBooksByAuthorAsync(author, language, cancellationToken);
                return Ok(books);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("AuthorBooks request cancelled for {Author}", author);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
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
                var langName = MapLanguageCodeToName(request.Language);
                request.Messages.Insert(0, new ChatMessageDto
                {
                    Role = "system",
                    Content = $"Please respond in {langName}. Keep answers concise and avoid repetition."
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