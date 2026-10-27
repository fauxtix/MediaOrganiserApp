
# AI Fallback Runbook (Blazor + Web API) — compile-oriented

This document describes the implemented AI fallback approach in this solution.

## Standing rule

External metadata APIs first; AI fallback runs **only** when external lookup fails/returns empty.

## Usage logging (non-chat)

Every non-chat AI operation is auto-recorded server-side by `MediaOrganizerApp.WebApi\Controllers\AiController.cs`
into the `AIUsageLog` table (`AiUsageRecorderService`). The `Scenario` column tags the caller context:
- `cloud-fallback` set by the Blazor wrappers (`TranslateClientService`, `AiBookClientService`, `AuthorBooks.razor.cs`) when OpenRouter failed and the local Ollama provider was used;
- `googlebooks` / `author-api` / `media-api` tagged at the page call sites (`GoogleBooksApiSearch`, `EditAuthor`, `EditMedia`);
- `direct` (default) for plain cloud/direct usage.

Dashboard section "IA em uso interno" reads `GET /api/ai-usage/summary` via `Web.Blazor\Services\ApiWrappers\AiUsageClientService.cs`.
Chat is intentionally excluded — it stays in `AIResponses`.

### Books: "Pesquisar Books Online/AI"

`Web.Blazor\Pages\Publications\GoogleBooksApiSearch.razor.cs` (hosted by `EditPublicacoes`) resolves a book as:
1. **Google Books API** (`GoogleBooksClientService.GetBookAsync`) with **bare** `intitle:...`/`inauthor:...` operators — the quoted phrase syntax (`intitle:"..."`) returns **zero items**, so never reintroduce it. The translated-title retry was removed with it. When a found volume lacks `volumeInfo.description`, its `searchInfo.textSnippet` becomes `Resumo` (MyMemory-translated) — AI is only reached for items that have **neither** `description` **nor** `textSnippet`.
2. **OpenRouter cloud** — `AiBookClientService.GetBookSummaryCloudAsync`/`GetBookSummaryOnlyCloudAsync` → `POST api/Ai/book-summary-cloud` / `book-summary-only-cloud` → `CloudBookSummaryService` (free/paid gate + catalog sweep + free-daily-cap). Only when Google Books returns no items at all, or items without both `description` and `textSnippet`.
3. **Ollama local** — on cloud null/503 it falls back to `book-summary`/`book-summary-only` with `scenario: "cloud-fallback"`.

Both cloud endpoints record `AIUsageLog` rows with `provider=openrouter` + the caller `scenario` (`googlebooks`); the local fallback records `provider=ollama` + `scenario=cloud-fallback` (`EraFallback=1`).

### Author books: `Components/Authors/AuthorBooks.razor`

Different flow from the summary lookup above (it resolves *books for an existing author*, not a book for an ISBN/title):
1. **Google Books API** — `GoogleBooksClientService.GetBooksByAuthorAsync` with the bare `inauthor:{author}` operator plus `fields=...,volumeInfo/authors`. The unquoted operator matches loosely-related volumes, so the result is filtered client-side by `Application/Utilities/AuthorNameMatcher` (tolerant of accents, punctuation, word order and middle initials: `DelBono` = `Del Bono`, `McCaffrey, James` = `James M. McCaffrey`). Items without `volumeInfo/authors` are always kept, and if the filter would empty the list the **unfiltered** list is returned instead, so a different credited spelling never blanks the page. The bare `intitle:`/`inauthor:` form is mandatory — the quoted phrase syntax returns zero items.
2. **OpenRouter cloud** — `AiAuthorBooksClientService` → `CloudAuthorBooksService`, which sweeps free models. Because the free pool is flaky, the sweep is defensive: `ChatSafetyArtifactFilter` drops responses that are only a content-safety classification (`User Safety: safe;` from guard/free-router models) instead of treating them as a parse failure, safety classifiers are excluded from the candidate list, and an **upstream** 429 (`limit_source=upstream_provider_shared_pool`) is retried once on the same model after 6s (account-level 429s are not retried). The retry delay is injectable via the constructor for tests.
3. **Ollama local** — on cloud null/503 it falls back to the local `author-books` endpoint with `scenario: "cloud-fallback"`.

### Translation: user cancellation

`Web.Blazor\Pages\Tools\TranslateText.razor` is hosted by 8 pages (all pass `TranslateFunc`, so none of them needed changes): `CategoriasMedia`, `CategoriasPublicacoes`, `EditAuthor`, `Formatos`, `FormatosMedia`, `LocaisArquivo`, `EditMedia`, `EditPublicacoes`.

The component owns a `CancellationTokenSource` per attempt and hands its token to `TranslateFunc`:

- **Cancel button** while `IsTranslating` → `CancelTranslate()` aborts the pending request. The UI stops **immediately** — it never waits for the provider to acknowledge (local Ollama may keep inferring on its own). The **Translate** button stays available on the left; the static `TituloTraducaoCancelada` note ("Cancelado pelo utilizador" / "Cancelled by the user" / "Annulé par l'utilisateur" / "Cancelado por el usuario") appears on the **right**, in the slot that otherwise holds `Confidence` (which is `null` in this state, so the two never collide). Insisting on translating clears the note (`Translate()` sets `IsCancelled = false`); `Confirmar` stays disabled because there is no translated text. The 900px dialog is wide enough for both slots (≈110px button + ≈195px note against ≈395px of column) — no width change was needed.
- **Closing the dialog** (`OnExit`) and **component disposal** (`DisposeAsync`, and `Visible` flipping to false in `OnParametersSetAsync`) cancel silently, so a late answer can no longer overwrite the field the translation was requested for (e.g. the author bio). `Translate()` also guards every write with `if (cts.IsCancellationRequested || !ReferenceEquals(_cts, cts)) return;` — a superseded attempt never paints state, and its `finally` tolerates an already-disposed component.

Cancellation is propagated end-to-end, so the provider stops too: aborting the Blazor `HttpClient` request makes ASP.NET Core cancel the `CancellationToken` the controller receives → `CloudTranslateService.ThrowIfCancellationRequested()` / `SendAsync(..., ct)` and `OllamaBaseClient` (linked CTS that distinguishes a 300 s timeout from a user cancel). Both providers **rethrow** `OperationCanceledException` instead of reporting it as an AI failure: `OllamaTranslateService.TranslateAsync` and `TranslateClientService.TranslateLocalAsync` (the cloud path and `TranslateAsync` also `ThrowIfCancellationRequested` before the local fallback, so cancelling cloud does not trigger an Ollama call).

`TranslateClientService` is registered with `Timeout = 6 min` (was the 100 s default) to outlast the server-side 300 s budget in `OllamaBaseClient.ExecuteGenerateAsync`; the timeout is a ceiling, never a wait.

### Translation: which model answered

The sweep of up to 12 candidates (`CloudTranslateService.BuildCandidateModelsAsync`) plus the silent cloud→Ollama fallback make the serving model **unpredictable by construction** — the configured `CloudChat:DefaultModel` is rarely what answered. Two transports, answering two different questions:

- **Finished answer → `TranslateResponseDto.Provider` / `.Model`** (authoritative; survives a lost progress event). `CloudTranslateService` stamps the model the provider **actually served**, read from the response body (`ExtractServedModel`), because the request may have gone through the `openrouter/free` router and the requested id is then never the real one — the same trick as `RealModelName` in `OpenRouterChat`. When the body carries no `model`, it falls back to the requested candidate, which the UI renders as `CloudChatAutomaticModel` ("Automatic") rather than printing `openrouter/free`. `OllamaTranslateService` resolves the local name once via `GetAiSettingsAsync()` (`protected` on `OllamaBaseClient`).
- **Model in flight → `ProgressUpdateDto.Provider` / `.Model`.** The only possible source on cancellation, since an aborted request never returns a body. `CloudTranslateService` now injects `IProgressPublisher` and publishes `cloud_try` per candidate plus `cloud_success`/`cloud_failed`/`cloud_empty` for the winner; `OllamaTranslateService` carries the name on every stage it already published. `TrackModelInFlight` in `TranslateText.razor.cs` records the latest one, with a fallback to the stage message when the structured field is missing.

Both services **stamp** the values over the deserialized DTO. `TranslateResponseDto` is deserialized straight from the model's own JSON output, so a `"provider"`/`"model"` emitted by the model must never be trusted (covered by tests on both providers).

UI: the chip sits **under the Translate button** — never in the right slot, which is taken by the cancelled note or by `Confidence`, and model ids are long enough to overlap. It shows in exactly two states — **success** (`⊘ OpenRouter · llama-3.3-70b`, from the DTO) and **cancelled** (same chip, from progress, with "Model being attempted" in the `title`). While translating the chip is **absent on purpose**: the progress message already names the model, and `AIProcessingCloudTrying` ("A tentar com OpenRouter ({0})...") only started firing now that `CloudTranslateService` publishes `cloud_try` — before this change the dialog never even showed that OpenRouter was being tried. The finished answer wins over the model that was merely in flight (`ShowModelChip`/`ShownProvider`/`ShownModel`), and the chip is never shown when the response carries an `ErrorMessage`. Names are truncated in the chip with the full id in the tooltip; a SignalR hiccup degrades to no chip, never a broken one.

Auditing: `AiController.RecordUsageAsync` gained a `modelo` parameter and `Translate`/`TranslateCloud` pass `resp.Model`, so the `AIUsageLog.Modelo` column (which existed and was never written) is finally populated. `AiUsageRecorderService` truncates it to 128 chars, matching the column — without that, an over-long id would throw inside the recorder and turn a successful translation into a 503. Only the `translate` operation is filled; the other five still record no model.

## Implemented components

### Shared contracts

- `Application\DTOs\AiAuthorProfileRequestDto.cs`
- `Application\DTOs\AiAuthorProfileResponseDto.cs`
- `Application\Interfaces\IAiAuthorProfileService.cs`

### Web API

- Endpoints: `POST api/Ai/author-profile-cloud` (OpenRouter first) and `POST api/Ai/author-profile` (local fallback)
- Controller: `MediaOrganizerApp.WebApi\Controllers\AiController.cs`
- Services: `MediaOrganizerApp.WebApi\Services\CloudChat\CloudAuthorProfileService.cs` (OpenRouter, gate + catalog sweep + free-daily-cap) and `MediaOrganizerApp.WebApi\Services\AiAuthorProfileService.cs` / `OllamaAuthorProfileService.cs` (local)
- DI registration: `MediaOrganizerApp.WebApi\Program.cs`
  - `builder.Services.AddScoped<CloudAuthorProfileService>();`
  - `builder.Services.AddScoped<IAiAuthorProfileService, AiAuthorProfileService>();`

### Blazor

- Wrapper: `Web.Blazor\Services\ApiWrappers\AiClientService.cs` — `GetAuthorProfileAsync` calls `author-profile-cloud` first, falls back to `author-profile` with `Scenario = "cloud-fallback"` on null/503
- DI registration: `Web.Blazor\ServiceRegistration.cs`
  - `services.AddHttpClient<AiClientService>();`
- Usage (external first, AI fallback only if empty):
  - `Web.Blazor\Pages\LookupTables\EditAuthor.razor.cs` → `GetAuthorDataFromApi()`

## Runtime prerequisites

- Add the `OpenAI` NuGet package to `MediaOrganizerApp.WebApi`.
- Configure in Web API (user-secrets or environment variables):
  - `OpenAI:ApiKey`
  - `OpenAI:Model` (optional)

## Security

- The AI endpoint is protected by your existing `ApiKeyMiddleware` (header: `ApiKey`).
- Do not store LLM keys in `Web.Blazor`.

---

## 1) Solution context / conventions

- UI project: `Web.Blazor` (Blazor Server)
- Backend project: `MediaOrganizerApp.WebApi`
- Shared DTOs: `MediaOrganizerApp.Application.DTOs`
- Current pattern:
  - Blazor calls Web API through typed wrappers in `Web.Blazor\Services\ApiWrappers`
  - wrappers registered in `Web.Blazor\ServiceRegistration.cs` via `services.AddHttpClient<...>()`
  - Web API endpoints live in `MediaOrganizerApp.WebApi\Controllers`
  - API is protected by `ApiKeyMiddleware` (header `ApiKey`)

---

## 2) Standing rule: external API first, AI fallback only on failure

### 2.1 What counts as “external API failed”
In `Web.Blazor\Pages\LookupTables\EditAuthor.razor.cs` (`GetAuthorDataFromApi()`):
- failure = exception OR external returns empty bio (`string.IsNullOrEmpty(authorDetails.bio)`)

### 2.2 Why this rule exists
- External metadata APIs often return “not found”.
- LLMs can “always respond”, but without sources can produce incorrect facts.
- So: **use external APIs as the primary grounded source**, and AI as a best-effort fallback.

---

## 3) Design goals for AI fallback
- Improve hit-rate when external APIs can’t find the author profile.
- Keep AI/LLM secrets server-side (Web API), never in Blazor.
- Prefer structured output for stability:
  - `Bio`, `PhotoUrl`, `Confidence`, `Notes`
- Recommended UI behavior:
  - if AI fallback used, show a banner/message like “Bio gerada por AI (fallback)”.

---

## 4) Implementation plan (high level)

1. Add shared DTOs: request/response
2. Add Web API service `IAiAuthorProfileService` + implementation
3. Add Web API endpoint: `POST api/Ai/author-profile`
4. Add Blazor wrapper: `AiClientService`
5. Register wrapper in `Web.Blazor\ServiceRegistration.cs`
6. Update `EditAuthor.GetAuthorDataFromApi()` to call AI fallback only when external returns empty

---

## 5) Contracts (DTOs) — shared project

Create these in `MediaOrganizerApp.Application` so both Web API and Blazor can reuse them.

### 5.1 Request DTO
Create `MediaOrganizerApp.Application\DTOs\AiAuthorProfileRequestDto.cs`

MediaOrganizerApp.Application\DTOs\AiAuthorProfileRequestDto.cs namespace MediaOrganizerApp.Application.DTOs;
public sealed class AiAuthorProfileRequestDto { public string Name { get; set; } = string.Empty; }

### 5.2 Response DTO
Create `MediaOrganizerApp.Application\DTOs\AiAuthorProfileResponseDto.cs`

MediaOrganizerApp.Application\DTOs\AiAuthorProfileResponseDto.cs namespace MediaOrganizerApp.Application.DTOs;
public sealed class AiAuthorProfileResponseDto {
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Notes { get; set; } = string.Empty;
}

---

## 6) Web API: AI service + endpoint

> Notes:
> - This runs in `MediaOrganizerApp.WebApi` so the API key stays server-side.
> - The code below uses the `OpenAI` .NET package (official).
> - If you want Azure OpenAI specifically, adjust client creation (endpoint + deployment).

### 6.1 Add Web API NuGet package
In `MediaOrganizerApp.WebApi` install:
- `OpenAI` (NuGet)

### 6.2 Configuration (Web API)
Add these to Web API user-secrets / config:
- `OpenAI:ApiKey`
- `OpenAI:Model` (e.g., `gpt-4o-mini`)

### 6.3 Service contract
Create `MediaOrganizerApp.Application\Interfaces\IAiAuthorProfileService.cs`

MediaOrganizerApp.Application\Interfaces\IAiAuthorProfileService.cs using MediaOrganizerApp.Application.DTOs; using System.Threading; using System.Threading.Tasks;
namespace MediaOrganizerApp.Application.Interfaces;
public interface IAiAuthorProfileService { Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, CancellationToken cancellationToken = default); }

### 6.4 Web API service implementation
Create `MediaOrganizerApp.WebApi\Services\AiAuthorProfileService.cs`


MediaOrganizerApp.WebApi\Services\AiAuthorProfileService.cs using MediaOrganizerApp.Application.DTOs; using MediaOrganizerApp.Application.Interfaces; using Microsoft.Extensions.Configuration; using OpenAI.Chat; using System; using System.Text.Json; using System.Threading; using System.Threading.Tasks;
namespace MediaOrganizerApp.WebApi.Services;
public sealed class AiAuthorProfileService : IAiAuthorProfileService { private readonly ChatClient _chat;
public AiAuthorProfileService(IConfiguration config)
{
    var apiKey = config["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("Missing configuration key: OpenAI:ApiKey");

    var model = config["OpenAI:Model"];
    if (string.IsNullOrWhiteSpace(model))
        model = "gpt-4o-mini";

    _chat = new ChatClient(model, apiKey);
}

public async Task<AiAuthorProfileResponseDto> GetAuthorProfileAsync(string name, CancellationToken cancellationToken = default)
{
    // Defensive: external APIs failed, so we prefer cautious output.
    var system =
        """
        You generate short author profiles.
        Requirements:
        - If you are not confident about facts, state it in Notes and lower Confidence.
        - Do NOT invent exact dates, awards, or long bibliographies.
        - If the name is ambiguous (multiple candidates), mention ambiguity in Notes.
        - Output MUST be valid JSON matching:
          { "bio": string|null, "photoUrl": string|null, "confidence": number|null, "notes": string|null }
        """;

    var user =
        $"""
         Author name: {name}
         Write a concise bio (2-6 sentences) in Portuguese (pt-PT).
         """;

    var messages = new ChatMessage[]
    {
        new SystemChatMessage(system),
        new UserChatMessage(user),
    };

    var result = await _chat.CompleteChatAsync(messages, cancellationToken: cancellationToken);
    var text = result.Value.Content[0].Text;

    try
    {
        var parsed = JsonSerializer.Deserialize<AiAuthorProfileResponseDto>(
            text,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return parsed ?? new AiAuthorProfileResponseDto { Bio = text, Confidence = 0.2, Notes = "Non-JSON fallback." };
    }
    catch
    {
        return new AiAuthorProfileResponseDto
        {
            Bio = text,
            Confidence = 0.2,
            Notes = "Model response was not valid JSON."
        };
    }
}
}

### 6.5 Web API controller
Create `MediaOrganizerApp.WebApi\Controllers\AiController.cs`

MediaOrganizerApp.WebApi\Controllers\AiController.cs
using MediaOrganizerApp.Application.DTOs; 
using MediaOrganizerApp.Application.Interfaces; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using System;

namespace MediaOrganizerApp.WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly IAiAuthorProfileService _aiAuthorProfileService;

    public AiController(IAiAuthorProfileService aiAuthorProfileService)
    {
        _aiAuthorProfileService = aiAuthorProfileService;
    }

    [HttpPost("author-profile")]
    [Authorize(Policy = "ApiKey")]
    public async Task<IActionResult> AuthorProfile([FromBody] AiAuthorProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Author name is required.");

        try
        {
            var result = await _aiAuthorProfileService.GetAuthorProfileAsync(request.Name);

            return result.Confidence < 0.3
                ? Ok(new AiAuthorProfileResponseDto { Bio = null, Notes = "AI fallback used, low confidence." })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

