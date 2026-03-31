# 📚 Media Organiser App

[Versão em Português](README-PT.md)

Have you ever felt frustrated with managing your media like hundreds of media scattered across different storage mediums/ hard disks? Wouldn't it be great to have it organized, allowing for easy management?

Media Organizer App is an application to organize your media (Videos and MP3 files) and Documents (PDFs and Word documents).  
It also allows you to organize collections of Books, LPs, CDs, DVDs, and documents written on paper (filing cabinets, for example).

This application is designed to streamline the organization, management, and retrieval of media and publication data with a user-friendly interface and robust backend.

---

## ✨ Key Features

### 📚 Publications Management
- **CRUD Operations**: Create, read, update, and delete publications with detailed metadata.
- **Bulk Import/Upload**:
  - Upload publications in bulk using CSV, Excel, or JSON files.
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

### 🖼️ Media Management
- **File Uploads**: Drag-and-drop file uploads for images, PDFs, videos, and audio files.
  - **Progress Tracking**: Real-time progress bar for file uploads.
- **Preview Media**: Inline previews for supported file types.
- **Link Media to Publications**: Associate media files with specific publications.
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

### 🤖 AI Fallback
- **Metadata Retrieval**: Automatically fetch metadata (e.g., author profiles) using AI when external API calls fail.
- **Ollama Integration**: Utilized for generating author profiles and metadata.
  - **Author Profiles**: Fetches detailed author information using AI prompts.
  - **Customizable Prompts**: Supports dynamic queries for personalized results.

### 🔧 Additional server & utility features

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

### 🔐 User Management & Authorization
- **Role-Based Access Control**: Assign roles (Admin, Editor, Viewer) to users.
- **User Profiles**: Manage user profiles, including personal details and preferences.
- **Activity Logs**: Track user activities for accountability.
- **Authentication**: Secure login using ASP.NET Core Identity with optional MFA.

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
| **Data Integrity**         | Tools to find publications with missing files and store PDFs/PNGs flagged as not deleted. |
| **View Logs**              | View application logs for debugging and monitoring.                        |
| **Edit Prompts**           | Edit AI prompts for metadata generation.                                   |
| **Reports**                | Generate and download reports for publications and media.                  |
| **User Management**        | Manage user roles, permissions, and profiles.                              |

---

## ⚙️ Technologies Used

- **Frontend**: Blazor WebAssembly
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Dapper
- **Real-Time Communication**: SignalR
- **AI Integration**: OpenAI API for metadata fallback
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
   git clone https://github.com/fauxtix/MediaOrganiserApp.Web.Blazor.git
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

## 🔐 User Roles & Authorization

### **User Roles**
The **Media Organizer App** uses **Role-Based Access Control (RBAC)** to differentiate between user types and their permissions. Below is a detailed breakdown of the roles:

| Role       | Permissions                                                                 |
|------------|-----------------------------------------------------------------------------|
| **Admin**  | Full access to all features, including user management, app settings, logs, and backups. |
| **Editor** | Manage publications and media files.                                        |
| **Viewer** | Read-only access to publications and media.                                 |

---

### **What Differentiates Normal Users from Admins?**

#### **Normal Users (Editors & Viewers)**:
- **Editors**:
  - Can create, update, and delete publications and media files.
  - Cannot access system-level settings, logs, or backups.
- **Viewers**:
  - Can only view publications and media files.
  - Cannot make any changes or access advanced features.

#### **Admins**:
- **Full System Control**:
  - Manage all publications, media, and user accounts.
  - Access advanced system settings and configurations.
- **Logs Management**:
  - View and analyze application logs for debugging and monitoring.
- **Backup & Restore**:
  - Perform backups of the database and media files.
  - Restore data from backups in case of system failure.
- **App Settings**:
  - Configure application-wide settings, including API keys, storage options, and security policies.

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

## ⚙️ Installation

### Prerequisites
1. **.NET SDK 10.0** installed.
2. **SQL Server** with the `Publicacoes_Prod` database.
3. **IIS** for deployment (optional).
4. **Node.js** (if using JavaScript libraries).

### Setup
1. Clone the repository:
```
   git clone https://github.com/fauxtix/MediaOrganiserApp.Web.Blazor.git
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
- **GitHub Issues**: [Submit an Issue](https://github.com/fauxtix/MediaOrganiserApp.Web.Blazor/issues)

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

## 🎉 Thank You for Using Media Organizer App!

# Demo

https://github.com/fauxtix/MediaOrganiserApp.Backend/assets/49880538/7eb85ef6-0a38-4d9e-af2f-1c374fde4801
