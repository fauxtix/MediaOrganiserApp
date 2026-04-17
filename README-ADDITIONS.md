# Media Organizer App — Implementation Additions & Technical Details

This file supplements README.md without replacing it. README.md remains the primary user-facing document (unchanged). Use this file for maintainers and integrators who need implementation, API, and operations detail about features that are implemented in the code but not fully described in the original README.

Important: this document focuses on implementation details and developer-oriented examples. It assumes you are running the solution on .NET 10 and prioritizes the Blazor Server frontend (Web.Blazor) and Web API (MediaOrganizerApp.WebApi).

---

## Purpose & scope
- Preserve the original README content and user-facing description (no changes were made to README.md).
- Describe implemented features that were not fully documented, provide concrete API examples, and call out configuration details developers need to run and integrate these features.

---

## Where to look in the code (quick map)
- Web.Blazor (UI, client wrappers, pages, components)
  - Key folders: Pages, Components, Services/ApiWrappers, Controllers (Blazor-side controllers if present)
- MediaOrganizerApp.WebApi (backend controllers, AI services, helpers)
  - Key files:
    - Controllers/ConversationsController.cs
    - Controllers/AiController.cs
    - Controllers/PublicationsController.cs
    - Controllers/MediaController.cs
    - Controllers/FileManagerController.cs
    - Services/OllamaAuthorProfileService.cs
    - Services/OllamaBooksService.cs
    - Prompts/*.prompt.txt
    - Helpers/BusinessObjects/BO_*.cs (file integrity / repair helpers)
- ReportsRenderer and Web.Blazor.ReportObjects (report templates & helpers)

---

## Detailed features implemented (expanded)

1) AI / Conversations / Chat
- Files: AiController.cs, ConversationsController.cs, AiAuthorProfileService.cs, OllamaAuthorProfileService.cs, OllamaBooksService.cs, Prompts/*.prompt.txt
- Behavior:
  - The UI uses a typed client wrapper (Web.Blazor/Services/ApiWrappers/AiClientService and ConversationsClientService) to call API chat endpoints.
  - ConversationsController exposes conversation listing and streaming chat at `/api/ai/conversations` (explicit route attribute).
  - AiController exposes one-off AI endpoints at `/api/ai` (controller route attribute).
  - Ollama integration is used as an LLM fallback when third-party lookups (e.g., Google Books) return insufficient data.
- Notes / developer hints:
  - Ollama runs at http://localhost:11434 in this workspace — do not change the port without updating user-secrets/appsettings.
  - Prompts live under the Web API Prompts folder and are editable through the Blazor "Edit Prompts" admin UI.
  - The ChatRequest DTO should include a Language property to propagate culture; if it doesn't, add it or ensure clients prepend a system message with language.

2) File Manager
- Files: FileManagerController.cs (WebApi), Web.Blazor/Services/Utilities/FileManagerService.cs
- Behavior:
  - Provides endpoints for browsing server-side directories, uploading raw files, and simple file operations used by admin UI and background tasks.
  - The Blazor FileManager components call a typed client or FileManagerService rather than direct HTTP calls.
- Dev note: watch file path configuration (Storage:LocalRootPath) and ensure permissions for the app user.

3) Playlists & Media
- Files: PlaylistsBase.cs, PlaylistClientService.cs, UploadMediaFiles.razor, EditMedia.razor
- Behavior:
  - Support audio/video playlists with CRUD operations, playlist metadata, and listing UI.
  - Media pages include dynamic titles, thumbnails, preview players and a "Get Info" AI metadata button.
- Dev note: uploading large media requires correct server limits and storage configuration. Check Web API Kestrel/form size settings and storage path.

4) Document editor & viewer
- Files: Components/DocumentEditor.razor, DocumentViewerComponent.razor
- Behavior:
  - In-browser .docx editing (Syncfusion SfDocumentEditor) and a dedicated viewer with print/export support.
- Dev note: Syncfusion licensing may be required for production builds.

5) Reporting
- Files: ReportsRenderer project, Web.Blazor.ReportObjects/Helpers/ReportLists.cs
- Behavior:
  - Server-side report rendering to PDF/Excel. Blazor pages trigger report generation via the Web API or a local ReportsRenderer service.

6) Background helpers & integrity checks
- Files: Helpers/BusinessObjects/BO_*.cs, services referenced in WebApi
- Behavior:
  - File integrity checks to detect publications with missing files.
  - Retention helpers for PDFs/PNGs and cascade cleanup logic when deleting publications.

7) Admin UI & settings
- App Settings pages expose toggles for storage options, API keys, prompt editing and other admin controls.
- The code includes a Prompts editor UI used to modify prompt templates at runtime.

---

## Concrete API usage examples (curl) — copy/paste smoke tests
Notes: Update host/port if you use non-default launch profiles. These examples assume an auth cookie or Bearer token; replace <TOKEN> with a valid token or remove Authorization header if running with anonymous dev auth.

1) Create a publication (JSON)

```bash
curl -X POST "https://localhost:5001/api/publications" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Example Book","authors":[{"name":"Jane Doe"}],"year":2020}'
```

2) Upload a media file (multipart)

```bash
curl -X POST "https://localhost:5001/api/media/upload" \
  -H "Authorization: Bearer <TOKEN>" \
  -F "file=@/path/to/file.mp3" \
  -F "title=My Track"
```

3) Start a conversation (AI) — simple POST to start /api/ai/conversations

```bash
curl -X POST "https://localhost:5001/api/ai/conversations" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Author lookup for 'Jane Doe'","messages":[{"role":"user","content":"Find author profile for Jane Doe"}],"language":"en-US"}'
```

4) Ask the AiController for an author profile (fallback)

```bash
curl -X POST "https://localhost:5001/api/ai/authorprofile" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","source":"fallback","language":"en-US"}'
```

Adjust route names for your build if controller route attributes differ; check the controller classes for authoritative route attributes (see Code vs README guidelines).

---

## Configuration & secrets (developer tips)
- Ollama: default base URL `http://localhost:11434` — set `Ollama:BaseUrl` in WebApi user-secrets or appsettings.
- OpenAI: if used as a fallback, store `OpenAI:ApiKey` in the Web API user secrets (Web.Blazor also stores a key for client features).
- DB: `ConnectionStrings:DefaultConnection` handles the SQL Server database.

PowerShell example to set Web API secrets (replace values):

```powershell
$proj = ".\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj"

dotnet user-secrets --project $proj set "ConnectionStrings:DefaultConnection" "Server=.;Database=Publicacoes_Prod;Trusted_Connection=True;"
dotnet user-secrets --project $proj set "Ollama:BaseUrl" "http://localhost:11434"
```

---

## Suggested next additions (optional, high ROI)
- Add quick API curl examples to the primary README (README.md) for public visibility.
- Add a Mermaid diagram near the top of README.md describing components and data flows.
- Add screenshot files in `/docs/screenshots/` and reference them from README.md.

---

## Notes about modifications
- README.md was intentionally left intact; this file is additive.
- If you prefer a different filename or want these additions merged back into README.md, tell me and I will prepare a merge patch — I will not modify README.md without explicit confirmation.

---

Created for maintainers: keeps code documentation separate from the public-facing README content. If you want these additions moved into README.md or published as `CONTRIBUTING.md` or `DEVELOPER.md`, say which one and I will prepare the exact file replacement.
