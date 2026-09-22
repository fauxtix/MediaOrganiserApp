# AI Fallback (Author Profile) — Testing Steps

This file describes the **steps to test** the AI fallback that was implemented for author profiles.

Scope:
- Blazor UI: `Web.Blazor`
- Web API backend: `MediaOrganizerApp.WebApi`
- AI fallback rule: **external API first; AI only when external lookup returns empty/fails**

---

## 1) Pre-requisites

### 1.1 Web API configuration
The Web API must have the OpenAI settings configured (Development uses user-secrets via `AddUserSecrets<Program>()`).

Required keys:
- `OpenAI:ApiKey`
- `OpenAI:Model` (optional; default used by code is `gpt-4o-mini`)

### 1.2 API key middleware
All calls to the Web API are protected by `ApiKeyMiddleware`.

You must have a valid value configured and sent via request header:
- Header name: `ApiKey`

This is already done by the Blazor wrappers (including `AiClientService`).

---

## 2) Build verification

1. Build the whole solution (should succeed).
2. If build fails with missing `OpenAI.*` namespaces, confirm the NuGet package `OpenAI` is installed in `MediaOrganizerApp.WebApi`.

---

## 3) Web API smoke-test (AI endpoint)

### 3.1 Run the Web API
1. Set `MediaOrganizerApp.WebApi` as Startup Project.
2. Run it (HTTPS).
3. Confirm it starts without an exception like “Missing configuration key: OpenAI:ApiKey”.

### 3.2 Call the AI endpoint directly
Use PowerShell (adjust port to your actual Web API HTTPS port):

```powershell
$baseUrl = "https://localhost:44327"  # adjust if different
$apiKey  = "<YOUR_API_KEY>"           # same value your other clients use

$headers = @{ ApiKey = $apiKey }
$body    = @{ name = "José Saramago" } | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "$baseUrl/api/Ai/author-profile" -Headers $headers -ContentType "application/json" -Body $body
```

The endpoint above is the local fallback. The cloud-first endpoint is `POST .../api/Ai/author-profile-cloud` (same body); it returns **HTTP 503** when OpenRouter is unavailable (offline, no key, 429, HTTP/parse failure) — exactly the trigger that makes the Blazor wrapper fall back to `/api/Ai/author-profile` with `scenario: cloud-fallback`.

Expected result:
- HTTP 200
- JSON response containing at least `bio` (may also contain `photoUrl`, `confidence`, `notes` depending on model output)

Failure modes:
- 401/403 (or middleware rejection): wrong/missing `ApiKey` header
- 500 at startup: missing `OpenAI:ApiKey`

---

## 4) End-to-end test (Blazor → external API → AI fallback)

### 4.1 Run both projects
1. Start `MediaOrganizerApp.WebApi`.
2. Start `Web.Blazor`.

### 4.2 Test scenario A: external API SUCCESS (AI should NOT run)
1. Open the UI that hosts `EditAuthor`.
2. Select an author name that you know the external API can resolve.
3. Trigger the action that calls `GetAuthorDataFromApi()`.

Expected:
- `SelectedAuthor.Sobre` is filled from the external API result.
- You do **not** see the message: `Bio gerada por AI (fallback).`

### 4.3 Test scenario B: external API FAIL (AI fallback SHOULD run)
1. Use an author name that the external API normally fails to resolve.
2. Trigger `GetAuthorDataFromApi()`.

Expected:
- `SelectedAuthor.Sobre` is filled from AI.
- You see a visible UI message (current implementation uses `message` + `alertVisibility`):
  - `Bio gerada por AI (fallback).`

### 4.4 Test scenario C: external FAIL + AI FAIL
1. Temporarily break AI config (e.g., remove `OpenAI:ApiKey`) **or** stop the Web API.
2. Trigger `GetAuthorDataFromApi()` with an author that fails external lookup.

Expected:
- UI displays:
  - `Não foi possível obter detalhes do autor (API e AI).`

---

## 5) Quick checklist of files involved

- Blazor fallback call site:
  - `Web.Blazor\Pages\LookupTables\EditAuthor.razor.cs`
- Blazor wrapper for AI endpoint:
  - `Web.Blazor\Services\ApiWrappers\AiClientService.cs`
- Web API endpoint:
  - `MediaOrganizerApp.WebApi\Controllers\AiController.cs`
- Web API service (LLM call):
  - `MediaOrganizerApp.WebApi\Services\AiAuthorProfileService.cs`

---

## Notes

- AI output is best-effort. If you need higher factual accuracy, add retrieval/citations (RAG) or a second “verify” step.
