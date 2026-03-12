
# AI Fallback Runbook (Blazor + Web API) — compile-oriented

This document describes the implemented AI fallback approach in this solution.

## Standing rule

External metadata APIs first; AI fallback runs **only** when external lookup fails/returns empty.

## Implemented components

### Shared contracts

- `Application\DTOs\AiAuthorProfileRequestDto.cs`
- `Application\DTOs\AiAuthorProfileResponseDto.cs`
- `Application\Interfaces\IAiAuthorProfileService.cs`

### Web API

- Endpoint: `POST api/Ai/author-profile`
- Controller: `MediaOrganizerApp.WebApi\Controllers\AiController.cs`
- Service: `MediaOrganizerApp.WebApi\Services\AiAuthorProfileService.cs`
- DI registration: `MediaOrganizerApp.WebApi\Program.cs`
  - `builder.Services.AddScoped<IAiAuthorProfileService, AiAuthorProfileService>();`

### Blazor

- Wrapper: `Web.Blazor\Services\ApiWrappers\AiClientService.cs`
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

