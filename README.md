# 📚 Media Organiser App

**Multi-User Collection Management System**

<img width="800" height="600" alt="MediaOrganiser_en" src="https://github.com/user-attachments/assets/bc258e0e-a461-4b9b-bd7c-20cbf9a4ea3c" />

---

[Versão em Português](README-PT.md)

Have you ever felt frustrated managing hundreds of media files scattered across different storage mediums and hard disks? Wouldn't it be great to have them organized and easy to manage?

Media Organizer App is a **multi-user application** to organize your media (videos and MP3 files) and documents (PDFs and Word documents).
It also allows you to organize collections of books, LPs, CDs, DVDs, and documents written on paper (filing cabinets, for example).

This application is designed to streamline the organization, management, and retrieval of media and publication data with **role-based access control**, **user-specific data isolation**, and a robust backend.

**Perfect for:** personal collections, families, small libraries, archive projects, collector teams, or any scenario where multiple users need to maintain separate catalogs in a shared system.

> 📊 **[View Interactive Architecture Diagrams](docs/architecture/USER_ROLE_DATA_FLOW.md)** — see how data isolation and role-based access control work.

---

## ✨ Key Features

### 📚 Publications Management
- **CRUD operations**: create, read, update and delete publications with detailed metadata.
- **Bulk import/upload**:
  - Upload publications in bulk using PDF, Excel or Word files.
  - Automatic and manual data mapping.
  - Validation for required fields, duplicate detection and error reporting.
- **Search & filter**: advanced search by title, author, category or keywords.
- **Duplicate detection**: automatically flag duplicate publications based on metadata.
- **Reports**: generate detailed reports for publications, including statistics by category, author or year (PDF/Excel).
- **Google Books API search**: integrated search for book metadata.
- **Author's books view**: view an author's books directly from the publication editor and from the author details page (Google Books, Wikidata and AI fallback when necessary).
  - From the publication editor: open a publication in `Publications` and click the author lookup/search button next to the author field to open the author dialog; it shows author details and an "Author Books" list with thumbnails, titles and links.
  - From the author editor: open `Manage Authors`, edit an author and click the "Author Books" button to view the same list in a dialog.
- **Data integrity tools**: detect publications whose digital files are missing on disk (`PubsNotFoundOnDisk`), publications with wrong paths (`PubsWithWrongPath`), and store PDFs/PNGs flagged as "not deleted".

### ✍️ Authors & Author Management
- **Manage Authors**: a dedicated area to create, edit and remove author records. Each author stores basic profile data (name, biography, external links, photo) and links to publications.
- **Author Books**: a dialog that combines Google Books, Wikidata and AI-powered fallback results so you can quickly attach accurate metadata and cover images to publications.
- **Author cleanup**: when you delete a publication the app can automatically remove the author if they have no other publications — configurable in App Settings.
- **PDF export**: generate a PDF list of authors from the authors page.
- **Why this matters**: centralized author records keep publication metadata consistent, speed up data entry and enable author-centric reports and filters across the app.

### 🖼️ Media Management
- **File uploads**: drag-and-drop for images, PDFs, videos and audio files.
  - **Progress tracking**: real-time progress bar for uploads.
- **Preview media**: inline previews and players for supported file types.
- **Categorization & tagging**: organize media files by categories and tags.
- **Playlists**: create, manage and edit audio/video playlists.
  - **Playlist details**: names, descriptions, associated media and a dedicated player.
- **Galleries**: video gallery, music gallery and image gallery.
- **Dynamic page titles**: media pages include dynamic titles with badges and subtitles.
- **YouTube integration**: add YouTube records/subscriptions and retrieve video metadata directly from the media area.
- **AI metadata retrieval**: the **Get Info** button fetches metadata (notes, metadata, edition year) using AI.
  - **Status updates**: progress and alerts during metadata retrieval.

### 📄 Document Management
- **Document editing**: edit Word documents (`.docx`) directly in the browser using `SfDocumentEditor`.
- **Printing**: print documents directly from the editor.
- **Document viewer**: dedicated viewer component with print/export support.
- **Custom styles**: enhanced styling for document options.

### 🔄 Real-Time Updates
- **SignalR integration**: real-time progress updates for long-running operations such as bulk imports, metadata retrieval and backups.

### 🤖 AI Integration (cloud-first with local fallback)
- **Cloud-first with local fallback**: AI features call **OpenRouter (cloud)** first and fall back to a **local Ollama** model when the cloud is unavailable (no API key, offline, 429/HTTP errors). Author-profile can also use an OpenAI-SDK client pointed at the configured local model.
- **Covered operations**: translate, media notes, media metadata, edition year, author books, book summary and author profile. Each has a cloud endpoint (`*-cloud`) and a local Ollama endpoint.
- **Metadata retrieval**: automatically fetch metadata (author profiles, author books, book summaries, media notes/metadata, edition year) using AI when external API calls fail.
- **Free vs paid models**: the `AllowPaidModels` toggle is enforced server-side. Free-model usage is counted and capped per day; when the cap is exhausted the cloud services return null so the caller falls back to Ollama.
- **Provider/model display**: the translation dialog shows which provider/model actually answered (e.g. `⊘ OpenRouter · llama-3.3-70b`), including the served model when the `openrouter/free` router is used.
- **Cancellation**: translations can be cancelled from the UI without waiting for the provider.
- **Customizable prompts**: prompt templates live in `MediaOrganizerApp.WebApi/Prompts/*.prompt.txt` and are editable at runtime via the **Edit Prompts** admin page.
- **Usage logging**: every non-chat AI call is recorded in the `AIUsageLog` table and surfaced on the dashboard ("IA em uso interno").

### 💬 AI Chat & Conversations
- **Cloud Chat** (`/ai/openrouter-chat`): user-facing chat backed by OpenRouter, with streaming replies.
- **Local Chat** (`/ai/chat`): local-only Ollama chat (exception to the cloud-first rule).
- **Conversations**: chats are grouped into conversations; each conversation stores messages and context and can be reopened for follow-up queries.
- **Chat feedback**: assistant replies can be copied to the clipboard and rated 👍/👎. Ratings persist per conversation in `ChatFeedback`.
- **Chat history deletion**: users can delete conversations from the History panel; deletion is per-conversation (Admin deletes all).
- **Reasoning** ("💭 Raciocínio"): when enabled in the common AI settings, models show a collapsible reasoning block per message; the choice is respected and persisted. It works for OpenRouter and local Ollama (including Qwen3-style inline `<think>` blocks).
- **Safety artifact filter**: strips model safety-classification-only lines from responses; when the whole response is the artifact, the UI prompts the user to reply "continua".
- **Fallback behavior**: the UI prefers external APIs (Google Books, Wikidata, Last.fm) and falls back to AI only when external lookups do not return acceptable results. This reduces unnecessary LLM usage and improves metadata accuracy.
- **Usage logging**: chat responses are stored in `AIResponses`; non-chat AI usage goes to `AIUsageLog` (see above).

### 🗂️ AI Models, Prompts & Usage (admin)
- **AI Models**: a lookup table of models (`/aimodels`) with add/edit support, used by the settings pickers.
- **Prompt editor**: admins can edit all prompt templates used by the AI services without deploying code.
- **Usage dashboard**: the dashboard shows "IA em uso interno" with per-user/per-operation usage from `AIUsageLog`.
- **Benchmark**: AI request metrics (latency, success, provider) are exposed by `/api/ai-benchmark` and used by the dashboard.

### 🎨 UI, Themes & Localization
- **Light/dark theme**: app-wide CSS custom properties (`theme-tokens.css`) with a toggle in the top bar. The top bar and sidebar stay dark in both themes by design.
- **Localization**: Portuguese (default), English, French and Spanish, selectable in Settings (footer of the sidebar).
- **Built-in Help Manual**: the `HelpDialog` component renders `Web.Blazor/docs/Help*.md` in the four languages; open it with the `?` button in the top bar without leaving the current page.

### 🔧 Additional Server & Utility Features
- **File integrity checks**: APIs and tools to detect publications whose digital files are missing on disk and to run verification or repair operations.
- **PDF/PNG retention helpers**: endpoints to store files flagged as "not deleted" and preserve important uploads.
- **Cascade author cleanup**: when deleting a publication, the system can automatically remove the author record if they have no other publications.
- **AI prompts management**: UI page to edit and manage AI prompt templates.
- **Ollama author & book services**: server-side Ollama clients generate author profiles and assist with book metadata when external APIs fail.
- **Folder picker**: Syncfusion File Manager is used in App Settings to choose storage/backup folders.
- **Application logs**: built-in log viewer with severity filters, login views and deletion.

### 📊 Analytics & Reporting
- **Publication statistics**: number of publications by category, author or year.
- **Media usage insights**: analyze media file usage and identify unused or outdated files.
- **Customizable reports**: PDF reports rendered server-side from RDLC templates (`Reports_RDLC/`) through `ReportViewerCore.NETCore`; Excel export for lists.
- **Dashboard**: summary cards, pie charts and AI usage panels.

### 🔐 Multi-User System & Data Isolation
- **Multi-user architecture**: multiple users with individual accounts and separate collections.
- **Role-based data access**:
  - **Admin**: full system access — view and manage ALL users' data, settings, logs and backups.
  - **GeneralUser**: access ONLY their own publications, media and playlists — data is automatically filtered by `UserID`.
- **User management**: administrators can create, edit and delete user accounts from the "Manage Users" page.
- **Data isolation**: each user's data is stored with their `UserID`; repositories automatically filter queries to the logged-in user (unless Admin). Isolation is enforced at the repository level, not just in the UI.
- **Authentication**: secure login using ASP.NET Core Identity, including optional two-factor authentication (authenticator app + recovery codes).
- **Use cases**: families, small libraries/archives, collector teams, personal collections with admin oversight.

---

## 🧩 Solution Structure

| Project | Role | TFM |
|---|---|---|
| `MediaOrganizerApp.WebApi` | REST API backend (controllers, AI services, Ollama/OpenRouter integration, reports) | net10.0 |
| `Web.Blazor` | Blazor Server frontend (UI, Syncfusion components, typed API clients) | net10.0 |
| `MediaOrganizerApp.Application` | Interfaces, DTOs, validation (FluentValidation), Dapper queries | net10.0 |
| `MediaOrganizerApp.Domain` | Domain entities | net10.0 |
| `MediaOrganizerApp.Persistence` | EF Core + Dapper repositories and data access | net10.0 |
| `MediaOrganizerApp.Identity` | ASP.NET Core Identity setup | net10.0 |
| `MediaOrganizerApp.EmailService` | FluentEmail-based email sending | net10.0 |
| `MediaOrganizerApp.Tests` | xUnit + Moq tests | net10.0 |
| `Web.Blazor.ReportObjects` | RDLC report templates and helpers | net10.0 |
| `PromptEditor` | Standalone Windows Forms prompt editor (not part of the solution) | net10.0-windows |

**Two independent processes**: `MediaOrganizerApp.WebApi` and `Web.Blazor`. The Blazor app calls the API over HTTP using `BaseUrl` from its configuration; the API also has a direct database connection.

---

## 📂 Pages Overview

| Page | Description |
|---|---|
| **Home** | Dashboard with key metrics, charts and quick links. |
| **Publications** | Manage publications, view details and perform CRUD operations. |
| **GoogleBooksApiSearch** | Search and retrieve book metadata using the Google Books API. |
| **Media** | Upload, preview and manage media files. |
| **Video Gallery / Music Gallery** | Browse and manage video and music files. |
| **Playlists** | Create and manage audio/video playlists and players. |
| **Backup Database** | Back up the SQL Server database. |
| **Backup Publications / Media / Images** | Back up the corresponding upload folders. |
| **Manage Authors** | Manage author information, books and PDF export. |
| **Manage Users** | Manage user accounts and roles. |
| **Lookup Tables** | Manage auxiliary data (authors, categories, publishers, formats, archive locations, AI models). |
| **App Settings** | Configure application-wide settings (classic tabbed version, `/admin/appsettings`). |
| **App Settings 2** | Configure application-wide settings (refactored icon-sidebar version, `/admin/appsettings2` — linked from the sidebar footer). |
| **Manage Lookup Tables** | Manage auxiliary data (classic tabbed version). |
| **Manage Lookup Tables New** | Manage auxiliary data (refactored grouped/cards version). |
| **File Manager** | Browse folders and choose storage/backup paths. |
| **Data Integrity** | Find publications with missing/wrong files and preserve PDFs/PNGs. |
| **View Logs** | View and filter application logs. |
| **Edit Prompts** | Edit AI prompt templates. |
| **AI Models** | Manage the AI model lookup table. |
| **Chat (Ollama)** | Local-only Ollama chat. |
| **Chat (OpenRouter)** | Cloud chat with conversations, feedback and history. |
| **Help** | Open the built-in user manual via the top-bar `?` button. |
| **Reports** | Generate and download reports for publications and media. |

---

## 🧭 Navigation / Sidebar Overview

The app uses a persistent sidebar navigation accessible from the main layout. Key groups and pages include:

- **Backup**
  - Backup Database
  - Backup Publications / Media / Images
- **Maintenance / Admin**
  - Manage Users
  - Manage Lookup Tables
  - File Manager
  - View Logs
- **AI**
  - Edit Prompts
  - Chat (Ollama)
  - Chat (OpenRouter)
- **Books / Publications**
  - Manage Publications
  - Image Gallery
  - Authors
- **Media**
  - Manage Media
  - Video Gallery
  - Playlists
- **Footer**
  - Settings (`/admin/appsettings2`, includes language and theme)
  - Help (`?` in the top bar)
  - Logout

Labels are localized through the Blazor resources (`pt`, `en`, `fr`, `es`).

---

## ⚙️ Technologies Used

- **Frontend**: Blazor Server (Syncfusion Blazor components)
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Dapper (main queries) + EF Core (Identity and some persistence)
- **Real-time communication**: SignalR
- **AI integration**: OpenRouter (cloud-first) with local Ollama fallback; Cloud Chat with AI usage logging
- **Authentication**: ASP.NET Core Identity
- **Logging**: Serilog (SQL Server sink, `AppLogs` table)
- **Reports**: RDLC via `ReportViewerCore.NETCore`
- **Other tools**: FluentValidation, AutoMapper
- **Optional containers**: Docker Compose (Linux containers)

---

## 🛠️ Getting Started

There are two supported ways to run the app. **Docker is the reference path** — it is the only one that behaves the same on Windows, Linux and macOS. The Visual Studio path is Windows-only and matches the configuration the repository ships with.

| | Docker | Windows + Visual Studio |
|---|---|---|
| OS | Windows, Linux, macOS | Windows |
| Needs | Docker Desktop (Windows/macOS) or Docker Engine + compose (Linux) | .NET SDK 10, SQL Server, Visual Studio 2022 |
| Database | restored by `db-init` from a `.bak` | restored by you in SSMS |
| Ports | API `5000`, UI `5001` | API `44303`, UI `44327` |

> **`dotnet run` on its own does not work.** Both projects default to `https://localhost:5001`, and in Development the UI calls `https://localhost:44303` (the IIS Express port). See [Running with `dotnet run`](#running-with-dotnet-run) if you want the command line.

### Prerequisites (all paths)

- **.NET SDK 10.0** — also required for the Docker build.
- **A database backup** (`.bak` of `Publicacoes_Prod`) **or** an existing database with the same schema. The repository does **not** ship one: the backups under `Web.Blazor/wwwroot/Files/Backup/Database/` are ignored by git.
- **Ollama** (optional) for the local AI fallback; the cloud path (OpenRouter) works without it.

The clone is large (~400 MB) because it carries the app's image and audio assets.

### Path A — Docker (recommended; Windows, Linux, macOS)

1. **Place the database backup.** Copy your `.bak` into `Web.Blazor/wwwroot/Files/Backup/Database/`. If its name is not `28102026_123256.Bak`, set it in `.env`:

   ```bash
   SQL_BACKUP_FILE=mybackup.Bak
   ```

   `db-init` restores it on the first start; on later starts it detects the database and skips the restore (idempotent).

2. **Start the stack** from the repository root (not from `docker/`):

   ```bash
   docker compose up --build
   ```

3. **Open** <http://localhost:5001> (API on <http://localhost:5000>).

4. **First login** — see [First login](#first-login-all-paths).

5. **Local AI (optional)** — pull a model and select it in App Settings: see [Ollama in the Docker stack](#ollama-in-the-docker-stack).

Notes for Linux and macOS:

- Only Docker is needed; nothing else from this repository is installed on the host.
- On **Apple Silicon** the SQL Server image is x86-64 only and runs under emulation (slow, sometimes unstable). On Intel/AMD it runs natively.
- `System.Drawing.Common` is Windows-only: image resizing and some PDF generation throw `PlatformNotSupportedException` in the containers. Browsing, CRUD and the AI features work.

### Path B — Windows + Visual Studio (IIS Express)

This is the configuration the repository ships with: `appsettings.Development.json` and the sample database point at the IIS Express ports.

1. Install **.NET SDK 10**, **SQL Server** (Express is enough) and **Visual Studio 2022** with the ASP.NET workload.
2. Restore the `.bak` in SQL Server Management Studio as `Publicacoes_Prod`.
3. Point the API at your instance — the value in the repository is the author's machine (`DESKTOP-SLIMNUD\SQLEXPRESS`):

   ```powershell
   dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "ConnectionStrings:SqlServerCon" "Server=.\SQLEXPRESS;Database=Publicacoes_Prod;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
   ```

4. Open `MediaOrganizerApp.sln`, set **both** `MediaOrganizerApp.WebApi` and `Web.Blazor` as startup projects, and press **F5**. The API listens on `https://localhost:44303`, the UI on `https://localhost:44327`.
5. **First login** — see below.

### First login (all paths)

Create an account at `/Identity/Account/Register` (linked from the login page).

**The sample data belongs to the user who created the backup.** A new account has the `GeneralUser` role and only sees its own (empty) data. To see the existing data, the account must be **Administrator**:

```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u CROSS JOIN AspNetRoles r
WHERE u.UserName = 'your-user@example.com' AND r.Name = 'Administrator';
```

- **Windows / Visual Studio**: run it in SSMS against `Publicacoes_Prod`.
- **Docker**: run it inside the container (the password comes from `.env`):

  ```bash
  docker compose exec -T sql bash -s <<'EOF'
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d Publicacoes_Prod -Q "
  INSERT INTO AspNetUserRoles (UserId, RoleId)
  SELECT u.Id, r.Id FROM AspNetUsers u CROSS JOIN AspNetRoles r
  WHERE u.UserName = 'your-user@example.com' AND r.Name = 'Administrator';"
  EOF
  ```

Once an administrator exists, further roles are managed in **Administration**.

### Running with `dotnet run`

The command-line path is **not configured** in the repository. To use it you have to change four things:

1. **Different ports** — both launch profiles ask for 5001:

   ```powershell
   dotnet run --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj --urls "https://localhost:5001"
   dotnet run --project .\Web.Blazor\Web.Blazor.csproj --urls "https://localhost:5002"
   ```

   `5002` is just an example — any free port works.

2. **`BaseUrl`** in `Web.Blazor/appsettings.Development.json` must point at the API (`https://localhost:5001`), not at `https://localhost:44303`.
3. **Connection string** — set it with user-secrets (see Path B). On Linux/macOS `Trusted_Connection=True` does not exist: use SQL authentication (`User Id=sa;Password=...`).
4. **A database** (see below).

`scripts/start-all.ps1` launches both projects with the default profiles, so it hits the same port collision — it is not a workaround.

### Configuration & secrets
The main connection string key is **`ConnectionStrings:SqlServerCon`**. Example:
```json
{
  "ConnectionStrings": {
    "SqlServerCon": "Server=DESKTOP-SLIMNUD\\SQLEXPRESS;Database=Publicacoes_Prod;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
  },
  "Ollama": { "BaseUrl": "http://localhost:11434" }
}
```

Use .NET user-secrets for local development:
```powershell
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "ConnectionStrings:SqlServerCon" "Server=.;Database=Publicacoes_Prod;Trusted_Connection=True;"
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "Ollama:BaseUrl" "http://localhost:11434"
```

External service keys (Last.fm, Google Books, IMDb, YouTube, OpenRouter) are **stored in the database**, in `ApplicationSettings`, and edited from the **Serviços / IA** areas of App Settings. They are read at runtime; there is no `Keys.cs`.

The Web API is protected by an `ApiKey` middleware. Set the `ApiKey` value in `appsettings.json` (or user-secrets) and send it in the `ApiKey` header from clients.

### Database
- The application schema lives in `Publicacoes_Prod`.
- Feature tables/procedures are created with the scripts in `scripts/` (run them on the target database):
  - `CreateConversationsTable.sql` and `CreateConversationsStoredProcedures.sql`
  - `CreateAIChatSchema.sql`
  - `CreateAIUsageLog.sql`
  - `CreateChatFeedback.sql`
  - `AddGoogleBooksSettings.sql`
  - `AddYouTubeSettings.sql`
- The ASP.NET Core Identity schema is managed by EF Core migrations in `Web.Blazor/Data/Migrations`:
  ```powershell
  dotnet ef database update --project .\Web.Blazor\Web.Blazor.csproj
  ```

### Run both processes
The solution has **two independent processes**. Use **Path A (Docker)** or **Path B (Visual Studio + IIS Express)** above; the command-line path is described in [Running with `dotnet run`](#running-with-dotnet-run) and requires configuration changes.

> **Production ports** used by the IIS pipeline: Web API `http://localhost:4100` (loopback), Blazor `https://localhost:4110`.

### Tests
```powershell
dotnet test MediaOrganizerApp.sln
```
The tests mock HTTP calls (no live Ollama or database required) using xUnit + Moq.

---

## 🔌 API & Integrations

The Web API exposes REST endpoints for the main areas:

| Area | Endpoint |
|---|---|
| Publications | `/api/publications` |
| Media | `/api/media` |
| Authors | `/api/autores` |
| Playlists | `/api/playlists` |
| Users | `/api/users` |
| Application settings | `/api/appsettings` |
| Dashboard / stats | `/api/dashboard`, `/api/stats` |
| AI operations | `/api/ai` (translate, media notes/metadata, author books/profile, book summary) |
| Cloud chat (streaming) | `/api/cloud-chat` |
| Conversations | `/api/conversations` |
| Chat feedback | `/api/chat-feedback` |
| AI usage / benchmark | `/api/ai-usage`, `/api/ai-benchmark` |
| Reports | `/api/report` |
| Google Books / Wikidata / Wikipedia | `/api/googlebooks`, `/api/wikidata`, `/api/wikipedia` |
| PDF operations | `/api/pdf` |

Requests to the Web API must include the `ApiKey` header (validated by the API-key middleware). The Blazor UI authenticates users with ASP.NET Core Identity and forwards the configured API key.

### Integrations
- **Local file storage**: uploads and publication files are stored under `Web.Blazor/wwwroot/Files` (the `Pasta*` paths in `ApplicationSettings` can point elsewhere).
- **External metadata**: Google Books, Wikidata, Wikipedia, Last.fm and YouTube.
- **AI providers**: OpenRouter (cloud) and Ollama (local fallback), configured from the app settings stored in the database.

### Smoke-test examples
```bash
curl -X GET "https://localhost:5001/api/publications" \
  -H "ApiKey: <API_KEY>"

curl -X POST "https://localhost:5001/api/media/upload" \
  -H "ApiKey: <API_KEY>" \
  -F "file=@/path/to/file.mp3" \
  -F "title=My Track"
```

---

## 🔐 User Roles & Data Isolation

### User roles
| Role | Data access | System access |
|---|---|---|
| **Admin** | **Sees ALL users' data** — publications, media and playlists from every user. | Full system access: user management, app settings, logs, backups, prompts. |
| **GeneralUser** | **Sees ONLY their own data** — publications, media and playlists filtered by `UserID`. | Standard features: create/edit/delete their own data. Cannot access admin pages. |

### How data isolation works
Every publication and media record is tagged with a `UserID`. The repository layer filters queries based on the logged-in user's role:

```csharp
if (userRole == UserRole.GeneralUser)
{
    // Only this user's publications
    sbData.Append(" WHERE IdFormato = @IdFormato AND UserID = @UserID");
}
else // Admin
{
    // All users' publications
    sbData.Append(" WHERE IdFormato = @IdFormato ");
}
```

**Key points**
- **GeneralUser** sees a personalized catalog with only their own items.
- **Admin** has oversight and can view/manage data across all users.
- Data isolation is enforced at the **repository level**, not just in the UI.

> 📊 See detailed diagrams and code examples in the [Architecture Documentation](docs/architecture/USER_ROLE_DATA_FLOW.md).

---

## ⚙️ Admin Options

### 1. App Settings
App Settings (`/admin/appsettings` and `/admin/appsettings2`) is organized into four areas:

#### **Pastas / folders**
- Download folders (PDF and media).
- Backup folders (database and uploads).
- Publication, image, listing, author-cover and background folders.
- Folder pickers backed by the File Manager.

#### **Other Settings 1**
- Contacts (author mobile, support email).
- Notification time.
- Organized-files flag and "remove image after edit".
- Default language.

#### **Other Settings 2**
- API base URLs (production/development) for the Web API and File Manager.
- Last.fm, Google Books, IMDb and YouTube credentials.
- Database connection string.

#### **IA (single AI area)**
- **Common definitions**: reasoning toggle (shared by OpenRouter and Ollama).
- **OpenRouter**: API key, model picker, paid-models toggle, author-books cloud toggle, free daily cap and usage cards.
- **Ollama (local)**: API key, base URL and model name.

### 2. Logs
The **View Logs** page shows application logs (Serilog → SQL Server `AppLogs`) with:
- Filters by severity (information, warning, error) and login events.
- A consolidated table view.
- Details and deletion of entries.

### 3. Backups
Backups are performed from the Backup pages and use the folders configured in App Settings:
- **Database**: SQL Server backup of `Publicacoes_Prod`.
- **Uploads**: publications, media and images.
- Restore is done at the database/file level with the generated backup files (there is no scheduled-backup scheduler).

### 4. Lookup Tables
The lookup tables area manages shared data such as authors, categories, publishers, publication/media formats, archive locations and AI models.

---

## 🐳 Docker (Windows, Linux, macOS)

`docker-compose.yml` starts `sql` + `db-init` + `ollama` + `webapi` + `webblazor`. It is an **optional local stack** and does **not** replace the IIS production deployment.

**`.bak` prerequisite:** before the first start, copy your `Publicacoes_Prod` `.bak` into `Web.Blazor/wwwroot/Files/Backup/Database/` (the `sql` service mounts that folder as `/backup`). If its name is not `28102026_123256.Bak`, set it in `.env`:

```bash
SQL_BACKUP_FILE=mybackup.Bak
```

Without the `.bak`, `db-init` fails and the stack will not start — there is no fallback schema script in the stack.

```bash
# from the repository root (not from docker/)
docker compose up --build
```

- Published ports: **5000** (Web API) and **5001** (Blazor). `sql` and `ollama` are internal-only (the host already uses 1433/11434); access them with `docker exec`.
- `db-init` waits for SQL Server, restores the `.bak` from `Web.Blazor/wwwroot/Files/Backup/Database`, and normalizes `ApplicationSettings` for the containers (`\` → `/`, `ModelBaseUrl=http://ollama:11434`, API URLs pointing to `webapi:5000`). It is idempotent.
- `.env` defines `SQL_SA_PASSWORD` (do **not** end the password with `;` — it is the connection-string separator). After changing it, recreate the volume: `docker compose down -v && docker compose up -d`.
- **Known limitation**: `System.Drawing.Common` is Windows-only in .NET 6+. Image resizing and some PDF generation paths throw `PlatformNotSupportedException` on Linux containers. Browsing, CRUD and most AI features work.

#### Ollama in the Docker stack

The `ollama` service runs the Ollama **engine**, not models: model files are downloaded at runtime
into the compose volume (`ollama-data`). A fresh stack therefore has an **empty model library**, and
it is **independent** from any Ollama installed on your host — the container cannot see your
`~/.ollama`, and a host Ollama listens on `127.0.0.1:11434`, which the container cannot reach.

The local AI always uses **one** model: the one selected in
**Admin → App Settings → IA → Ollama (local)**. That is a **user setting**, not a fixed value — it is
stored in `ApplicationSettings.AiModelName` and can be changed at any time. The sample database
carries whatever was selected when the backup was taken (`Qwen3:1.7B`), so treat that as an example,
not a requirement.

1. **Install a model in the container** (any model you want; ~1.4 GB for a 1.7B one):

   ```bash
   docker compose up -d ollama
   docker compose exec ollama ollama pull qwen3:1.7b
   docker compose exec ollama ollama list
   ```

2. **Select it in the app**: Admin → App Settings → IA → Ollama (local). The dropdown lists the
   models installed in the container (`/api/tags`), the selection is saved to
   `ApplicationSettings.AiModelName` and applies immediately. Models can also be pulled/deleted from
   **Admin → Lookup tables → AI Models**.

3. If the selected name is not installed, every local AI call fails with
   `404 model "..." not found` — pull that model (step 1) or pick another one (step 2).

4. **Nothing needs to be "loaded"**: the app lists *installed* models; Ollama loads one into RAM on
   the first request and evicts it later (`ollama ps` is not used).

Models survive `docker compose down`/`up`; `docker compose down -v` deletes them (and the database,
which `db-init` restores again). Budget ~2.5–3 GB of RAM for a 3B model **on top of** ~2 GB for SQL
Server. To use the host's Ollama instead, start it with `OLLAMA_HOST=0.0.0.0:11434`, allow it
through the firewall, and set `ModelBaseUrl` to `http://host.docker.internal:11434` **after**
`docker compose up` (`db-init` rewrites it on every start) — then select one of the host's models.

| Symptom | Cause | Fix |
|---|---|---|
| Empty model list in App Settings | no models in the container | step 1 |
| `404 model "..." not found` | the selected model is not installed | step 1, or step 2 |
| Connection refused | the `ollama` container is not running | `docker compose up -d ollama` |
| `sql` container exits with 137 | not enough memory | raise WSL2/Docker Desktop memory |

Full guide: [`Web.Blazor/docs/ollama-docker-setup.md`](Web.Blazor/docs/ollama-docker-setup.md).

Memory note: SQL Server 2022 needs roughly 2 GB. If the `sql` container exits with code 137, increase the WSL2/Docker Desktop memory before `docker compose up`.

---

**Deploy with IIS (Windows only):** this section describes automated and manual deployment to IIS on Windows Server (`appcmd`, `icacls`, app pools, `C:\inetpub\...`). It does **not** apply to Linux/macOS.
## 🚀 Deploying with IIS

### Automated pipeline (GitHub Actions)
`.github/workflows/main.yml` publishes both projects **self-contained** (`win-x64`, `PublishReadyToRun=false`) and deploys to IIS on the same machine:

- **Web API**: HTTP `http://localhost:4100` (loopback, used by Blazor on the same host).
- **Blazor**: HTTPS `https://localhost:4110` with a self-signed certificate.
- **Sites/app pools**: `MediaOrganiserApi` → `C:\inetpub\MediaOrganizerAPI`; `OrganiseIt` → `C:\inetpub\OrganiseIt`.
- `ASPNETCORE_ENVIRONMENT=Production` is set explicitly on both app pools.
- The app-pool identities must match the SQL logins: `IIS APPPOOL\MediaOrganiserApi` and `IIS APPPOOL\OrganiseIt` (both `db_owner` in `Publicacoes_Prod`). With `Trusted_Connection=True`, renaming a pool without creating the matching SQL login removes database access.
- ACLs grant `Modify` on both deployment folders to the pool identities (required for uploads).
- **HSTS is disabled in production** while the certificate is self-signed (`Hsts:Enabled = false`). Enable it only after switching to a trusted certificate.
- The pipeline finishes with a smoke test: `GET /api/dashboard/health` with the `ApiKey` header and `GET https://localhost:4110/` with certificate validation disabled; failures dump IIS/event-log diagnostics and fail the job.
- The runner must be **elevated** (IIS and `Cert:\LocalMachine\My`) and the machine needs the **ASP.NET Core Hosting Bundle** (the publish uses `AspNetCoreModuleV2`, in-process).

### Manual publish
1. In Visual Studio, publish **MediaOrganizerApp.WebApi** and **Web.Blazor** to folders (for example `C:\Publish\WebApi` and `C:\Publish\Web.Blazor`).
2. In IIS Manager, create a site for the API (port `4100` or your choice) and one for Blazor (port `4110`/HTTPS).
3. Set the app pools to **No Managed Code** and `ASPNETCORE_ENVIRONMENT=Production`.
4. Grant the pool identities `Modify` on the deployment folders.
5. Configure the HTTPS binding with a trusted certificate (or a self-signed one for local use) and register it with `netsh http add sslcert` / the IIS binding UI.

### SQL Server logins
With Windows authentication, the process identity is `IIS APPPOOL\<pool name>`:
```sql
CREATE LOGIN [IIS APPPOOL\MediaOrganiserApi] FROM WINDOWS;
CREATE LOGIN [IIS APPPOOL\OrganiseIt] FROM WINDOWS;

USE Publicacoes_Prod;
CREATE USER [IIS APPPOOL\MediaOrganiserApi] FOR LOGIN [IIS APPPOOL\MediaOrganiserApi];
CREATE USER [IIS APPPOOL\OrganiseIt] FOR LOGIN [IIS APPPOOL\OrganiseIt];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\MediaOrganiserApi];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\OrganiseIt];
```

---

## 🧯 Troubleshooting (IIS Deployment)

### HTTP Error 500.19 (`0x8007000d` or `0x80070002`)
This error occurs when IIS cannot read the `<aspNetCore>` section in `web.config` because the **ASP.NET Core Module V2** was not registered in the server's global configuration (`applicationHost.config`) — even after installing the *.NET Hosting Bundle* (often due to antivirus blocking, missing administrator privileges or installation order).

> **Note:** if you run the app locally via Visual Studio or `dotnet run` (Kestrel/IIS Express), you can ignore this section.

#### How to fix
1. **Repair the Hosting Bundle**
   - Close *IIS Manager*.
   - Run the **.NET 10 Hosting Bundle** installer as **Administrator** and choose **Repair**.
   - Restart IIS from an elevated terminal: `iisreset`.

2. **Manual registration (if Repair fails)**
   If the installer is blocked by security policies or antivirus and cannot update IIS, run the following in **PowerShell as Administrator** to register the section and module (this is a reference script — review before applying):

   ```powershell
   $configPath = "$env:windir\System32\inetsrv\config\applicationHost.config"
   $content = [System.IO.File]::ReadAllText($configPath)

   # 1. Declare the aspNetCore section (fixes 0x8007000d)
   if ($content -notlike '*name="aspNetCore"*') {
       $content = $content -replace '(<sectionGroup name="system.webServer">)', "`$1`r`n            <section name=`"aspNetCore`" overrideModeDefault=`"Allow`" />"
   }

   # 2. Remove old module entries and inject the global module (fixes 0x80070002)
   $content = $content -replace '<add name="AspNetCoreModuleV2"[^>]*>', ''
   $moduleLine = '<add name="AspNetCoreModuleV2" image="%ProgramFiles%\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll" />'
   $content = $content -replace '(<globalModules>)', "`$1`r`n        $moduleLine"
   $content = $content -replace '(<modules[^>]*>)', "`$1`r`n        <add name=`"AspNetCoreModuleV2`" />"

   [System.IO.File]::WriteAllText($configPath, $content)
   iisreset
   ```

3. **Folder permissions**
   Ensure the application-pool identity (`IIS APPPOOL\<PoolName>`) has **Read & Execute** (or **Modify**, if the app writes uploads/backups) on the deployment folder.

### HTTP Error 500.30 (*ASP.NET Core app failed to start*)

The Blazor app configures **Serilog with an MSSQL Server sink during startup** (`WriteTo: MSSqlServer` with `autoCreateSqlTable: true`), so it **cannot start while SQL Server is unreachable**. When a SQL Server restart overlaps the deploy — a **Windows Update** for SQL Server (cumulative/GDR), a manual restart or a failover — the worker starts, crashes and leaves the site stuck:

- the `w3wp` process starts, throws `Microsoft.Data.SqlClient.SqlException` (`Win32Exception (258)` — TCP timeout) and **stays alive with the CLR unloaded**;
- every later request returns **`500.30` instantly, with no new Event Log entries** — the crash is recorded only once (`1007` *failed to load coreclr*, `1018` *hit unexpected managed exception*);
- the API can keep answering normally, because it started before the restart and reuses pooled connections.

Diagnose (PowerShell):

```powershell
# worker per site and when it was created
Get-CimInstance Win32_Process -Filter "Name='w3wp.exe'" | Select-Object ProcessId, CreationDate

# crash reason (one event per crash, not per request)
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IIS AspNetCore Module V2'; StartTime=(Get-Date).Date} |
    Where-Object Id -in 1007, 1018, 1026 | Select-Object TimeCreated, Id, Message
```

A worker created around the crash time, with no `1032 started successfully` after it, is a **zombie**: the app never starts again inside that process, so the `500.30` is not a configuration error.

#### How to fix
1. Confirm that the SQL Server engine is up and the deployment database is reachable:
   ```powershell
   Get-Service 'MSSQL$SQLEXPRESS'
   # or, from SQL: SELECT CONVERT(varchar(30), sqlserver_start_time, 121) FROM sys.dm_os_sys_info
   ```
2. Recycle the app pool from an **elevated** PowerShell — IIS creates a fresh worker and the app starts on the next request:
   ```powershell
   Import-Module WebAdministration
   Restart-WebAppPool -Name 'OrganiseIt'        # Blazor
   Restart-WebAppPool -Name 'MediaOrganiserApi' # API
   ```
3. Re-probe the site. A healthy start writes `1032 Application '...' started successfully` to the *Application* log:
   ```powershell
   curl.exe -k -sS -o NUL -w "%{http_code}`n" https://localhost:4110/
   ```
4. Re-run the pipeline. Its smoke test checks the API **and** the database (`/api/dashboard/health`), so a deploy that overlapped the SQL outage stays red until both are healthy again.

> **Tip:** schedule SQL Server updates outside deployment windows, or re-run the pipeline once the update finishes. The `500.30` above is exactly what the smoke test is meant to catch — a deploy is only green when the API returns `200` **and** Blazor returns `2xx/3xx`.

---

## 🤝 Contribution Guidelines

1. Fork the repository.
2. Create a branch: `git checkout -b feature/your-feature-name`.
3. Make your changes and run `dotnet build` and `dotnet test`.
4. Commit: `git commit -m "Add your message here"`.
5. Push: `git push origin feature/your-feature-name`.
6. Open a pull request.

---

## 📜 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE.txt) file for details.

---

## 📞 Contact & Support

- **Email**: support@mediaorganizerapp.com
- **GitHub Issues**: [Submit an Issue](https://github.com/fauxtix/MediaOrganiserApp/issues)

---

## 🖼️ Screenshots

_Add screenshots here to showcase the application._
