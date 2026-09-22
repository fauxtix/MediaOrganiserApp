# AGENTS.md

## Build & Run

```bash
dotnet restore MediaOrganizerApp.sln
dotnet build MediaOrganizerApp.sln
dotnet test MediaOrganizerApp.sln
```

Run the full app locally: `scripts/start-all.ps1` (launches WebApi, then Web.Blazor).

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
| `Web.Blazor.Reporting` | RDLC report templates & helpers (`Web.Blazor.ReportObjects/`) | net10.0 |
| `PromptEditor` | Windows Forms prompt editor tool (standalone, not in solution) | net10.0-windows |

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

**Two-layer architecture: OpenRouter (cloud) is the PRIMARY provider; Ollama (local) is the FALLBACK.** For translate, media notes/metadata/edition-year, author-books, book-summary and author-profile, the Blazor UI first calls the `*-cloud` WebApi endpoints (OpenRouter); when they return null/503 (offline, no API key, 429, HTTP/parse error), it falls back to the local Ollama endpoints (`/api/ai/translate`, `/api/ai/media-notes`, `/api/ai/media-metadata`, `/api/ai/author-books`, `/api/ai/book-summary|book-summary-only`, `/api/ai/author-profile`). Every Cloud service (`CloudTranslateService`, `CloudMediaService`, `CloudBookSummaryService`, `CloudAuthorBooksService`, `CloudAuthorProfileService`, `OpenRouterChatService`) is designed to return null on failure so the caller falls back to Ollama. The "Pesquisar Books Online/AI" flow (`Web.Blazor/Pages/Publications/GoogleBooksApiSearch.razor.cs`) follows: Google Books API first (query with `intitle:"..."`/`inauthor:"..."` phrase syntax + one translated-title retry); when a found volume lacks `volumeInfo.description`, its `searchInfo.textSnippet` is used as `Resumo` (then MyMemory-translated) before any AI; AI (`book-summary-cloud`→`book-summary`, OpenRouter→Ollama) is invoked only when Google Books returns no items at all, or items without both `description` and `textSnippet`.

- **Local (Ollama) services** (`MediaOrganizerApp.WebApi/Services/Ollama*Service.cs`) talk to `http://localhost:11434` (hardcoded in `Program.cs` named client `"OllamaSharedClient"`). The active local model (`ModelBaseUrl` + `AiModelName`) is stored in the DB (`ApplicationSettings` via `AiSettingsProvider`, 60s cache) — not from an `Ollama:BaseUrl` config key. `Web.Blazor` also connects to Ollama directly via Microsoft.Extensions.AI (`IChatClient`/`OllamaApiClient`) for the `AiChat` page.
- **`Ai:Provider` config key only switches the LOCAL fallback `IAiAuthorProfileService`** in `ServiceRegistration.cs`: `Ollama` → `OllamaAuthorProfileService`, anything else → OpenAI-SDK `AiAuthorProfileService` (pointed at the DB `ModelBaseUrl`). Author-profile itself is cloud-first (see below): `/api/ai/author-profile-cloud` (OpenRouter) is called first and falls back to the local `/api/ai/author-profile`. It does NOT control the other AI services — those are wired in `Program.cs` (local = Ollama, cloud = OpenRouter).
- **OpenRouter / cloud-chat settings live in the DB** (keys in `Application/Configurations/CloudChatSettingsKeys.cs`): `CloudChat:OpenRouterApiKey`, `CloudChat:DefaultModel` (default `openrouter/free`), `CloudChat:EnableReasoning`, `CloudChat:AllowPaidModels`, `CloudChat:FreeDailyCap`, `CloudChat:AuthorBooksUseCloud`, `CloudChat:ExtractionModels`, `CloudChat:EverPurchasedCredits` (snapshot interno, não editar). Configured via the **"Cloud Chat" tab of `Web.Blazor/Pages/Admin/AppSettings.razor`** (admin-only: API key, reasoning/paid-models/author-books toggles, model picker from the live catalog, free-daily cap, usage cards).

**Settings pages (admin):** `AppSettings.razor` (`/admin/appsettings`) is the ORIGINAL page (footer link `SidebarData.cs:71` points here; unchanged). `AppSettings2.razor` (`/admin/appsettings2`, same folder) is the REFACTORED version of the same settings screen — Dribbble-inspired layout with a custom icon sidebar ("Pastas", "Outras Definições", "APIs & URLs", "Cloud Chat" groups) + content cards on the right, full CloudChat port, dark-theme tokenized (`var(--app-*)`), no `SfTreeView`. It is reachable only by direct URL (not linked in the sidebar). Keep both in sync when settings change; new resx keys for its section labels live in `App.resx`/`App.en.resx`/`App.fr.resx`/`App.es.resx`. **Lookup tables admin:** `ManageLookupTables.razor` (`/admin/managelookuptables`) is the ORIGINAL tabbed page (7 `SfTab` tabs embedding the self-contained components from `Web.Blazor/Pages/LookupTables/`). `ManageLookupTablesNew.razor` (`/admin/managelookuptables-new`, same folder) is a REFACTORED version of the same screen using the AppSettings2 template — icon sidebar with collapsible groups ("Publicações", "Média", "IA") + content card showing the selected lookup component; reuses the exact same `LookupTables` components (no logic duplication), dark-theme tokenized (`var(--app-*)`). Reachable only by direct URL (not linked in the sidebar). Keep both in sync; group/format labels use existing resx keys plus `TituloGrupoPublicacoes`/`TituloGrupoMedia`/`TituloGrupoIA`/`frmFormatosMedia` in the four `App*.resx`.
- **Free vs paid models:** the `AllowPaidModels` toggle MUST be honoured server-side. `OpenRouterCatalogService.GetFreeModelsAsync` exposes free models only unless the toggle is on; `CloudChatSettingsKeys.ApplyPaidModelGate` downgrades a paid recommended model to its `:free` variant (or `openrouter/free`). All cloud services must pass the resolved model through the gate before sweeping candidates (`BuildCandidateModelsAsync`): translate, media-notes, media-metadata, edition-year, book-summary and author-profile use `FreeModelsAsync` (live catalog) when the toggle is off; author-books likewise. Free tier usage is **counted and ENFORCED** via `CloudChatUsageService` (`CloudChat:FreeUsage:yyyyMMdd`, default cap 50/day without credits, 1000/day with credits, overridable with `CloudChat:FreeDailyCap`). Every free-model call first checks `ICloudChatUsageService.GetFreeRequestsRemainingAsync` (cap minus today's counter, resolved without network: `FreeDailyCap` override → `CloudChat:EverPurchasedCredits` snapshot persisted by `GetUsageAsync` → default 50); when 0, the cloud services (`CloudTranslate/Media/BookSummary/AuthorBooks/AuthorProfile`) return null so the caller falls back to Ollama, and `OpenRouterChatService` throws a friendly `CloudProviderTransientException` (cloud chat has no local fallback).
- **Cloud chat** (`Web.Blazor/Pages/AI/OpenRouterChat.razor` → `/api/cloud-chat/stream` → `OpenRouterChatService`) streams from OpenRouter only (no Ollama fallback; on 404 it retries `openrouter/free`, on reasoning-unsupported it retries without reasoning). When the effective model is the router `openrouter/free` and a candidate ends without producing text, `OpenRouterChatService` auto-sweeps to the next candidate (`BuildChatCandidatesAsync`): paid recommended prefixes when `AllowPaidModels` is on, otherwise the LIVE free catalog (recommended ones first, max 12) — freely-chosen free/paid models and HTTP/404/429 failures are never swept. Local chat (`Chat.razor` → `/api/ai/chat` → `OllamaChatService`) is a local-only Ollama page (`/ai/chat`, exception to cloud-first). `AiChat` (`/aichat`, direct `IChatClient`/`OllamaApiClient`) and `CopilotChat` (`/ai/copilot`) exist but are disabled in the sidebar (`Web.Blazor/Components/Layout/SideBar/SidebarData.cs:61-62`). Author-profile is now cloud-first: `EditAuthor` → `AiClientService.GetAuthorProfileAsync` → `/api/ai/author-profile-cloud` (OpenRouter) with fallback to `/api/ai/author-profile` (`Scenario = "cloud-fallback"`); the local `/api/ai/author-profile` uses Ollama or the OpenAI-SDK provider pointed at the DB `ModelBaseUrl`.
- **Chat feedback (👍/👎 + copiar, como no ChatGPT)**: ambos os chats (OpenRouter + Ollama) permitem copiar a resposta do assistente para o clipboard (`window.copyText` em `wwwroot/js/app.js`) e avaliá-la gosto/não gosto (toggle, botões 👍/👎 por mensagem). As avaliações persistem server-side na tabela `ChatFeedback` via `scripts/CreateChatFeedback.sql` (procs `usp_ChatFeedback_Upsert`/`usp_ChatFeedback_GetByConversation`), expostas por `POST /api/chat-feedback` e `GET /api/chat-feedback/by-conversation` (`ChatFeedbackController`, isolamento por `X-UserId`), consumidas pelo `ChatFeedbackClientService` (Web.Blazor, registado via `AddHttpClient`). Chave estável = (UserID, Interface `'openrouter'|'ollama'`, ConversationId, MessageIndex do assistente); o estado visual fica em memória (`_feedbackByIndex` nos code-behinds) e o JSON persistido das `Conversations` não é alterado. Cada resposta mostra uma etiqueta do estado persistido (`ChatVote*`: «Aprovo»/«Não aprovo»/«Não votado»), restaurada ao carregar a conversa. `Rating = 0` persiste a desmarcação (a linha mantém-se com Rating 0 na BD); respostas nunca votadas não têm linha.
- **Internal AI usage logging (non-chat):** every non-chat AI operation from within the app (translate, media-notes, media-metadata, edition-year, author-books, book-summary, author-profile) is auto-recorded **server-side in `AiController`** into the `AIUsageLog` table via `AiUsageRecorderService` (`WebApi/Services/AiUsageRecorderService.cs`) — one row per call with `Provider` (`openrouter`/`ollama`/`openai-sdk`), `Operacao`, `Contexto`, `Scenario`, `DurationMs`, `Sucesso`, `EraFallback` (bit, 1 when the local Ollama provider was used), `UserID`. `Scenario` comes from the request DTOs/query (`cloud-fallback` set by the Blazor wrappers when a cloud call failed and the local provider is used; `googlebooks`/`author-api`/`media-api` tagged at the page call sites; default `direct`). Chat stays in `AIResponses` (unchanged). Dashboard reads `GET /api/ai-usage/summary` → `AiUsageClientService` ("IA em uso interno" section in `Index.razor`); table created by `scripts/CreateAIUsageLog.sql`. Admin sees all users, GeneralUser only their own (`X-UserId`/`X-IsAdmin`).

Prompt templates live in `MediaOrganizerApp.WebApi/Prompts/*.prompt.txt` and are copied to output on build (`CopyToOutputDirectory: Always`). They are also editable at runtime via the "Edit Prompts" Blazor admin page.

**Workflow for any multi-step change:** before implementing any non-trivial multi-step change (AI flows, refactors, project/file removals — any work whose mid-way interruption would lose continuity), first write a transient markdown plan at the repo root (e.g. `PLAN_<TOPIC>.md`) with a **progress checklist** (`- [x]`/`- [ ]`) stating the status of each step, and present it to the user. **Implement only after the user confirms.** **Update the checklist as work progresses**, marking each step done. The plan file is a local working doc and must **not be committed**; delete it only when the work is finished and committed — never before. This ensures that if assistant credits run out mid-way, the plan still records exactly what is done, what remains, and where to resume. If the investigation finds nothing to change, don't create the plan and report the finding instead.

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
- The sidebar (`wwwroot/css/sidebarpro/SideBarStyle.css`) is **theme-independent**: it must look identical in light and dark mode (same as the dark-mode palette). It uses dedicated `--sidebar-*` tokens (defined in `theme-tokens.css`, fixed in `:root` and intentionally NOT redefined under `[data-theme="dark"]`). Do NOT use `var(--app-*)` here (those flip with the theme and would make the sidebar white in light mode). `theme-tokens.css` (loaded LAST) also re-asserts the sidebar surfaces (`.sidebar`, `.sidebar-layout`, `.sidebar-header/content/footer`, `.sub-menu-list`, menu item links) with `--sidebar-*` so no stale/earlier stylesheet can paint the menu white in light mode — keep that block in sync with `SideBarStyle.css`.
- The **top bar / header** (`MainLayout.razor` `.top-navbar`, `.brand`, `.nav-link`; `ThemeToggle.razor`) is likewise **theme-independent dark**: use the dedicated `--header-*` tokens (`--header-bg`, `--header-text`, `--header-border`, `--header-hover-bg`), fixed in `:root` and NOT redefined under `[data-theme="dark"]`. Never use `--app-header-bg`/`--app-header-text` for the app chrome (they are white in light mode), and never hardcode the toggle icon color (it becomes invisible on the dark bar).
- Dark-adapted so far (status as of 2026-09-16): theme-tokens.css, MainLayout (incl. top-navbar), site.css, PageTitleComponent, Dashboard (Grid/Card), Image/Video galleries, MusicGallery (polaroid), Media, AiChat (container/messages/scrollbar), Chat, CopilotChat, CultureSwitcher, Publicacoes, EditPublicacoes, EditMedia, EditYouTube, Playlists, YoutubePlaylists (thumb), ArtistsPublishedTitles, PubsNotFoundOnDisk, PubsWithWrongPath, GoogleBooksApiSearch, ManageLookupTables (PageTitle added), SelectAuthor dialog, AppSettings + AppSettings2 (refactored: sidebar ícones + cards tokenizados, CloudChat portado) + ConfigurationTablesView, LookupTableComponent, EditPromptFiles (README pane), NoDataAvailable, TranslateText, LoadingData, AuthorBooks (incl. AI-results table), AuthorPreview, AuthorReadOnly, Autores (toolbar), EditAuthor (upload/progress/polaroid), MediaGrid, ThemeToggle and both Identity `_Layout.cshtml` pages. Dashboard pie charts (`Index.razor`): o tooltip deixa um retângulo preto "fantasma" no dark-mode após sair do gráfico — artefacto de composição GPU do Chrome (Chromium mantém a tile da última pintura do tooltip; os `<svg>` de `[..._tooltip]` já foram removidos do DOM, por isso o artefacto não é um elemento e CSS não o resolve). Fix em vigor: `wwwroot/js/fix-chart-tooltip-ghost.js` — MutationObserver que, ao remover um `<svg>` de um elemento `[id$="_tooltip"]` dentro de `.e-accumulationchart`, força repaint do container do gráfico (toggle `transform: translateZ(0)` via rAF). Loaded from `App.razor`. `FadeOutDuration="0"` NÃO funciona — quebra o ocultar do tooltip por completo (o fade é o próprio ocultar).

## Conventions

- **ORM mix**: Dapper for most queries, EF Core for Identity and some persistence. Raw SQL strings are common in repositories (e.g., `StringBuilder`-based query building).
- **Localization**: Default culture is `pt` (Portuguese). Supported: en, fr, pt, es. Resources in `Web.Blazor/Resources/`.
- **Syncfusion**: UI components use Syncfusion Blazor suite. Requires a license key (hardcoded in `Web.Blazor/Program.cs`).
- **Reports**: RDLC templates in `Reports_RDLC/` directories, shipped with `MediaOrganizerApp.WebApi`. The WebApi renders them locally via `ReportViewerCore.NETCore` (`ReportController.cs`).
- **Service registration**: register services that depend on `HttpClient` via `AddHttpClient` in `ServiceRegistration.cs` (`AddServicesInfrastructure`) — never directly in `Program.cs`; persistence services go in `AddPersistenceInfrastructure`. Blazor API clients are registered via `AddHttpClient` in `Web.Blazor`'s `ServiceRegistration`. UI components should not call API endpoints directly — use typed API client wrappers reading `BaseUrl` and the `ApiKey` header from configuration.
- **SignalR**: all SignalR/Hub work lives in the service layer, not the UI.
- **No CI/CD config** in use: `.github/workflows/*.yml` exist but are stale (reference non-existent projects). No pre-commit hooks.
- **No linting/formatting enforcement** beyond the single `.editorconfig` rule (CA1051 suppressed).

## Gotchas

- `Web.Blazor.csproj` explicitly disables `GenerateAssemblyInfo` and `GenerateTargetFrameworkAttribute` to avoid duplicates — don't re-enable without checking.
- The `Web.Blazor` project excludes `Services/ApiRelated/**` from compilation — dead code kept for reference.
- All projects in the solution target **net10.0** (only `PromptEditor` is `net10.0-windows` and is not part of any solution).
- `nuget.config` includes a Syncfusion NuGet source with a placeholder API key (`<your-api-key>`). If restore fails, check this.
- Serilog writes to SQL Server (`AppLogs` table) — `autoCreateSqlTable: true` in config. The table is auto-created on first write.
- AI Ollama client uses `SocketsHttpHandler` with aggressive keepalive to handle long-running LLM inference requests (up to 10-15 minute timeouts).
- **Running the app locks `Web.Blazor.dll`** (VS + IIS Express). `dotnet build` will fail with `MSB3027/MSB3021` on the copy step while the app is running — the compilation itself succeeds; just stop the app or build a single project to verify. Do not mistake these for code errors.
