# User Role Data Flow Architecture

This diagram illustrates how data isolation works in the Media Organizer App based on user roles.

## Multi-User Data Flow with Role-Based Isolation

```mermaid
flowchart TD
	Start([User Login]) --> Auth{Authenticate}
	Auth -->|Success| SetSession[Set SessionParameters<br/>- UserID<br/>- UserRole<br/>- UserName]
	Auth -->|Failure| End([Access Denied])

	SetSession --> CheckRole{Check UserRole}

	CheckRole -->|Admin| AdminFlow[Admin Flow]
	CheckRole -->|GeneralUser| UserFlow[GeneralUser Flow]

	AdminFlow --> AdminQuery[Repository Query]
	AdminQuery --> AdminSQL["SQL: SELECT * FROM Publicacao<br/>WHERE IdFormato = @IdFormato<br/>(No UserID filter)"]
	AdminSQL --> AdminData[Returns ALL users' data]
	AdminData --> AdminUI[Display All Publications<br/>+ System Controls]
	AdminUI --> AdminFeatures["Access:<br/>- Manage Users<br/>- App Settings<br/>- View Logs<br/>- Backups<br/>- ALL data"]

	UserFlow --> UserQuery[Repository Query]
	UserQuery --> UserSQL["SQL: SELECT * FROM Publicacao<br/>WHERE IdFormato = @IdFormato<br/>AND UserID = @UserID"]
	UserSQL --> UserData[Returns ONLY this user's data]
	UserData --> UserUI[Display User's Publications Only]
	UserUI --> UserFeatures["Access:<br/>- Own Publications<br/>- Own Media<br/>- Own Playlists<br/>- No Admin Pages"]

	AdminFeatures --> Complete([Session Active])
	UserFeatures --> Complete

	style Start fill:#e1f5ff
	style End fill:#ffebee
	style Complete fill:#e8f5e9
	style AdminFlow fill:#fff3e0
	style UserFlow fill:#f3e5f5
	style AdminSQL fill:#ffccbc
	style UserSQL fill:#d1c4e9
	style AdminFeatures fill:#ffe0b2
	style UserFeatures fill:#e1bee7
```

## Data Isolation at Entity Level

```mermaid
erDiagram
	USER ||--o{ PUBLICACAO : "owns (UserID)"
	USER ||--o{ MEDIA : "creates (CreatedBy)"
	USER ||--o{ PLAYLIST : "creates"

	USER {
		int Id PK
		string UserName
		string Email
		string Role "Admin or GeneralUser"
	}

	PUBLICACAO {
		int Id PK
		int UserID FK "Data isolation key"
		string Titulo
		string Autor
		int IdCategoria
		string ImagemCapa
		string CreatedBy
	}

	MEDIA {
		int Id PK
		string FileName
		string FilePath
		string CreatedBy "Data isolation key"
		int TipoMedia
	}

	PLAYLIST {
		int Id PK
		string Name
		int MediaType
		int UserId FK "Data isolation key"
	}
```

## Repository-Level Filtering Process

```mermaid
sequenceDiagram
	participant UI as Blazor UI
	participant Service as API Service
	participant Repo as Repository
	participant Session as SessionParameters
	participant DB as SQL Server

	UI->>Service: Request Publications List
	Service->>Repo: GetAllPubs_Extended()
	Repo->>Session: Read UserRole & UserID
	Session-->>Repo: Role=GeneralUser, UserID=42

	alt User is GeneralUser
		Repo->>DB: SELECT * FROM Publicacao<br/>WHERE UserID = 42
		DB-->>Repo: Returns User 42's publications only
	else User is Admin
		Repo->>DB: SELECT * FROM Publicacao<br/>(No filter)
		DB-->>Repo: Returns ALL users' publications
	end

	Repo-->>Service: Filtered Results
	Service-->>UI: Display Publications

	Note over UI,DB: Data isolation enforced at repository level<br/>Security is NOT just UI-based
```

## Admin vs GeneralUser Feature Access

```mermaid
mindmap
  root((Media Organizer<br/>Multi-User System))
	Admin Role
	  Data Access
		ALL users' publications
		ALL users' media
		ALL playlists
		System-wide reports
	  Features
		User Management
		  Create users
		  Edit users
		  Delete users
		  Assign roles
		System Settings
		  App Settings
		  API Keys
		  Storage Options
		Monitoring
		  View Logs
		  Export Logs
		  Audit Trails
		Maintenance
		  Database Backup
		  Restore Operations
		  File Integrity Checks
		Lookup Tables
		  Categories
		  Formats
		  Publishers
	GeneralUser Role
	  Data Access
		OWN publications only
		OWN media only
		OWN playlists only
		Personal reports
	  Features
		Publications
		  Create own
		  Edit own
		  Delete own
		  Search/filter own
		Media
		  Upload own files
		  Edit own media
		  Create playlists
		  YouTube integration
		Documents
		  Edit .docx files
		  Print documents
		  Manage own files
		AI Tools
		  Chat/Conversations
		  Metadata retrieval
		  Author lookup
```

## Use Case Scenarios

### Scenario 1: Family Collection
```mermaid
graph LR
	A[Father<br/>Admin] -->|Manages| SYS[Media Organizer System]
	B[Mother<br/>GeneralUser] -->|Uses| SYS
	C[Son<br/>GeneralUser] -->|Uses| SYS
	D[Daughter<br/>GeneralUser] -->|Uses| SYS

	SYS -->|Shows ALL data| A
	SYS -->|Shows ONLY Mother's books| B
	SYS -->|Shows ONLY Son's media| C
	SYS -->|Shows ONLY Daughter's docs| D

	style A fill:#ff9800
	style B fill:#4caf50
	style C fill:#2196f3
	style D fill:#9c27b0
```

### Scenario 2: Small Library
```mermaid
graph TD
	LIB[Library System]

	ADMIN[Head Librarian<br/>Admin Role]
	STAFF1[Staff Member 1<br/>GeneralUser]
	STAFF2[Staff Member 2<br/>GeneralUser]
	STAFF3[Staff Member 3<br/>GeneralUser]

	ADMIN -->|Oversees all catalogs| LIB
	STAFF1 -->|Catalogs Fiction| LIB
	STAFF2 -->|Catalogs Non-Fiction| LIB
	STAFF3 -->|Catalogs Media| LIB

	LIB -->|View: ALL cataloged items| ADMIN
	LIB -->|View: Only Fiction entries| STAFF1
	LIB -->|View: Only Non-Fiction entries| STAFF2
	LIB -->|View: Only Media entries| STAFF3

	ADMIN -->|Generate consolidated reports| REPORTS[System Reports]

	style ADMIN fill:#ff5722
	style LIB fill:#607d8b
	style REPORTS fill:#ffc107
```

## Security Model

```mermaid
flowchart LR
	REQ[HTTP Request] --> AUTH[ASP.NET Identity<br/>Authentication]
	AUTH --> CLAIMS[Extract User Claims<br/>- UserId<br/>- UserName<br/>- Role]

	CLAIMS --> SESSION[SessionParameters<br/>Static Context]
	SESSION --> REPO[Repository Layer<br/>Data Access]

	REPO --> FILTER{Role-Based<br/>Query Filter}

	FILTER -->|Admin| ALLDATA[Query ALL data]
	FILTER -->|GeneralUser| USERDATA[Query WHERE<br/>UserID = current]

	ALLDATA --> DB[(SQL Server)]
	USERDATA --> DB

	DB --> RESULT[Filtered Result Set]
	RESULT --> API[API Response]
	API --> UI[Blazor UI]

	style AUTH fill:#4caf50
	style SESSION fill:#2196f3
	style FILTER fill:#ff9800
	style DB fill:#9c27b0
```

---

## Key Takeaways

1. **Data Isolation is Automatic**: The repository layer enforces data filtering based on `SessionParameters.UserRole` and `SessionParameters.UserID`.

2. **Security at Database Level**: Not just UI permissions—actual SQL queries are different for Admin vs GeneralUser.

3. **Admin Oversight**: Admin users can see and manage all data across all users, enabling system-wide management and reporting.

4. **User Privacy**: GeneralUser accounts only see their own data—other users' collections are invisible to them.

5. **Multi-Tenant Ready**: The architecture supports multiple independent users/teams using the same system instance.

---

## Code Reference

See these files for implementation details:
- `Application/AppGlobal/SessionParameters.cs` - User context storage
- `Application/Enums/Common.cs` - UserRole enum definition
- `MediaOrganizerApp.Persistence/Repositories/RawSQL_Generic/PublicacoesRepository.cs` - Repository filtering logic
- `Domain/Entities/Publicacao.cs` - Entity with UserID field
- `Web.Blazor/Pages/Admin/ManageUsers.razor` - Admin-only user management UI
