# AGENTS.md

## Build & Run

```bash
dotnet restore MediaOrganizerApp.sln
dotnet build MediaOrganizerApp.sln
dotnet test MediaOrganizerApp.sln
```

Run the full app locally: `scripts/start-all.ps1` (launches ReportsRenderer, then WebApi, then Web.Blazor).

**Port conflict**: both WebApi and Web.Blazor default to `https://localhost:5001`. Run them with separate launch profiles or override ports. The `start-all.ps1` script uses default profiles and expects you to handle this.

## Project Layout

| Project | Role | TFM |
|---|---|---|
| `MediaOrganizerApp.WebApi` | REST API backend (controllers, AI services, Ollama integration) | net10.0 |
| `Web.Blazor` | Blazor Server frontend (UI, Syncfusion components, SignalR client) | net10.0 |
| `MediaOrganizerApp.Application` | Interfaces, DTOs, validation (FluentValidation), Dapper queries | net10.0 |
| `MediaOrganizerApp.Domain` | Domain entities | net10.0 |
| `MediaOrganizerApp.Persistence` | EF Core + Dapper repositories, data access | net10.0 |
| `MediaOrganizerApp.Identity` | ASP.NET Core Identity setup | net10.0 |
| `MediaOrganizerApp.EmailService` | FluentEmail-based email sending | net10.0 |
| `MediaOrganizerApp.Tests` | xUnit + Moq tests | net10.0 |
| `ReportsRenderer` | Standalone RDLC report rendering microservice | **net7.0** |
| `MediaOrganizerApp.Shared` | Shared utilities | **net7.0** |
| `MediaOrganizerApp.YouTubeNotificationApi` | YouTube notification microservice | **net7.0** |
| `Infrastructure` | Legacy, currently unused | **net5.0** |
| `PromptEditor` | Windows Forms prompt editor tool | net10.0-windows |

**Two independent processes**: WebApi and Web.Blazor run separately. Web.Blazor calls WebApi over HTTP using `BaseUrl` in its `appsettings.json` (defaults to `http://localhost:4100/api` in production, read from config in dev).

## Database

SQL Server. Database name: `Publicacoes_Prod`. Connection string in `appsettings.json` under `ConnectionStrings:SqlServerCon`.

Secrets (API keys, connection strings) use .NET user-secrets:
```bash
dotnet user-secrets --project MediaOrganizerApp.WebApi set "ConnectionStrings:SqlServerCon" "..."
dotnet user-secrets --project MediaOrganizerApp.WebApi set "Ollama:BaseUrl" "http://localhost:11434"
```

User-secrets IDs: WebApi = `0682d17f-5198-4837-aa99-f93b8f85796d`, Web.Blazor = `aspnet-Web.Blazor-82599089-8959-419C-85C2-F04A9D8841F2`.

## Tests

Framework: **xUnit** with **Moq**. Test project: `MediaOrganizerApp.Tests`.

Run all tests: `dotnet test MediaOrganizerApp.Tests`

Run a single test: `dotnet test MediaOrganizerApp.Tests --filter "FullyQualifiedName~TranslateAsync_Parses_EmbeddedJsonInsideMarkdownFence"`

Tests mock HTTP calls (no live Ollama/DB required). Pattern: create a `StubHttpMessageHandler` with canned responses, wire up mocks with Moq.

## AI Integration

**Two-layer architecture: OpenRouter (cloud) is the PRIMARY provider; Ollama (local) is the FALLBACK.** For translate, media notes/metadata/edition-year and author-books, the Blazor UI first calls the `*-cloud` WebApi endpoints (OpenRouter); when they return null/503 (offline, no API key, 429, HTTP/parse error), it falls back to the local Ollama endpoints (`/api/ai/translate`, `/api/ai/media-notes`, `/api/ai/media-metadata`, `/api/ai/author-books`). Every Cloud service (`CloudTranslateService`, `CloudMediaService`, `CloudAuthorBooksService`, `OpenRouterChatService`) is designed to return null on failure so the caller falls back to Ollama.

- **Local (Ollama) services** (`MediaOrganizerApp.WebApi/Services/Ollama*Service.cs`) talk to `http://localhost:11434` (hardcoded in `Program.cs` named client `"OllamaSharedClient"`). The active local model (`ModelBaseUrl` + `AiModelName`) is stored in the DB (`ApplicationSettings` via `AiSettingsProvider`, 60s cache) — not from an `Ollama:BaseUrl` config key. `Web.Blazor` also connects to Ollama directly via Microsoft.Extensions.AI (`IChatClient`/`OllamaApiClient`) for the `AiChat` page.
- **`Ai:Provider` config key only switches `IAiAuthorProfileService`** in `ServiceRegistration.cs`: `Ollama` → `OllamaAuthorProfileService`, anything else → OpenAI-SDK `AiAuthorProfileService` (pointed at the DB `ModelBaseUrl`). It does NOT control the other AI services — those are wired in `Program.cs` (local = Ollama, cloud = OpenRouter).
- **OpenRouter / cloud-chat settings live in the DB** (keys in `Application/Configurations/CloudChatSettingsKeys.cs`): `CloudChat:OpenRouterApiKey`, `CloudChat:DefaultModel` (default `openrouter/free`), `CloudChat:EnableReasoning`, `CloudChat:AllowPaidModels`, `CloudChat:FreeDailyCap`, `CloudChat:AuthorBooksUseCloud`, `CloudChat:ExtractionModels`. Configured via the **"Cloud Chat" tab of `Web.Blazor/Pages/Admin/AppSettings.razor`** (admin-only: API key, reasoning/paid-models/author-books toggles, model picker from the live catalog, free-daily cap, usage cards).
- **Free vs paid models:** the `AllowPaidModels` toggle MUST be honoured server-side. `OpenRouterCatalogService.GetFreeModelsAsync` exposes free models only unless the toggle is on; `CloudChatSettingsKeys.ApplyPaidModelGate` downgrades a paid recommended model to its `:free` variant (or `openrouter/free`). All cloud services must pass the resolved model through the gate before sweeping candidates (`BuildCandidateModelsAsync`): translate, media-notes, media-metadata and edition-year use `FreeModelsAsync` (live catalog) when the toggle is off; author-books likewise. Free tier usage is counted/limited via `CloudChatUsageService` (`CloudChat:FreeUsage:yyyyMMdd`, default cap 50/day without credits, 1000/day with credits, overridable with `CloudChat:FreeDailyCap`).
- **Cloud chat** (`TestChat.razor` → `/api/cloud-chat/stream` → `OpenRouterChatService`) streams from OpenRouter only (no Ollama fallback; on 404 it retries `openrouter/free`, on reasoning-unsupported it retries without reasoning). Local chat (`Chat.razor`/`CopilotChat.razor`) uses local Ollama (`AiClientService` → `/api/ai/chat` → `OllamaChatService`).

Prompt templates live in `MediaOrganizerApp.WebApi/Prompts/*.prompt.txt` and are copied to output on build (`CopyToOutputDirectory: Always`). They are also editable at runtime via the "Edit Prompts" Blazor admin page.

**Workflow for AI changes:** before implementing any change to the AI flows (Media/Publicações/AuthorBooks — anything that can consume OpenRouter credits), first write the proposed steps to a markdown plan at the repo root (e.g. `PLAN_AI_MEDIA_FIXES.md`) and present it to the user. **Implement only after the user confirms.** This avoids burning daily/paid credits while work is still being planned. If the investigation finds nothing to change, don't create the plan and report the finding instead. The plan file is a transient working doc: delete/replace it once the work is done — do NOT keep completed plans in the repo.

**Documentation drift rule:** live docs (AGENTS.md, `Web.Blazor/docs/ai-fallback-*.md`, `ollama-local-setup.md`, `docs/architecture/USER_ROLE_DATA_FLOW.md`, `media_organizer_arquitetura_visao_geral.md`) must be updated in the same pass as the code change that affects them — never in a separate request. Do NOT reintroduce one-shot audit/history/plan docs into master.

## Data Isolation

Two roles: **Admin** (sees all data) and **GeneralUser** (sees only own data). Data isolation is enforced at the **repository level** via `UserID` filtering, not at the UI layer. When modifying queries in repositories, ensure the `UserRole` check pattern is preserved.

## Theme System (Light/Dark)

App-wide theming uses CSS custom properties (tokens), **NOT** Syncfusion theme packages.

- Single source of truth: `Web.Blazor/wwwroot/css/theme-tokens.css` — reference it LAST in `<head>` (see `App.razor` and both Identity `_Layout.cshtml` pages).
- Mechanism: `[data-theme]` attribute on `<html>`, `localStorage['theme']`, `themeToggle.js` (toggle without FOUC).
- Endpoint palettes:
  - **Light**: bg `#F8FAFC`, surface `#FFFFFF`, text `#0F172A`/`#475569`, CTA `#2563EB`, border `#E2E8F0`.
  - **Dark**: bg `#0F172A`, surface `#1E293B`, text `#F8FAFC`/`#94A3B8`, CTA `#3B82F6`, border `#334155`.
- Core tokens: `--app-bg`, `--app-surface`, `--app-surface-alt`, `--app-surface-hover`, `--app-text-primary`, `--app-text-secondary`, `--app-primary`, `--app-primary-hover`, `--app-on-primary`, `--app-border`, `--app-header-bg`, `--app-header-text`, `--app-shadow`.
- Accent/status tokens (Directriz UI/UX — Esmeralda `#10B981` / Âmbar `#F59E0B`, luminosidade adaptada ao tema; pares verificados >= 4.5:1 WCAG AA): `--app-accent-green(-hover/-strong)`, `--app-on-accent-green`, `--app-accent-yellow(-hover)`, `--app-on-accent-yellow`, `--app-success-bg/-text/-border`, `--app-warning-bg/-text/-border`, `--app-danger-bg/-text/-border/--app-danger(-hover)`, `--app-on-danger`, `--app-info-bg/-text/-border`.

**Rules for editing CSS in this repo:**
- Never hardcode colors in scoped CSS or inline styles — always use `var(--app-*)` tokens.
- For mixed light/dark rules, write the base rule (light) then `[data-theme="dark"]` overrides below it.
- To dark-adapt a Syncfusion component, add `[data-theme="dark"] .e-<component> { ... }` CSS overrides — do NOT switch to a Syncfusion dark theme.
- Kept accents (deliberate): gradients on dashboard cards, `--app-accent-yellow` (player selection/hover/seleção), red/amber modals (deletion) via `--app-danger`/`--app-accent-yellow`, MP3/YouTube players stay self-contained, `steelblue`/`slategray` remain in legacy spots. Do NOT reintroduce raw `#FFC107`/`#eeb711`/`#10B981`-style hexes — use the accent tokens above.
- Dark-adapted so far (status as of 2026-09-16): theme-tokens.css, MainLayout (incl. top-navbar), Sidebar, site.css, PageTitleComponent, Dashboard (Grid/Card), Image/Video galleries, MusicGallery (polaroid), Media, AiChat (container/messages/scrollbar), Chat, CopilotChat, CultureSwitcher, Publicacoes, EditPublicacoes, EditMedia, EditYouTube, Playlists, YoutubePlaylists (thumb), ArtistsPublishedTitles, PubsNotFoundOnDisk, PubsWithWrongPath, GoogleBooksApiSearch, ManageLookupTables (PageTitle added), SelectAuthor dialog, AppSettings + AppSettings2 (card-box) + ConfigurationTablesView, LookupTableComponent, EditPromptFiles (README pane), NoDataAvailable, TranslateText, LoadingData, AuthorBooks (incl. AI-results table), AuthorPreview, AuthorReadOnly, Autores (toolbar), EditAuthor (upload/progress/polaroid), MediaGrid, ThemeToggle and both Identity `_Layout.cshtml` pages.

## Conventions

- **ORM mix**: Dapper for most queries, EF Core for Identity and some persistence. Raw SQL strings are common in repositories (e.g., `StringBuilder`-based query building).
- **Localization**: Default culture is `pt` (Portuguese). Supported: en, fr, pt, es. Resources in `Web.Blazor/Resources/`.
- **Syncfusion**: UI components use Syncfusion Blazor suite. Requires a license key (hardcoded in `Web.Blazor/Program.cs`).
- **Reports**: RDLC templates in `Reports_RDLC/` directories. `ReportsRenderer` is a separate microservice that renders them.
- **No CI/CD config** found in the repo. No pre-commit hooks.
- **No linting/formatting enforcement** beyond the single `.editorconfig` rule (CA1051 suppressed).

## Gotchas

- `Web.Blazor.csproj` explicitly disables `GenerateAssemblyInfo` and `GenerateTargetFrameworkAttribute` to avoid duplicates — don't re-enable without checking.
- The `Web.Blazor` project excludes `Services/ApiRelated/**` from compilation — dead code kept for reference.
- `MediaOrganizerApp.Shared` and `ReportsRenderer` are still on **net7.0** while the main app targets net10.0 — cross-project references work but watch for API surface mismatches.
- `nuget.config` includes a Syncfusion NuGet source with a placeholder API key (`<your-api-key>`). If restore fails, check this.
- Serilog writes to SQL Server (`AppLogs` table) — `autoCreateSqlTable: true` in config. The table is auto-created on first write.
- AI Ollama client uses `SocketsHttpHandler` with aggressive keepalive to handle long-running LLM inference requests (up to 10-15 minute timeouts).
- **Running the app locks `Web.Blazor.dll`** (VS + IIS Express). `dotnet build` will fail with `MSB3027/MSB3021` on the copy step while the app is running — the compilation itself succeeds; just stop the app or build a single project to verify. Do not mistake these for code errors.
