using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

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
        private readonly ITranslateService _translateService;
        private readonly IPromptTemplateProvider _promptProvider;
        private readonly CloudAuthorBooksService _cloudBooks;
        private readonly CloudMediaService _cloudMedia;
        private readonly CloudTranslateService _cloudTranslate;
        private readonly CloudBookSummaryService _cloudSummaries;
        private readonly CloudAuthorProfileService _cloudAuthorProfile;
        private readonly AiUsageRecorderService _usage;
        private readonly ILogger<AiController> _logger;

        public AiController(
            IAiAuthorProfileService ai,
            IAiBookSummaryService books,
            IAiBooksService aiBooks,
            IAiMediaNotesService media,
            IAiMediaMetadataService mediaMetadata,
            ILogger<AiController> logger,
            IAiChatService chat,
            IPromptTemplateProvider promptProvider,
            ITranslateService translateService,
            CloudAuthorBooksService cloudBooks,
            CloudMediaService cloudMedia,
            CloudTranslateService cloudTranslate,
            CloudBookSummaryService cloudSummaries,
            CloudAuthorProfileService cloudAuthorProfile,
            AiUsageRecorderService usage)
        {
            _ai = ai;
            _books = books;
            _aiBooks = aiBooks;
            _media = media;
            _mediaMetadata = mediaMetadata;
            _logger = logger;
            _chat = chat;
            _promptProvider = promptProvider;
            _translateService = translateService;
            _cloudBooks = cloudBooks;
            _cloudMedia = cloudMedia;
            _cloudTranslate = cloudTranslate;
            _cloudSummaries = cloudSummaries;
            _cloudAuthorProfile = cloudAuthorProfile;
            _usage = usage;
        }

        private async Task RecordUsageAsync(
            string operacao,
            string provider,
            bool sucesso,
            double durationMs,
            string? contexto = null,
            string? scenario = null,
            CancellationToken ct = default)
        {
            var userId = Request?.Headers["X-UserId"].ToString();
            if (string.IsNullOrWhiteSpace(userId))
                userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            await _usage.RecordAsync(userId, operacao, provider, contexto: contexto, scenario: scenario, durationMs: durationMs, sucesso: sucesso, ct: ct);
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

        /// <summary>
        /// Returns the content of the README.md file in the Prompts folder, which can contain instructions or documentation 
        /// for how to create and use prompt templates.
        /// </summary>
        /// <returns></returns>

        [HttpGet("prompts/readme")]
        public async Task<IActionResult> GetReadme()
        {
            var readme = Path.Combine(AppContext.BaseDirectory, "Prompts", "README.md");
            if (System.IO.File.Exists(readme))
                return Content(await System.IO.File.ReadAllTextAsync(readme), "text/plain");

            return NotFound();
        }

        /// <summary>
        /// prompts endpoint allows the UI to list available prompt template files in the Prompts folder.
        /// </summary>
        /// <returns>Action result containing the list of prompt template files.</returns>

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

        /// <summary>
        /// get prompt file endpoint allows the UI to retrieve the content of a specific prompt template file by name.
        /// </summary>
        /// <param name="filename">The name of the prompt template file to retrieve.</param>
        /// <returns>Action result containing the content of the specified prompt template file.</returns>

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

        /// <summary>
        /// save prompt file endpoint allows the UI to save content to a specific prompt template file by name.
        /// </summary>
        /// <param name="filename">The name of the prompt template file to save.</param>
        /// <param name="content">The content to save to the prompt template file.</param>
        /// <returns>Action result indicating the success or failure of the save operation.</returns>

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

        /// <summary>
        /// translate endpoint accepts a text string along with source and target language codes, 
        /// and returns the translated text along with a confidence score and any error messages.
        /// </summary>
        /// <param name="request">The translation request containing the text, source language, and target language.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action result containing the translation response.</returns>

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
                if (_translateService == null)
                {
                    _logger.LogError("Translation service not available");
                    return Ok(new TranslateResponseDto
                    {
                        TranslatedText = string.Empty,
                        Confidence = 0,
                        ErrorMessage = "Translation service not available."
                    });
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var resp = await _translateService.TranslateAsync(request, cancellationToken);

                if (resp == null)
                {
                    // Service returned no object - map to DTO with error message
                    await RecordUsageAsync("translate", AiUsageRecorderService.ProviderOllama, false, sw.ElapsedMilliseconds,
                        $"{request.SourceLanguage}->{request.TargetLanguage}", request.Scenario, cancellationToken);
                    return Ok(new TranslateResponseDto
                    {
                        TranslatedText = string.Empty,
                        Confidence = 0,
                        ErrorMessage = "Translation service returned no response."
                    });
                }

                await RecordUsageAsync(
                    "translate",
                    AiUsageRecorderService.ProviderOllama,
                    !string.IsNullOrWhiteSpace(resp.TranslatedText) && string.IsNullOrWhiteSpace(resp.ErrorMessage),
                    sw.ElapsedMilliseconds,
                    $"{request.SourceLanguage}->{request.TargetLanguage}",
                    request.Scenario,
                    cancellationToken);

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

        /// <summary>
        /// delete prompt file endpoint allows the UI to delete a specific prompt template file by name, 
        /// which also invalidates any cached version of that prompt in the application.
        /// </summary>
        /// <param name="filename">The name of the prompt template file to delete.</param>
        /// <returns>Action result indicating the success or failure of the delete operation.</returns>

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

        /// <summary>
        /// get author profile endpoint accepts an author's name and optional language, and returns an AI-generated profile for that author,
        /// </summary>
        /// <param name="request">The request containing the author's name and optional language.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The AI-generated author profile.</returns>

        [HttpPost("author-profile")]
        public async Task<ActionResult<AiAuthorProfileResponseDto>> AuthorProfile(
            [FromBody] AiAuthorProfileRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Field 'name' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var provider = _ai is OllamaAuthorProfileService
                ? AiUsageRecorderService.ProviderOllama
                : AiUsageRecorderService.ProviderOpenAiSdk;
            var resp = await _ai.GetAuthorProfileAsync(
                request.Name,
                language,
                request.ConnectionId,
                request.CorrelationId,
                cancellationToken);

            await RecordUsageAsync("author-profile", provider, resp is not null, sw.ElapsedMilliseconds, request.Name, request.Scenario, cancellationToken);
            return Ok(resp);
        }

        /// <summary>
        /// Generates an author profile via OpenRouter. Returns 503 when the cloud
        /// path is unavailable so the UI can fall back to the local provider.
        /// </summary>
        [HttpPost("author-profile-cloud")]
        public async Task<ActionResult<AiAuthorProfileResponseDto>> AuthorProfileCloud(
            [FromBody] AiAuthorProfileRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Field 'name' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudAuthorProfile.GetAuthorProfileAsync(request.Name, language, cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("author-profile", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Name, request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync(
                    "author-profile",
                    AiUsageRecorderService.ProviderOpenRouter,
                    !string.IsNullOrWhiteSpace(resp.Bio),
                    sw.ElapsedMilliseconds,
                    request.Name,
                    request.Scenario,
                    cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("AuthorProfileCloud request cancelled.");
                await RecordUsageAsync("author-profile", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Name, request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating author-profile via OpenRouter");
                await RecordUsageAsync("author-profile", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Name, request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating author-profile via OpenRouter");
            }
        }

        /// <summary>
        /// summary endpoint accepts a book title, optional author name and language, 
        /// and returns an AI-generated summary of the book along with metadata such as 
        /// category, published date, page count, cover image URL, price, confidence level, and any warnings about the content.
        /// </summary>
        /// <param name="request">The request containing the book title, optional author name, and language.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The AI-generated book summary.</returns>

        [HttpPost("book-summary")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummary(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resp = await _books.GetBookSummaryAsync(request.Title, request.Author, language, cancellationToken);
            await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOllama, resp is not null, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
            return Ok(resp);
        }

        /// <summary>
        /// book summary only endpoint accepts a book title, optional author name and language,
        /// and returns an AI-generated summary of the book without additional metadata.
        /// </summary>
        /// <param name="request">The request containing the book title, optional author name, and language.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The AI-generated book summary without additional metadata.</returns>

        [HttpPost("book-summary-only")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummaryOnly(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resp = await _books.GetBookSummaryOnlyAsync(
                request.Title,
                request.Author,
                language,
                request.ConnectionId,
                request.CorrelationId,
                cancellationToken);
            await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOllama, resp is not null, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
            return Ok(resp);
        }

        /// <summary>
        /// Get an AI-generated book summary/metadata via OpenRouter. Returns 503
        /// when the cloud path is unavailable (offline, no key, HTTP/parse
        /// failure) so the UI can fall back to the local Ollama provider.
        /// </summary>
        [HttpPost("book-summary-cloud")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummaryCloud(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudSummaries.GetBookSummaryAsync(
                    request.Title,
                    request.Author,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, true, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("BookSummaryCloud request cancelled for '{Title}'", request.Title);
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating book summary via OpenRouter for '{Title}'", request.Title);
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating book summary via OpenRouter");
            }
        }

        /// <summary>
        /// Get an AI-generated book summary (without extra metadata) via OpenRouter.
        /// Returns 503 when the cloud path is unavailable so the UI can fall back
        /// to the local Ollama provider.
        /// </summary>
        [HttpPost("book-summary-only-cloud")]
        public async Task<ActionResult<AiBookSummaryResponseDto>> BookSummaryOnlyCloud(
            [FromBody] AiBookSummaryRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudSummaries.GetBookSummaryOnlyAsync(
                    request.Title,
                    request.Author,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, true, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("BookSummaryOnlyCloud request cancelled for '{Title}'", request.Title);
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating book summary-only via OpenRouter for '{Title}'", request.Title);
                await RecordUsageAsync("book-summary", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, request.Title, request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating book summary-only via OpenRouter");
            }
        }

        /// <summary>
        /// List of the most relevant books by a given author, based on AI analysis of the author's bibliography and influence. 
        /// Results may include books not written by the author but closely related to their work or frequently associated with them.
        /// </summary>
        /// <param name="author">The name of the author.</param>
        /// <param name="language">The language for the results.</param>
        /// <param name="connectionId">Optional connection ID for tracking.</param>
        /// <param name="correlationId">Optional correlation ID for tracking.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>List of books by the specified author.</returns>
        [HttpGet("author-books")]
        public async Task<ActionResult<List<GoogleBookInfo>>> AuthorBooks(
            string author,
            string language = "pt",
            string? connectionId = null,
            string? correlationId = null,
            string? scenario = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(author))
                return BadRequest("author parameter is required");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var books = await _aiBooks.GetBooksByAuthorAsync(author, language, connectionId, correlationId, cancellationToken);
                await RecordUsageAsync("author-books", AiUsageRecorderService.ProviderOllama, books is not null, sw.ElapsedMilliseconds, author, scenario, cancellationToken);
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
                return StatusCode(500, TruncateMessage(ex));
            }
        }

        /// <summary>
        /// List of the most relevant books by a given author, generated via OpenRouter.
        /// Always returns a <see cref="CloudAuthorBooksResult"/>: on success the Books
        /// list is populated; on cloud failure the FailureReason and Outcomes are set so
        /// the UI can explain and let the user decide whether to continue to the local
        /// AI provider. Returns 503 only on hard failures (exception or client abort).
        /// </summary>
        [HttpGet("author-books-cloud")]
        public async Task<ActionResult<CloudAuthorBooksResult>> AuthorBooksCloud(
            string author,
            string language = "pt",
            string? connectionId = null,
            string? correlationId = null,
            string? scenario = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(author))
                return BadRequest(new CloudAuthorBooksResult { FailureReason = "Parâmetro 'author' obrigatório." });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _logger.LogInformation(
                    "AuthorBooksCloud iniciado para {Author} (language={Language}).",
                    author, language);

                var result = await _cloudBooks.GetAuthorBooksResultAsync(
                    author, language, connectionId, correlationId, cancellationToken);
                await RecordUsageAsync(
                    "author-books",
                    AiUsageRecorderService.ProviderOpenRouter,
                    result is not null && result.Books?.Count > 0,
                    sw.ElapsedMilliseconds,
                    author,
                    scenario,
                    cancellationToken);
                return Ok(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "AuthorBooksCloud request cancelled for {Author} após {DurationMs}ms (função terminada por timeout do cliente).",
                    author, sw.ElapsedMilliseconds);
                await RecordUsageAsync(
                    "author-books", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, author, scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new CloudAuthorBooksResult
                    {
                        FailureReason = $"Pedido OpenRouter cancelado após {sw.ElapsedMilliseconds}ms (timeout do cliente).",
                        WasCancelled = true
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating author books via OpenRouter for {Author} após {DurationMs}ms", author, sw.ElapsedMilliseconds);
                await RecordUsageAsync(
                    "author-books", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, author, scenario, cancellationToken);
                return StatusCode(503,
                    new CloudAuthorBooksResult { FailureReason = TruncateMessage(ex) });
            }
        }

        /// <summary>
        /// Get flag values for AI features. This allows the UI to enable/disable features based on these flags without needing a full app configuration refresh.
        /// </summary>
        /// <returns>Current feature flags for AI features.</returns>

        [HttpGet("flags")]
        public ActionResult<MediaOrganizerApp.Application.FeatureFlags> GetFlags()
        {
            return Ok(MediaOrganizerApp.WebApi.Services.FeatureFlagsStore.Get());
        }

        /// <summary>
        /// Update flag values for AI features. This allows dynamic enabling/disabling of features without needing to restart the application or refresh a full configuration.
        /// </summary>
        /// <param name="flags">The new feature flags to set.</param>
        /// <returns>Action result indicating the outcome of the operation.</returns>

        [HttpPut("flags")]
        public ActionResult SetFlags([FromBody] MediaOrganizerApp.Application.FeatureFlags flags)
        {
            MediaOrganizerApp.WebApi.Services.FeatureFlagsStore.Set(flags);
            return Ok();
        }


        /// <summary>
        /// Get AI-generated notes for a media item (e.g. song, movie) based on its title and artist.
        /// The response includes the generated notes, confidence level, and any warnings about the content. 
        /// This can be used to provide additional context or insights about the media item in the UI.
        /// </summary>
        /// <param name="request">The request containing the media item's title, artist, and optional language.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>Action result containing the AI-generated notes for the media item.</returns>

        [HttpPost("media-notes")]
        public async Task<ActionResult<AiMediaNotesResponseDto>> MediaNotes(
            [FromBody] AiMediaNotesRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _media.GetMediaNotesAsync(
                    request.Artist,
                    request.Title,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOllama, resp is not null, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating media notes for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOllama, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return BadRequest("Error generating media notes.");
            }
        }

        /// <summary>
        /// Get AI-generated metadata for a media item (e.g. song, movie) based on its title and artist.
        /// The response includes the generated metadata, confidence level, and any warnings about the content.
        /// This can be used to provide additional context or insights about the media item in the UI.
        /// </summary>
        /// <param name="request">The request containing the media item's title, artist, and optional language.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>Action result containing the AI-generated metadata for the media item.</returns>

        [HttpPost("media-metadata")]
        public async Task<ActionResult<AiMediaMetadataResponseDto>> MediaMetadata(
            [FromBody] AiMediaMetadataRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resp = await _mediaMetadata.GetMediaMetadataAsync(
                request.Artist,
                request.Title,
                language,
                request.ConnectionId,
                request.CorrelationId,
                cancellationToken);
            await RecordUsageAsync("media-metadata", AiUsageRecorderService.ProviderOllama, resp is not null, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
            return Ok(resp);
        }

        /// <summary>
        /// Get the edition year for a media item (e.g. song, movie) based on its title and artist using AI analysis.
        /// </summary>
        /// <param name="request">The request containing the media item's title, artist, and optional language.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>Action result containing the AI-generated edition year for the media item.</returns>

        [HttpPost("media-edition-year")]
        public async Task<ActionResult<string>> MediaEditionYear(
            [FromBody] MediaEditionYearRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var year = await _mediaMetadata.GetMediaEditionYearAsync(
                request.Artist,
                request.Title,
                language,
                request.ConnectionId,
                request.CorrelationId,
                cancellationToken);
            await RecordUsageAsync("media-edition-year", AiUsageRecorderService.ProviderOllama, !string.IsNullOrWhiteSpace(year), sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
            return Ok(string.IsNullOrWhiteSpace(year) ? "N/A" : year);
        }

        /// <summary>
        /// Get AI-generated notes for a media item via OpenRouter. Returns 503
        /// when the cloud path is unavailable (offline, no key, HTTP/parse
        /// failure) so the UI can fall back to the local Ollama provider.
        /// </summary>
        [HttpPost("media-notes-cloud")]
        public async Task<ActionResult<AiMediaNotesResponseDto>> MediaNotesCloud(
            [FromBody] AiMediaNotesRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudMedia.GetMediaNotesAsync(
                    request.Artist,
                    request.Title,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOpenRouter, true, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("MediaNotesCloud request cancelled for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating media notes via OpenRouter for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-notes", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating media notes via OpenRouter");
            }
        }

        /// <summary>
        /// Get AI-generated metadata for a media item via OpenRouter. Returns 503
        /// when the cloud path is unavailable so the UI can fall back to Ollama.
        /// </summary>
        [HttpPost("media-metadata-cloud")]
        public async Task<ActionResult<AiMediaMetadataResponseDto>> MediaMetadataCloud(
            [FromBody] AiMediaMetadataRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudMedia.GetMediaMetadataAsync(
                    request.Artist,
                    request.Title,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("media-metadata", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync("media-metadata", AiUsageRecorderService.ProviderOpenRouter, true, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("MediaMetadataCloud request cancelled for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-metadata", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating media metadata via OpenRouter for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-metadata", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating media metadata via OpenRouter");
            }
        }

        /// <summary>
        /// Get the edition year for a media item via OpenRouter. Returns 503
        /// when the cloud path is unavailable so the UI can fall back to Ollama.
        /// </summary>
        [HttpPost("media-edition-year-cloud")]
        public async Task<ActionResult<string>> MediaEditionYearCloud(
            [FromBody] MediaEditionYearRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Field 'title' is required.");
            if (string.IsNullOrWhiteSpace(request.Artist)) return BadRequest("Field 'artist' is required.");

            var language = string.IsNullOrWhiteSpace(request.Language) ? "pt" : request.Language;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var year = await _cloudMedia.GetMediaEditionYearAsync(
                    request.Artist,
                    request.Title,
                    language,
                    request.ConnectionId,
                    request.CorrelationId,
                    cancellationToken);
                if (year is null)
                {
                    await RecordUsageAsync("media-edition-year", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync("media-edition-year", AiUsageRecorderService.ProviderOpenRouter, true, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return Ok(year);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("MediaEditionYearCloud request cancelled for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-edition-year", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating media edition year via OpenRouter for '{Artist}' - '{Title}'", request.Artist, request.Title);
                await RecordUsageAsync("media-edition-year", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.Artist} - {request.Title}", request.Scenario, cancellationToken);
                return StatusCode(503, "Error generating media edition year via OpenRouter");
            }
        }

        /// <summary>
        /// Translates text via OpenRouter. Returns 503 when the cloud path is
        /// unavailable so the UI can fall back to the local Ollama provider.
        /// </summary>
        [HttpPost("translate-cloud")]
        public async Task<ActionResult<TranslateResponseDto>> TranslateCloud(
            [FromBody] TranslateRequestDto request,
            CancellationToken cancellationToken)
        {
            if (request is null) return BadRequest("Request body is required.");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var resp = await _cloudTranslate.TranslateAsync(request, cancellationToken);
                if (resp is null)
                {
                    await RecordUsageAsync("translate", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.SourceLanguage}->{request.TargetLanguage}", request.Scenario, cancellationToken);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "OpenRouter unavailable");
                }
                await RecordUsageAsync(
                    "translate",
                    AiUsageRecorderService.ProviderOpenRouter,
                    !string.IsNullOrWhiteSpace(resp.TranslatedText) && string.IsNullOrWhiteSpace(resp.ErrorMessage),
                    sw.ElapsedMilliseconds,
                    $"{request.SourceLanguage}->{request.TargetLanguage}",
                    request.Scenario,
                    cancellationToken);
                return Ok(resp);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("TranslateCloud request cancelled.");
                await RecordUsageAsync("translate", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.SourceLanguage}->{request.TargetLanguage}", request.Scenario, cancellationToken);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Request cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error translating text via OpenRouter");
                await RecordUsageAsync("translate", AiUsageRecorderService.ProviderOpenRouter, false, sw.ElapsedMilliseconds, $"{request.SourceLanguage}->{request.TargetLanguage}", request.Scenario, cancellationToken);
                return StatusCode(503, "Error translating text via OpenRouter");
            }
        }

        /// <summary>
        /// Chat endpoint that accepts a list of messages and streams AI-generated responses back to the client in real-time.
        /// </summary>
        /// <param name="request">The request containing the list of messages and optional language.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>Action result containing the AI-generated responses streamed in real-time.</returns>

        [HttpPost("chat")]
        public async Task Chat([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            Response.ContentType = "application/x-ndjson";

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
                    request.CorrelationId,
                    cancellationToken))
                {
                    await Response.WriteAsync(JsonSerializer.Serialize(chunk), cancellationToken);
                    await Response.WriteAsync("\n", cancellationToken);

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

        private static string TruncateMessage(Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex?.Message) ? ex?.GetType().Name ?? "Unknown error" : ex.Message;
            return message.Length > 200 ? message[..200] + "..." : message;
        }
    }
}
