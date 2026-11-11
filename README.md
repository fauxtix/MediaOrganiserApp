# 📚 Media Organiser App

**Multi-User Collection Management System**

[Versão em Português](README-PT.md) | [Architecture Diagrams](docs/architecture/USER_ROLE_DATA_FLOW.md)

Have you ever felt frustrated with managing your media like hundreds of media scattered across different storage mediums/ hard disks? Wouldn't it be great to have it organized, allowing for easy management?

Media Organizer App is a **multi-user application** to organize your media (Videos and MP3 files) and Documents (PDFs and Word documents).  
It also allows you to organize collections of Books, LPs, CDs, DVDs, and documents written on paper (filing cabinets, for example).

This application is designed to streamline the organization, management, and retrieval of media and publication data with **role-based access control**, **user-specific data isolation**, and a robust backend.

**Perfect for:** Personal collections, families, small libraries, archive projects, collector teams, or any scenario where multiple users need to maintain separate catalogs in a shared system.

> 📊 **[View Interactive Architecture Diagrams](docs/architecture/USER_ROLE_DATA_FLOW.md)** - See how data isolation and role-based access control work

---

## ✨ Key Features

### 📚 Publications Management
- **CRUD Operations**: Create, read, update, and delete publications with detailed metadata.
- **Bulk Import/Upload**:
  - Upload publications in bulk using Pdf, Excel, or Word files.
  - Automatic and manual data mapping for seamless integration.
  - Validation for required fields, duplicate detection, and error reporting.
- **Search & Filter**: Advanced search capabilities by title, author, category, or keywords.
- **Duplicate Detection**: Automatically flag duplicate publications based on metadata.
- **Reports**: Generate detailed reports for publications, including statistics by category, author, or year.
- **Google Books API Search**: Integrated search for book metadata using the Google Books API.
 - **Author's books view**: View an author's books directly from the publication editor and from the author details page (uses Google Books with AI fallback when necessary).
   - How to access:
     - From the publication editor: open a publication in `Publications` and click the author lookup/search button next to the author field to open the author dialog; the dialog shows author details and an "Author Books" list with thumbnails, titles and links.
     - From the author editor: open `Manage Authors`, edit an author and click the "Author Books" button to view the same list in a dialog.

### ✍️ Authors & Author Management (user-facing)
- **Manage Authors**: A dedicated Authors area lets you create, edit and remove author records. Each author stores basic profile data (name, biography, external links) and links to publications in the system.
- **Author Books**: From a publication editor or the author details page you can open an "Author Books" dialog. The dialog combines Google Books results (when available) with AI-powered fallback results so you can quickly attach accurate metadata and cover images to publications.
- **Author cleanup**: When you delete a publication the app can automatically remove the author if they have no other publications — this behavior is configurable in App Settings.
- **Why this matters**: Centralized author records keep publication metadata consistent, speed up data-entry and enable author-centric reports and filters across the app.

### 🖼️ Media Management
- **File Uploads**: Drag-and-drop file uploads for images, PDFs, videos, and audio files.
  - **Progress Tracking**: Real-time progress bar for file uploads.
- **Preview Media**: Inline previews for supported file types.
- **Categorization & Tagging**: Organize media files by categories and tags.
- **Playlists**: Create, manage, and edit playlists for organizing media files.
  - **Media Types**: Support for audio and video playlists.
  - **Playlist Details**: Display playlist names, descriptions, and associated media.
- **Dynamic Page Titles**: Media pages now include dynamic titles with badges and subtitles.
- **YouTube Integration**: Add YouTube records directly from the media management interface.
- **AI Metadata Retrieval**: Fetch metadata for media files directly from the UI.
  - **Get Info Button**: Allows users to retrieve metadata for a selected file using AI prompts.
  - **Status Updates**: Displays progress and alerts during the metadata retrieval process.

### 📄 Document Management
- **Document Editing**: Edit Word documents (`.docx`) directly in the browser using `SfDocumentEditor`.
- **Printing**: Print documents directly from the editor.
- **Custom Styles**: Enhanced styling for document options.

### 🔄 Real-Time Updates
- **SignalR Integration**: Real-time progress updates for long-running operations like bulk imports or metadata retrieval.

### 🤖 AI Integration
- **Cloud-first with local fallback**: AI features call **OpenRouter (cloud)** first and fall back to a **local Ollama** model when the cloud is unavailable (no API key, offline, 429/HTTP errors). Author-profile can also use an OpenAI-SDK client pointed at the configured local model.
- **Metadata Retrieval**: Automatically fetch metadata (author profiles, author books, book summaries, media notes/metadata, edition year) using AI when external API calls fail.
- **Customizable Prompts**: Dynamic queries for personalized results; prompt templates are editable via the "Edit Prompts" admin page.

### 🤖 AI Chat & Conversations (user-facing)
- **Chat page**: The app exposes a user-facing Cloud Chat page (AI) reachable from the main sidebar. Use Chat to run ad-hoc queries, fetch author profiles, or request metadata for media and publications.
- **Conversations**: Chats are grouped into conversations. Each conversation stores messages, context and can be re-opened for follow-up queries. The UI supports streaming replies for a natural chat experience.
- **Chat feedback**: Assistant replies can be copied to the clipboard and rated 👍/👎 (persisted per conversation, shown in the UI).
- **Chat history deletion**: Users can delete conversations from the History panel; deletion is per-conversation (Admin deletes all).
- **Raciocínio (Raciocínio)**: When enabled in common IA settings, models show a collapsible "💭 Raciocínio" block per message; the choice is respected and persisted.
- **Safety artifact filter**: strips model safety classification-only lines from responses; when the whole response is the artifact, the UI prompts the user to reply "continua".
- **Edit Prompts**: Admins can edit prompt templates used by the AI directly from the Blazor admin UI (Edit Prompts). This enables tuning of LLM behavior without code deployments.
- **Fallback behavior**: The UI prefers external APIs (Google Books) and falls back to the AI model only when external lookups do not return acceptable results. This reduces unnecessary LLM usage and improves metadata accuracy.
- **Usage logging**: Every non-chat AI call is recorded in the `AIUsageLog` table and surfaced on the dashboard ("IA em uso interno").


### 🔧 Additional server & utility features

- **Built-in Help Manual**: The `HelpDialog` component renders the Markdown user manual (from `Web.Blazor/docs/Help*.md`) in four languages, opened via the `?` button in the top bar without leaving the current page.
- **File integrity checks**: APIs and tools to detect publications whose digital files are missing on disk and to run verification or repair operations.
- **PDF/PNG retention helpers**: Endpoints to store files flagged as "not deleted" and related server-side handling to preserve important uploads.
- **Cascade author cleanup**: When deleting a publication, the system can automatically remove the author record if they have no other publications.
- **AI prompts management**: UI page to edit and manage AI prompt templates used by the AI metadata fallback services.
- **Ollama author & book services**: Server-side Ollama clients generate author profiles and assist with book metadata when external APIs fail.
- **Document viewer & printing**: Dedicated document viewer component with print/export support, complementing the in-browser document editor.
- **File Manager API & UI**: Endpoints and client services for browsing, uploading and managing application files (PDFs, images, media).

### 📊 Analytics & Reporting
- **Publication Statistics**: View statistics such as the number of publications by category, author, or year.
- **Media Usage Insights**: Analyze media file usage and identify unused or outdated files.
- **Customizable Reports**: Generate PDF or Excel reports tailored to your needs.

### 🔐 Multi-User System & Data Isolation
- **Multi-User Architecture**: Support multiple users with individual accounts and separate collections.
- **Role-Based Data Access**: 
  - **Admin Role**: Full system access — view and manage ALL users' data, system settings, logs, and backups.
  - **GeneralUser Role**: Access ONLY their own publications, media, and playlists — data is automatically filtered by UserID.
- **User Management**: Administrators can create, edit, and delete user accounts from the "Manage Users" page.
- **Data Isolation**: Each user's publications and media are stored with their UserID; queries automatically filter data to show only the logged-in user's items (unless you're an Admin).
- **Authentication**: Secure login using ASP.NET Core Identity.
- **Use Cases**: 
  - **Families**: Each family member manages their own book/media collection.
  - **Small Libraries/Archives**: Librarian (Admin) oversees; staff members (GeneralUser) catalog their assigned items.
  - **Collector Teams**: Multiple collectors maintain separate collections in one system.
  - **Personal with Admin Oversight**: Single user with admin access for maintenance and reporting.

---

## 📂 Pages Overview

| Page Name                  | Description                                                                 |
|----------------------------|-----------------------------------------------------------------------------|
| **Home**                   | Dashboard with key metrics and quick links.                               |
| **Publications**           | Manage publications, view details, and perform CRUD operations.            |
| **GoogleBooksApiSearch**   | Search and retrieve book metadata using Google Books API.                  |
| **Media**                  | Upload, preview, and manage media files.                                   |
| **Video Gallery**          | Browse and manage video files.                                             |
| **Playlists**              | Create and manage playlists for media files.                               |
| **Backup Database**        | Backup the database.                                                       |
| **Backup Publications**    | Backup publication uploads.                                                |
| **Backup Media**           | Backup media uploads.                                                      |
| **Backup Images**          | Backup image uploads.                                                      |
| **Manage Authors**         | Manage author information.                                                 |
| **Manage Users**           | Manage user accounts and roles.                                            |
| **Lookup Tables**          | Manage auxiliary data like authors, categories, and publishers.            |
| **Application Settings**   | Configure application-wide settings.                                       |
| **File Manager**           | Manage files and directories.                                              |
| **View Logs**              | View application logs for debugging and monitoring.                        |
| **Edit Prompts**           | Edit AI prompts for metadata generation.                                   |
| **Help**                   | Open the built-in user manual via the top-bar `?` button.                  |
| **Reports**                | Generate and download reports for publications and media.                  |
| **User Management**        | Manage user roles, permissions, and profiles.                              |
| **App Settings**           | Configure application-wide settings (classic tabbed version).              |
| **App Settings 2**         | Configure application-wide settings (refactored icon-sidebar version).     |
| **Manage Lookup Tables**   | Manage auxiliary data like authors, categories, and publishers.            |
| **Manage Lookup Tables New**| Manage auxiliary data (refactored layout with groups/cards).              |

---

## 🧭 Navigation / Sidebar overview

The app uses a persistent sidebar navigation accessible from the main layout. Key top-level groups and pages (as shown in the UI) include:

- Backup
  - Backup Database
  - Backup Publications (uploads)
  - Backup Media (uploads)
  - Backup Images
- Maintenance / Admin
  - Manage Users
  - Manage Lookup Tables
  - App Settings
  - File Manager
  - View Logs
- AI
  - Edit Prompts
  - Chat (Conversations)
- Home
- Publications
  - Manage Publications
  - Image Gallery
- Media
  - Manage Media
  - Video Gallery
  - Playlists
- Authors

Use the sidebar to quickly locate these features in the UI; labels are localized via resources in the Blazor project.

---

## Developer / Implementation Details

This section is for maintainers and integrators. It expands the README with implementation locations, detailed feature notes, API examples and configuration tips. The user-facing content above is preserved unchanged.

Where to look in the code (quick map)
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
- Web.Blazor.ReportObjects (report templates & helpers)

Detailed features implemented (expanded)

1) AI / Conversations / Chat
- Files: AiController.cs, ConversationsController.cs, AiAuthorProfileService.cs, OllamaAuthorProfileService.cs, OllamaBooksService.cs, Prompts/*.prompt.txt
- Behavior:
  - The UI uses typed client wrappers (Web.Blazor/Services/ApiWrappers/AiClientService and ConversationsClientService) to call API chat endpoints.
  - ConversationsController exposes conversation listing and streaming chat at `/api/ai/conversations` (explicit route attribute).
  - AiController exposes one-off AI endpoints at `/api/ai` (controller route attribute).
  - Ollama integration is used as an LLM fallback when third-party lookups (e.g., Google Books) return insufficient data.

2) File Manager
- Files: FileManagerController.cs (WebApi), Web.Blazor/Services/Utilities/FileManagerService.cs
- Behavior:
  - Endpoints for browsing server-side directories, uploading raw files, and file operations used by the admin UI and background tasks.

3) Playlists & Media
- Files: PlaylistsBase.cs, PlaylistClientService.cs, UploadMediaFiles.razor, EditMedia.razor
- Behavior:
  - Audio/video playlists with CRUD operations, playlist metadata and listing UI. Media pages include dynamic titles, thumbnails, preview players and a "Get Info" AI metadata button.

4) Document editor & viewer
- Files: Components/DocumentEditor.razor, DocumentViewerComponent.razor
- Behavior:
  - In-browser .docx editing (Syncfusion SfDocumentEditor) and a viewer with print/export support.

5) Reporting
- Files: Web.Blazor.ReportObjects/Helpers/ReportLists.cs, MediaOrganizerApp.WebApi/Controllers/ReportController.cs
- Behavior:
  - Server-side report rendering to PDF. Blazor pages trigger report generation via the Web API, which renders locally with ReportViewerCore.NETCore (RDLC templates in `Reports_RDLC/`).

6) Background helpers & integrity checks
- Files: Helpers/BusinessObjects/BO_*.cs
- Behavior:
  - File integrity checks to detect publications with missing files; retention helpers; cascade cleanup when deleting publications.

7) Admin UI & settings
- App Settings pages expose toggles for storage options, API keys and prompt editing. Prompts are editable via the Blazor "Edit Prompts" page.

Concrete API usage examples (curl) — copy/paste smoke tests
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

3) Start a conversation (AI) — POST to /api/ai/conversations

```bash
curl -X POST "https://localhost:5001/api/ai/conversations" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Author lookup for Jane Doe","messages":[{"role":"user","content":"Find author profile for Jane Doe"}],"language":"en-US"}'
```

4) Ask the AiController for an author profile (fallback)

```bash
curl -X POST "https://localhost:5001/api/ai/authorprofile" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"name":"Jane Doe","source":"fallback","language":"en-US"}'
```

Configuration & secrets (developer tips)
- Ollama: default base URL `http://localhost:11434` — set `Ollama:BaseUrl` in WebApi user-secrets or appsettings.
- OpenAI: store `OpenAI:ApiKey` in the Web API user secrets (Web.Blazor may also store a key for client features).
- DB: `ConnectionStrings:DefaultConnection` for SQL Server.

PowerShell example to set Web API secrets (replace values):

```powershell
$proj = ".\\MediaOrganizerApp.WebApi\\MediaOrganizerApp.WebApi.csproj"

dotnet user-secrets --project $proj set "ConnectionStrings:DefaultConnection" "Server=.;Database=Publicacoes_Prod;Trusted_Connection=True;"
dotnet user-secrets --project $proj set "Ollama:BaseUrl" "http://localhost:11434"
```

Suggested next additions (optional, high ROI)
- Add quick API curl examples to the primary README for public visibility.
- Add a Mermaid diagram near the top of README.md describing components and data flows.
- Add screenshot files in `/docs/screenshots/` and reference them from README.md.


---

## ⚙️ Technologies Used

- **Frontend**: Blazor Server
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Dapper
- **Real-Time Communication**: SignalR
- **AI Integration**: OpenRouter (cloud-first) with local Ollama fallback; Cloud Chat with AI usage logging
- **Authentication**: ASP.NET Core Identity
- **Logging**: Serilog
- **Other Tools**: FluentValidation, AutoMapper

---

## 🛠️ Installation

### Prerequisites
1. **.NET SDK 10.0** installed.
2. **SQL Server** with the `Publicacoes_Prod` database.
3. **IIS** for deployment (optional).
4. **Node.js** (if using JavaScript libraries).

### Setup
1. Clone the repository:
```
   git clone https://github.com/fauxtix/MediaOrganiserApp.git
```

2. Navigate to the solution directory:
```
   cd MediaOrganizerApp
```

3. Restore dependencies:
```
   dotnet restore
```

4. Update the `appsettings.json` file with your database connection string.

5. Run the migrations:
```
   dotnet ef database update
```

6. Start the application:
```
   dotnet run
```

---

## 📖 API & Integrations

The solution provides a RESTful API for managing publications and media. Key endpoints include:

- **Publications**: `/api/publications`
- **Media**: `/api/media`
- **Users**: `/api/users`

### Integrations
- **External Storage**: Media files can be stored in Azure Blob Storage or a local file system.
- **AI Metadata Fallback**: Automatically fetch metadata using AI when external APIs fail.

---

## 🔐 User Roles & Data Isolation

### **User Roles**
The **Media Organizer App** uses **Role-Based Access Control (RBAC)** with **automatic data isolation**. The system implements two roles:

| Role            | Data Access                                                                 | System Access |
|-----------------|-----------------------------------------------------------------------------|---------------|
| **Admin**       | **Sees ALL users' data** — publications, media, playlists from every user. | Full system access: user management, app settings, logs, backups, prompts. |
| **GeneralUser** | **Sees ONLY their own data** — publications, media, and playlists are automatically filtered by UserID. | Standard features: create/edit/delete their own publications and media. Cannot access admin pages. |

---

### **How Data Isolation Works**

Every publication and media file is tagged with a `UserID` when created. The repository layer automatically filters queries based on the logged-in user's role:

```csharp
// Example from PublicacoesRepository.cs
if (userRole == UserRole.GeneralUser)
{
    // Only show this user's publications
    sbData.Append(" WHERE IdFormato = @IdFormato AND UserID = @UserID");
    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), 
        new { IdFormato, UserID = iUserID })).ToList();
}
else // Admin
{
    // Show all publications from all users
    sbData.Append(" WHERE IdFormato = @IdFormato ");
    return (await connection.QueryAsync<PubsWithDescriptions>(sbData.ToString(), 
        new { IdFormato })).ToList();
}
```

**Key Points:**
- **GeneralUser** accounts see a personalized catalog containing only their own items.
- **Admin** accounts have oversight and can view/manage data across all users.
- Data isolation is enforced at the **repository level**, not just the UI — ensuring security.

---

### **What Differentiates GeneralUser from Admin?**

#### **GeneralUser**:
- **Data Scope**: Can ONLY see and manage their own publications, media, and playlists.
- **Feature Access**: Full CRUD operations on their own data (create, read, update, delete).
- **Restrictions**: 
  - Cannot see other users' data.
  - Cannot access admin pages: Manage Users, App Settings, View Logs, Backup operations.
- **Perfect for**: Individual users, family members, team members in a shared library.

#### **Admin**:
- **Data Scope**: Can see and manage **ALL users' data** across the entire system.
- **System Control**:
  - **User Management**: Create, edit, delete user accounts via "Manage Users" page.
  - **App Settings**: Configure API keys, storage options, AI prompts, feature flags.
  - **Logs Management**: View application logs for debugging and monitoring.
  - **Backup & Restore**: Perform database and file backups; restore from backups.
  - **Lookup Tables**: Manage shared lookup data (categories, formats, publishers).
- **Perfect for**: System administrator, librarian, collection manager with oversight responsibilities.

---

## ⚙️ Admin Options

### **1. App Settings**
Admins have access to the **App Settings** section, which is divided into two tabs:

#### **Tab 1: General Settings**
- **API Keys**:
  - Configure API keys for external integrations (e.g., YouTube API, Azure Blob Storage).
- **Storage Options**:
  - Choose between local storage and cloud storage for media files.
- **Default Categories**:
  - Set default categories for publications and media files.
- **User Registration**:
  - Enable or disable user self-registration.

#### **Tab 2: Security Settings**
- **Authentication Policies**:
  - Configure password policies (e.g., minimum length, complexity).
  - Enable or disable Multi-Factor Authentication (MFA).
- **Role Management**:
  - Add, edit, or delete user roles.
- **Session Timeout**:
  - Set session timeout duration for inactive users.

---

### **2. Logs Operations**
Logs are critical for monitoring the health and performance of the application. Admins can access the **Logs Operations** section to:

- **View Logs**:
  - Access detailed logs for user activities, system events, and errors.
  - Filter logs by date, severity, or category.
- **Export Logs**:
  - Export logs to a file (e.g., CSV, JSON) for external analysis.
- **Error Tracking**:
  - Identify and troubleshoot errors in real-time.
- **Audit Trails**:
  - Track changes made by users, including publication edits and media uploads.

---

### **3. Backup Options**
The **Backup Options** feature ensures data integrity and disaster recovery. Admins can:

#### **Perform Backups**:
- **Database Backup**:
  - Create a backup of the SQL Server database, including all publications, media metadata, and user data.
- **Media Files Backup**:
  - Backup all media files stored locally or in the cloud.
- **Scheduled Backups**:
  - Set up automated backups at regular intervals.

#### **Restore Data**:
- **Restore from Backup**:
  - Restore the database and media files from a previous backup.
- **Selective Restore**:
  - Restore specific publications, media files, or user data.

#### **Why Backups Are Important**:
- **Data Integrity**:
  - Protect against accidental data loss or corruption.
- **Disaster Recovery**:
  - Quickly recover from system failures or cyberattacks.
- **Compliance**:
  - Meet regulatory requirements for data retention and recovery.

---

### **Why These Features Are Critical**
1. **App Settings**:
   - Ensure the application is configured to meet organizational needs.
   - Provide flexibility for integrations and security policies.
2. **Logs Operations**:
   - Enable proactive monitoring and troubleshooting.
   - Ensure accountability with detailed audit trails.
3. **Backup Options**:
   - Protect against data loss and ensure business continuity.
   - Provide peace of mind with automated and manual backup options.

---

## 🤝 Contribution Guidelines

We welcome contributions! To contribute:
1. Fork the repository.
   
2. Create a new branch:
```
   git checkout -b feature/your-feature-name
```

3. Commit your changes:
```
   git commit -m "Add your message here"
```

4. Push to your branch:
```
   git push origin feature/your-feature-name
```

5. Open a pull request.

---

## 📜 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📞 Contact & Support

For questions or support, please contact:
- **Email**: support@mediaorganizerapp.com
- **GitHub Issues**: [Submit an Issue](https://github.com/fauxtix/MediaOrganiserApp/issues)

---

## 🖼️ Screenshots

_Add screenshots here to showcase the application._

---

## 🚀 How to Deploy the Solution in IIS

### 1. Publish the API and Blazor Projects
1. Open the solution in Visual Studio.
2. Right-click the **MediaOrganizerApp.WebApi** project and select **Publish**.
3. Choose **Folder** as the target and publish to a directory (e.g., `C:\Publish\WebApi`).
4. Repeat the process for the **Web.Blazor** project.

### 2. Configure IIS
1. Open **IIS Manager**.
2. Create a new site for the API:
- Site name: `MediaOrganizerAPI`
- Physical path: `C:\Publish\WebApi`
- Binding: `http://localhost:5000`
3. Create another site for the Blazor app:
- Site name: `MediaOrganizerApp`
- Physical path: `C:\Publish\Web.Blazor`
- Binding: `http://localhost:5001`

### 3. Set Up SQL Server Users
1. Open SQL Server Management Studio.
2. Create a new login for the application
```
CREATE LOGIN MediaAppUser WITH PASSWORD = 'YourSecurePassword';
```

3. Map the login to the `Publicacoes_Prod` database
```
USE Publicacoes_Prod; 
CREATE USER MediaAppUser FOR LOGIN MediaAppUser; 
EXEC sp_addrolemember 'db_owner', 'MediaAppUser';
```

### 4. Configure HTTPS with a Trusted Certificate
1. Open IIS Manager and select the site.
2. Click **Bindings** > **Add** > **HTTPS**.
3. Select a trusted certificate or create one using **Let's Encrypt** or **Self-Signed Certificates**.
4. Bind the certificate to the site.

---

## Troubleshooting (IIS Deployment)

### HTTP Error 500.19 (`0x8007000d` or `0x80070002`)

This error occurs when IIS cannot read the `<aspNetCore>` section inside the `web.config` file. It usually happens because the **ASP.NET Core Module V2** was not properly registered in the server's global configuration (`applicationHost.config`), even after installing the *.NET Hosting Bundle* (often due to antivirus blocking, missing administrator privileges, or installation order).

> **Note:** If you are running the application locally via Visual Studio or `dotnet run` (Kestrel/IIS Express), you can safely ignore this section.

#### How to fix:

1. **Repair the Hosting Bundle:**
   * Close *IIS Manager*.
   * Run the **.NET 10 Hosting Bundle** installer as **Administrator** and select **Repair**.
   * Restart IIS by opening a terminal as Administrator and running `iisreset`.

2. **Manual Registration (if Repair fails):**
   If the installer is blocked by security policies or antivirus software and fails to update IIS, open **PowerShell as Administrator** and run the following script to manually register the section and module:

   ```powershell
   $configPath = "C:\Windows\System32\inetsrv\config\applicationHost.config"
   $content = [System.IO.File]::ReadAllText($configPath)

   # 1. Declare the aspNetCore section (fixes 0x8007000d)
   if ($content -notlike "*name=`"aspNetCore`"*") {
       $content = $content -replace '<sectionGroup name="system.webServer">', "<sectionGroup name=`"system.webServer`">`n            <section name=`"aspNetCore`" overrideModeDefault=`"Allow`" />"
   }

   # 2. Clean up old entries and inject the global module with an absolute path (fixes 0x80070002)
   content = content -replace '<add name="AspNetCoreModuleV2"[^>]*>', ''
   \$globalModuleLine = '<add name="AspNetCoreModuleV2" image="C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll" />'
   \$content = \(content -replace '<globalModules>', "<globalModules>`n        $globalModuleLine" $moduleLine = '<add name="AspNetCoreModuleV2" />' $content = $content -replace '<modules[^>]*>', "$&`n        \)moduleLine"

   [System.IO.File]::WriteAllText(configPath, content)
   iisreset
   ```

3. **Folder Permissions:**
   Ensure the *Application Pool* identity (`IIS APPPOOL\<PoolName>`) has **Read & Execute** permissions (or **Modify**, if the app handles file uploads) on the physical deployment folder.

---
## 🎉Thank you for using Media Organizer App!

