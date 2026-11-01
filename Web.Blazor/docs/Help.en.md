# User Manual — Media Organizer

> Complete guide to the **Media Organizer** application: managing publications (books, e-books, and documents), authors, media (audio, video, and YouTube), galleries, artificial intelligence features, lookup tables, administration, reports, and tools.
>
> This manual is intended for **end users** and **administrators**. No programming knowledge is required.
>
> 💡 The manual is available **inside the application**: click the help button (`?`) in the **top bar** to open it in a dialog without leaving the page you are on.

---

## Table of Contents

- [0. Quick Start — get started in 5 minutes](#0-quick-start--começar-em-5-minutos)
- [1. Overview](#1-visão-geral)
- [2. Getting Started](#2-primeiros-passos)
- [3. Dashboard (Home)](#3-dashboard-home)
- [4. Publications (books, e-books, and documents)](#4-publicações-livros-e-books-e-documentos)
- [5. Authors](#5-autores)
- [6. Media (Audio, Video, and YouTube)](#6-média-áudio-vídeo-e-youtube)
- [7. Galleries](#7-galerias)
- [8. Artificial Intelligence](#8-inteligência-artificial)
- [9. Lookup Tables](#9-tabelas-de-apoio)
- [10. Administration](#10-administração)
- [11. Reports](#11-relatórios)
- [12. Tools](#12-ferramentas)
- [13. Tips and Shortcuts](#13-dicas-e-atalhos)
- [14. Troubleshooting](#14-resolução-de-problemas)
- [15. Glossary](#15-glossário)
- [16. Complete scenarios (step by step)](#16-cenários-completos-passo-a-passo)
- [Appendix A — Main routes](#apêndice-a--rotas-principais)
- [Appendix B — Periodic maintenance checklist](#apêndice-b--checklist-de-manutenção-periódica)
- [Appendix C — Frequently asked questions (FAQ)](#apêndice-c--perguntas-frequentes-faq)

---

## 0. Quick Start — get started in 5 minutes {#0-quick-start--começar-em-5-minutos}

If this is your first time using the application, start with these essential steps.

### 0.1 Sign in to the application

1. Open the application address in your browser (in a local environment, usually `https://localhost:4110`).
2. Enter your account **email** and **password**.
3. Click **Sign in**.
4. If you do not have an account yet, use the **register** link on the authentication screen and wait for the user account to be created (in some installations, the administrator creates accounts).

### 0.2 Choose a language

1. Open **Settings** (at the bottom of the sidebar) → **Language**.
2. Choose **Portuguese (default)**, **English**, **French**, or **Spanish**.
3. Your choice is saved and applied throughout the application (the page reloads).

> 💡 The **top bar** does **not** have a language selector: it only shows the current language (globe icon + `PT`/`EN`/`FR`/`ES`), along with your username, the help button (`?`), the theme button, and **Sign out**.

### 0.3 Choose a theme

1. In the top bar, click the **theme** icon (sun/moon).
2. The application switches between **light mode** and **dark mode**.
3. The top bar and sidebar are always dark—this is intentional.

### 0.4 Create or import your first publication

1. In the sidebar, open **Publications → Management** (`/publicacoes`).
2. To create a record manually (for example, for a printed book), click **New** (the green `+` button), fill in at least **Title**, **Author**, **Category**, **Publisher**, **Format**, and **File location**, then click **Save**.
3. To import a document from your PC (PDF, DOCX, or Outlook `.msg` message), use **Import PDF/DOCX**, select the file, and let the application read its **metadata**.
4. Always **review** automatically populated data before saving.

> 💡 **Publications** are not just for books: you can also manage **e-books (PDFs)**, **text documents (DOCX)**, **Outlook messages**, and other documents. The form is the same; what changes most is the data source (manual entry or import).

### 0.5 Create your first author

1. Open **Publications → Authors** (`/autores`).
2. Click **New**.
3. Enter the **name** and, if possible, a **biography**.
4. **Save**.
5. Optional: upload a **photo** and use AI to generate/complete the **author profile**.

### 0.6 Add media (audio/video)

1. Open **Media → Management** (`/media`).
2. Click **New** to create a record manually, or **Upload** to upload files.
3. Choose the type (audio, video, or YouTube), enter the details, and **Save**.

### 0.7 Chat with AI

1. Open **AI → Chat** (`/ai/openrouter-chat`) for cloud chat, or **AI → Chat (Ollama)** (`/ai/chat`) for local chat.
2. Type your question and press **Enter**.
3. The response streams in; you can **copy** it, rate it with **👍/👎**, and view your history.

### 0.8 “I want to do X → go to Y” table

| I want to… | Go to… | Route |
|---|---|---|
| See the overview | Home | `/` |
| Manage publications (books, e-books, documents) | Publications → Management | `/publicacoes` |
| View covers in a gallery | Publications → Gallery | `/imagegallery` |
| Manage authors | Publications → Authors | `/autores` |
| Manage media | Media → Management | `/media` |
| View videos | Media → Gallery | `/videogallery` |
| Create playlists | Media → Playlists | `/media/playlists` |
| Edit AI prompts | AI → Prompt Editor | `/edit-prompts` |
| Cloud chat | AI → Chat | `/ai/openrouter-chat` |
| Local chat | AI → Chat (Ollama) | `/ai/chat` |
| Manage users | Maintenance → Users | `/admin/manageusers` |
| Manage lookup tables | Maintenance → Lookup Tables | `/admin/managelookuptables` |
| View logs | Logs | `/admin/viewapplogs` |
| Manage files | File Manager | `/admin/filemanager` |
| Back up the database | Backup → Database | `/backupdatabase` |
| Settings | Menu footer → Settings | `/admin/appsettings2` |

---

## 1. Overview {#1-visão-geral}

### 1.1 What is Media Organizer?

Media Organizer is a **personal/organizational catalog** application that brings together, in one place:

- **Publications** — a mixed collection of **printed books, e-books (PDFs), text documents (DOCX), Outlook messages (`.msg`)**, and other documents, with cover, synopsis, ISBN, publisher, category, format, file location, and personal rating. You can create records manually or **import** a file and let the application populate the data from its metadata.
- **Authors** — complete profiles with biographies, photos, and book lists.
- **Media** — audio, video, and YouTube links, organized into playlists and played within the application.
- **Galleries** — visual views of images, videos, and music.
- **Artificial intelligence** — translation, summaries, metadata, author profiles, and two chats (cloud and local).

### 1.2 How the application is set up (without technical details)

The application has two parts that work together:

1. **Service/API** — stores and processes data (database, files, AI).
2. **Interface** — what you see in your browser.

To users, it behaves like a single website. In a local environment, the two components may use different ports; if a page does not load, see [Troubleshooting](#14-resolução-de-problemas).

### 1.3 User roles

There are two roles:

| Role | What they can see | What they can do |
|---|---|---|
| **Admin** | **All** data from all users | Everything: user management, lookup tables, settings, logs, backups, and deleting everyone’s history |
| **GeneralUser** | **Only their own** data | Create/edit/delete their publications, authors, media, and playlists; use AI; manage their chat history |

> ⚠️ **Important:** data isolation is enforced on the server. A GeneralUser **cannot** see another user's records, even by changing the page address. If a record has “disappeared,” make sure you are signed in to the correct account.

### 1.4 Navigation map

```
Home (/)
├── Publicações
│   ├── Gestão (/publicacoes)
│   ├── Galeria (/imagegallery)
│   └── Autores (/autores)
├── Media
│   ├── Gestão (/media)
│   ├── Galeria (/videogallery)
│   └── Playlists (/media/playlists)
├── AI
│   ├── Editor de Prompts (/edit-prompts)
│   ├── Chat (Ollama) (/ai/chat)
│   └── Chat (/ai/openrouter-chat)
├── [Admin] Backup
│   ├── Base de Dados (/backupdatabase)
│   ├── Uploads Publicações (/backupuploads/1)
│   ├── Uploads Media (/backupuploads/2)
│   └── Uploads Imagens (/backupuploads/3)
├── [Admin] Manutenção
│   ├── Utilizadores (/admin/manageusers)
│   └── Tabelas Auxiliares (/admin/managelookuptables)
├── [Admin] Gestor de Ficheiros (/admin/filemanager)
├── [Admin] Logs (/admin/viewapplogs)
└── Definições (/admin/appsettings2)   ← rodapé do menu
```

### 1.5 Where things are stored

- **Records** (publications, authors, media, playlists, conversations, ratings) — in the database.
- **Files** (PDFs, covers, author images, audio, video) — in folders configured in Settings.
- **Local preferences** (theme) — in the browser; the **language** is set in **Settings → Language** and applied through a culture cookie.
- **AI configuration** — in the database, editable in the **AI** area of Settings.

---

## 2. Getting Started {#2-primeiros-passos}

### 2.1 Authentication

#### Sign in

1. Open the application.
2. Enter your **email** and **password**.
3. Click **Sign in**.
4. If your credentials are correct, you will go directly to the **Dashboard**.

#### Reset your password

1. On the authentication screen, click **Forgot password**.
2. Enter the email address associated with your account.
3. Follow the link sent by email.
4. Set a new password.

> 💡 If the password reset email does not arrive, check your spam folder or contact the administrator (email delivery depends on the server configuration).

#### Sign out

- Use the **Sign out** option in the top bar. Your session will end and you will return to the authentication screen.

### 2.2 Languages

The application is available in **four languages**:

| Language | Code | Notes |
|---|---|---|
| Portuguese | `pt` | **Default** language |
| English | `en` | — |
| French | `fr` | — |
| Spanish | `es` | — |

**To change the language:**

1. Open **Settings** (at the bottom of the sidebar) → **Language** section.
2. Choose your preferred language from the list.
3. The application saves your choice and reloads the interface in the new language.

> 💡 The **top bar does not have a language selector**. It only shows an indicator for the current language (globe + `PT`/`EN`/`FR`/`ES`), along with your username, the help button (`?`), the theme button, and **Sign out**. Change the language in **Settings**.

**What is translated:** menus, buttons, labels, validation messages, and interface text.

**What is not translated:** the **content** you enter (titles, synopses, biographies) and AI model names. You can use the interface in English while your publications are in Portuguese—that is normal.

> 💡 Your choice is saved in Settings and applied throughout the application (it is not a separate preference for each tab).

### 2.3 Light/dark theme

1. Click the **theme** icon in the top bar.
2. The application switches instantly without reloading.
3. Your preference is remembered in the browser.

**Important visual rules:**

- The **sidebar** and **top bar** are **always dark**, in both themes.
- Cards, grids, forms, and dialogs follow the selected theme.
- Dashboard charts also adapt.

> 💡 If you notice a visual artifact on a chart after moving the pointer over it, simply move the pointer away and back again; the application automatically corrects the repaint.

### 2.4 Sidebar layout

The sidebar has three areas:

1. **Top** — general items: Home, Publications, Media, AI.
2. **Middle** — administrative items (Admin only): Backup, Maintenance, File Manager, Logs.
3. **Bottom** — **Settings**.

**Behavior:**

- Groups with child items expand/collapse when clicked.
- The active item is highlighted.
- On small screens, the sidebar can be collapsed.

### 2.5 Top bar

Typically contains:

- The application **name/brand**.
- The **current language** (globe icon + `PT`/`EN`/`FR`/`ES`) — this is only an indicator; change the language in **Settings**.
- Your **username**.
- The **help** button (`?`), which opens this manual in a dialog without leaving the current page.
- The **theme button**.
- The **session** option (sign out).

### 2.6 First-time setup checklist

- [ ] I was able to sign in.
- [ ] I chose a language.
- [ ] I chose a theme.
- [ ] I created or imported at least one book/document.
- [ ] I created at least one author.
- [ ] I confirmed that the essential lookup tables have data (categories, formats, publishers).
- [ ] I tried an AI translation or summary.
- [ ] I opened the chat and sent a message.

---

## 3. Dashboard (Home) {#3-dashboard-home}

The Dashboard is the home page (`/`) and provides an overview of your collection and activity.

### 3.1 Total count cards

Cards with counts appear at the top, for example:

- **Publications**.
- **Authors**.
- **Publishers**.
- **Formats**.
- **Media**.

Each card is clickable (where applicable) and takes you to the corresponding page.

### 3.2 Pie charts

Charts show distributions by:

- **Publication categories**.
- **Publication formats**.
- **Publishers**.
- **Media categories**.
- **Media formats**.

**How to read them:**

- Each slice represents a category, format, or publisher.
- Hover over a slice to see its name and value.
- Very small slices may be grouped visually; check the lookup tables for details.

> 💡 If a chart is empty, there are not enough records (or the records do not have that classification filled in).

### 3.3 “Health” section (data quality)

Indicators for incomplete records or issues:

| Indicator | Meaning | What to do |
|---|---|---|
| **Publications without covers** | Publications without a cover image | Open each publication and upload a cover |
| **Publications without page counts** | Publications without a page count | Complete the **Pages** field |
| **Authors without publications** | Authors with no associated publications | Associate publications or remove orphaned authors |
| **Publishers without publications** | Unused publishers | Reuse them or remove them from the lookup tables |
| **Categories without publications** | Unused categories | Reuse them or remove them |
| **Formats without publications** | Unused formats | Reuse them or remove them |
| **File locations without publications** | Unused locations | Reuse them or remove them |
| **Duplicate titles** | Possible duplicates | Check and consolidate records |

> ⚠️ These indicators are **suggestions**, not errors. A record may be incomplete on purpose.

### 3.4 “Activity” section

AI/chat usage metrics:

- **Conversations** — number of conversations.
- **Total responses**.
- **Last 7 days**.
- **Average latency**.
- **Tokens/second** (with an explanatory note that this is an approximate measure).
- **Average token count**.
- **Percentage of cloud usage**.
- **Most-used models** (table).

### 3.5 “Internal AI usage” section

Shows AI usage **outside chat** (translations, summaries, metadata, profiles, etc.):

- Total operations.
- Last 7 days.
- Calls through **OpenRouter**.
- Calls through **Ollama** (local).
- **Fallbacks** (how many times the cloud failed and local service was used).
- Table **by operation** (translation, summary, metadata, etc.).
- Table of **most recent records** with operation, provider, context, scenario, duration, and date.

**How to interpret this:**

- Many **fallbacks** = the cloud is unstable, has no key, or has no available balance; the app is using Ollama.
- Long durations = local models on modest machines or lengthy prompts.

### 3.6 “Audit” section

- **Recent sign-ins**.
- **Recent errors** (with level and message).
- Total error count.

> 💡 For full details, open **Logs** (`/admin/viewapplogs`).

### 3.7 Refreshing the data

The Dashboard loads when you open the page. If you made changes in another tab, reload the page (F5) to see the updated numbers.

---

## 4. Publications {#4-publicações-livros-e-books-e-documentos}

Management page: **Publication Management** (`/publicacoes`).

### 4.1 The publications grid

The grid lists the publications you can access, with columns such as title, author, publisher, category, format, year, rating, and reading status.

**Available operations:**

| Action | How |
|---|---|
| Sort | Click a column heading |
| Filter | Use the filter row under the heading |
| Search | Use the grid search box |
| Group | Drag a heading to the grouping area |
| Resize columns | Drag the edge of a heading |
| Page through results | Use the controls at the bottom of the grid |

### 4.2 Create a publication

1. Click **New** (the green `+` button).
2. Fill in the form (see [4.4 Fields](#44-campos-do-formulário)).
3. Click **Save**.
4. The record appears in the grid.

> 💡 **Title** and **Author** are the most important fields. To **save** a publication, the application also requires **Category**, **Publisher**, **Format**, and **File location**—create them in the Lookup Tables first if they are missing.

### 4.3 Edit and delete

**Edit:**

1. In the publication row, click the **pencil** icon (Edit).
2. Change the fields.
3. **Save**.

**Delete:**

1. In the row, click the **trash** icon (Delete).
2. Confirm in the dialog.

> ⚠️ Deletion is permanent. If the publication has associated files, confirm whether you also want to remove them from disk (the application will warn you when applicable).

### 4.4 Form fields {#44-campos-do-formulário}

| Field | Required | Description |
|---|---|---|
| **ID** | — | Generated automatically; read-only |
| **ISBN** | No | Publication identifier; useful for Google Books searches |
| **Movement date** | No | Acquisition/record date |
| **Title** | **Yes** | Publication name (2 to 256 characters) |
| **Edition year** | No | If entered, it must have **4 digits** and cannot be later than the current year; AI can suggest it |
| **Pages** | No | Number of pages |
| **Price** | No | Monetary value |
| **Author** | **Yes** | Select from the author list (searchable) |
| **Category** | **Yes** | Publication category |
| **Publisher** | **Yes** | Publisher |
| **Format** | **Yes** | Physical/digital format |
| **File location** | **Yes** | Where the copy/file is stored |
| **Rating** | No | Personal rating (stars) |
| **Read** | No | Mark whether you have read it |
| **Synopsis** | No | Description/summary; supports AI translation |

**Notes about selectors:**

- The **Author**, **Category**, **Publisher**, **Format**, and **File location** fields are searchable—type part of a name to filter.
- If an option is missing, create it in **Lookup Tables** first (or ask the Admin).

### 4.5 Import publications from files (PDF/DOCX)

1. On the management page, use the **Import PDF/DOCX** button.
2. Select one or more files.
3. The application creates/updates records based on the content.
4. Review the created records and correct anything necessary.

> 💡 Importing attempts to extract the title, author, and other data. **Always review** the results—automatic extraction is not perfect.

### 4.6 Read a publication in the application

1. In the publication row, click the **Read PDF** command.
2. The viewer opens the associated file.
3. You can **edit the publication** from the viewer or **close** it.
4. Word documents have their own viewer, with the same actions.

> ⚠️ If the file does not exist at the specified location, the application will warn you. Use the diagnostic pages (see [4.9](#49-ficheiros-em-falta-e-caminhos-errados)).

### 4.7 Google Books search

1. In the publication form (or search flow), open **Search Online**.
2. Enter the **title** and/or **author**.
3. The application searches **Google Books**.
4. Choose a result to populate the fields.
5. If the result has no description, the application uses the available **snippet**.
6. If there are no results, AI may be used to generate a summary.
7. Confirm and **save**.

**Best practices:**

- Search using the **exact title** (without quotation marks) and the author's **surname**.
- If the publication is a translation, try the original title.
- Always check the **year** and **publisher** before saving.

### 4.8 AI features for publications

You can use the following features in the publication form:

| Feature | What it does | Where the result appears |
|---|---|---|
| **AI Summary** | Generates a synopsis from the title/author/details | **Synopsis** field |
| **Translate** | Translates the synopsis/text between languages | Translation dialog → **Confirm** |
| **AI Edition Year** | Suggests the edition year | **Edition year** field |
| **Author’s Books** | Lists/generates books by the selected author | **Author’s Books** panel |

For details on how this works (including the **model chip** and **Cancel** button), see [8. Artificial Intelligence](#8-inteligência-artificial).

### 4.9 Missing files and incorrect paths {#49-ficheiros-em-falta-e-caminhos-errados}

Two diagnostic pages help keep your collection consistent:

#### Publications not found on disk (`/pubsnotfoundondisk`)

- Lists publications whose **referenced file does not exist**.
- Common actions: correct the path, upload the file again, or remove the record.

#### Publications with incorrect paths (`/pubswithwrongpath`)

- Lists publications whose paths point to an invalid/old location.
- Common actions: update the **File location**, move the file, or correct the folder configuration.

> 💡 Run these checks after moving folders, changing computers, or restoring backups.

### 4.10 Statistics and “author’s books”

- The **Statistics** button opens a summary with counts and distributions.
- The **Author’s Books** button opens a panel with books by the selected author, making it easy to navigate between works.

### 4.11 Publication best practices

- Fill in **Author**, **Category**, **Publisher**, and **Format** to improve the Dashboard charts.
- Use the **ISBN** to avoid duplicates.
- Name files consistently (for example, `ISBN - Title.pdf`).
- Keep **covers** in the configured folder.
- Review the Dashboard **Health** section regularly.

---

## 5. Authors {#5-autores}

Page: **Publications → Authors** (`/autores`).

### 5.1 Author list

The grid shows the authors you can access, with their names, book counts, and photos (when available).

**Operations:**

- Search by name.
- Sort and filter.
- Open a profile (**Edit**).
- View in **read-only** mode.
- Delete (with confirmation).

### 5.2 Create an author

1. Click **New**.
2. Enter the **Name** (required, 2 to 128 characters).
3. Enter **About** (biography; optional, but recommended).
4. Enter the **date of birth** (optional).
5. Upload a **photo** (optional).
6. **Save**.

### 5.3 Author profile

The profile (edit form) includes:

| Field | Required | Description |
|---|---|---|
| **Name** | **Yes** | Author’s name (2 to 128 characters) |
| **Date of birth** | No | Author’s date of birth |
| **About / Biography** | No | Free text (formatted text editor); supports AI translation |
| **Photo** | No | Author’s image |
| **Books** | — | List of associated works |
| **AI Profile** | — | Profile generated/completed by AI |

### 5.4 Author photo

1. Use the **upload** area in the profile.
2. Select an image (common formats: JPG, PNG).
3. Wait for the upload to finish.
4. The image appears in the profile and list.

> 💡 Use square or portrait images with good resolution for the best presentation.

### 5.5 Biography and translation

1. Write or paste the biography.
2. Use **Translate** to convert it to another language.
3. Review the text in the dialog.
4. Click **Confirm** to apply it.

> ⚠️ Translation replaces the field text when you confirm. If you want to keep the original, copy it to a text editor first.

### 5.6 Author’s books

- Open **Author’s Books** in the profile or list.
- You can **associate** existing books and **create** new books already linked to the author.
- The list reflects what is in the database for that author.

### 5.7 Generate an AI profile

1. Open the author profile.
2. Click **Generate AI Profile** (or equivalent).
3. The application tries the **cloud** (OpenRouter) first and uses **local Ollama** if that fails.
4. The result is displayed for review.
5. Edit it if necessary and **save**.

**What AI may use:** name, existing biography, and known works.

> 💡 Always review the result: AI may invent dates or works. The profile is a **starting point**, not a source of truth.

### 5.8 Read-only view

The **AuthorReadOnly** view displays the profile without allowing edits—useful for quick reference or presentation.

### 5.9 Delete authors

1. Click **Delete** in the author row.
2. Confirm.

> ⚠️ If the author has associated books, the application may prevent deletion or display a warning. Resolve the associations first.

---

## 6. Media (Audio, Video, and YouTube) {#6-média-áudio-vídeo-e-youtube}

Management page: **Media → Management** (`/media`).

### 6.1 Media types

The application distinguishes between:

- **Audio** — MP3 and similar formats.
- **Video** — local video files.
- **YouTube** — links to videos hosted on YouTube (no local file).

Each type has its own fields and actions, but they share the same grid and editing pattern.

### 6.2 The media grid

Typical columns: title, artist/author, category, format, rating, type, and actions.

**Actions for each row:**

| Icon | Action |
|---|---|
| ▶ | **Play** (the player appropriate for the type) |
| ℹ | **Information/details** |
| ✏ | **Edit** |
| 🗑 | **Delete** |

### 6.3 Create a media record

1. Click **New**.
2. Choose the **type** (audio, video, or YouTube).
3. Fill in the fields (see the table below).
4. For YouTube, enter the video **link**.
5. **Save**.

**Fields and validation:**

| Field | Required | Notes |
|---|---|---|
| **Title** (file/record name) | **Yes** | 2 to 256 characters |
| **Type** | **Yes** | Audio or video (YouTube is handled separately) |
| **Author/Artist** | **Yes** | — |
| **Media format** | **Yes** | For example, MP3, MP4 (lookup table) |
| **Genre/Category** | **Yes** | Media category (lookup table) |
| **Edition year** | No | If entered, 4 digits and no later than the current year |
| **Movement date** | No | Cannot be a future date |
| **Rating** | No | Stars |
| **Notes/description** | No | Supports AI |

### 6.4 Upload files

1. Click **Upload**.
2. Select one or more files.
3. Choose the destination/type if prompted.
4. Wait for the upload to finish.
5. Review the created records.

**Recommendations:**

- Avoid unnecessarily large files.
- Use file names without problematic special characters.
- Check the **media format** (category) so the charts display correctly.

### 6.5 AI metadata and notes

You can use the following features in a media form/details view:

- **AI Notes** — generates a description/notes about the item.
- **AI Metadata** — suggests data such as genre, year, and artist.
- **Year/edition** — where applicable.

You always **review** the result before saving.

### 6.6 Playlists

Page: **Media → Playlists** (`/media/playlists`).

**Create a playlist:**

1. Click **New**.
2. Enter a **name** and, if you like, a description.
3. **Save**.
4. Open the playlist and **add items** (existing media).

**Manage items:**

- Add/remove items.
- Reorder them (when available).
- Play the entire playlist.

**Play a playlist:**

1. Open the playlist.
2. Click **Play**.
3. The **Playlist Player** (`/media/playlists/{id}/player`) opens with the queue.
4. Use the controls to skip forward/back.

> 💡 If an item does not play, check that the file exists on disk or that the YouTube link is valid.

### 6.7 MP3 player

Page: **`/mp3player`**.

- Plays audio tracks.
- Lets you select a track, pause, skip, and adjust the volume.
- Shows information about the current track.

> 💡 The player works within the page; to keep listening while browsing, open it in a separate browser tab.

### 6.8 YouTube player

Page: **`/youtubeplayer`**.

- Plays videos using the link saved in the record.
- If the link is invalid or the video has been removed, an error appears—correct the link in the record.

### 6.9 Media details

- **Audio details** (`Mp3Detail`) — information and playback.
- **Video details** (`VideoDetail`) — information and playback.

### 6.10 YouTube and subscriptions

When configured, the application includes a **subscriptions** area (`/subscriptions`) for following YouTube channels/links.

### 6.11 Media best practices

- Use consistent **artist** and **category** names.
- Create one playlist per context (for example, “Reading,” “Travel”).
- For YouTube, prefer stable public or “unlisted” links.
- Check the **media formats** in the lookup tables before importing in bulk.

---

## 7. Galleries {#7-galerias}

Galleries provide visual alternatives to the grids.

### 7.1 Image gallery (`/imagegallery`)

**What it shows:** book covers and associated images in a thumbnail grid.

**How to use it:**

1. Open **Publications → Gallery**.
2. Browse the images.
3. Click an image to enlarge it/view details.
4. Use the link to open the corresponding record.

**Behavior:**

- Missing images display a placeholder.
- The gallery respects data isolation (you only see your own items, unless you are an Admin).

### 7.2 Video gallery (`/videogallery`)

**What it shows:** video thumbnails with integrated playback.

**How to use it:**

1. Open **Media → Gallery**.
2. Click a video to play it.
3. Use its details to edit metadata.

### 7.3 Music gallery (`/musicgallery`)

**What it shows:** a **Polaroid-style** view of tracks/albums.

**Interactions:**

- Hovering highlights an item (accent color).
- Clicking selects/plays it.
- The selected item is visually distinct.

> 💡 The music gallery is ideal for browsing album covers; to edit metadata, use the **Media** grid.

### 7.4 When a gallery is empty

If there are no images/videos/tracks, the message **“No data available”** appears. Check:

1. Whether records of the right type exist.
2. Whether image/video files are associated.
3. Whether you are using the correct account/user.

---

## 8. Artificial Intelligence {#8-inteligência-artificial}

AI is one of the application's most feature-rich areas. This section explains **how it works** and **how to use each feature**.

### 8.1 Two-layer architecture (the essentials)

The application uses **two providers**:

1. **Cloud (OpenRouter)** — the **first choice**. Fast, with larger models, but requires an internet connection and an API key and is subject to limits.
2. **Local (Ollama)** — the **fallback**. Runs on the machine, with no per-call cost, but is slower and depends on the hardware.

**Typical flow:**

```
Pedido do utilizador
        │
        ▼
  Cloud (OpenRouter)  ──falha/sem chave/limite──►  Local (Ollama)
        │                                              │
        └──────────── resposta ────────────────────────┘
```

**What counts as a cloud “failure”:** no connection, no key, daily limit reached, HTTP error, or an invalid or empty response.

> 💡 This process is **automatic**. You do not need to choose a provider for each operation—but you **can see which one was used** in the model chip (see [8.3](#83-o-chip-do-modelo)).

### 8.2 Where AI is used

| Operation | Where | Result |
|---|---|---|
| **Translate** | Publication/author forms, translation tool | Translated text |
| **Publication summary** | Publication editing / Google Books search | Synopsis |
| **Edition year** | Publication editing | Suggested year |
| **Media notes** | Media editing | Notes/description |
| **Media metadata** | Media editing | Suggested fields |
| **Author’s books** | Author profile | List of works |
| **Author profile** | Author profile | Profile/biography |
| **Cloud chat** | `/ai/openrouter-chat` | Conversation |
| **Local chat** | `/ai/chat` | Conversation |

### 8.3 The model chip {#83-o-chip-do-modelo}

Whenever an AI operation finishes, a **chip** (small label) may appear showing the model used, for example:

```
⊘ OpenRouter · llama-3.3-70b
```

**What it means:**

- **OpenRouter · model-name** — that model answered from the cloud.
- **Ollama · model-name** — the local model answered.
- **Automatic** — the cloud router chose the model itself (a specific model was not requested).
- **Model being tried** (in the `title`) — appears when the operation was **cancelled**; it indicates which model was in use.

**Why it is useful:** because the application tries several models and may fall back to local service, the actual model cannot be predicted. The chip shows you the response's **provenance**.

> 💡 For translation, the chip appears **below the Translate button**, so it does not compete with the status message (cancellation or confidence).

### 8.4 Translate text

**Where to find it:** book forms (synopsis), author profiles (biography), and the translation tool.

**Step by step:**

1. Open the translation dialog.
2. Choose the **source** and **target** languages.
3. Click **Translate**.
4. While processing, a progress message appears that already **names the model** in use.
5. If it takes too long, click **Cancel**.
6. When it succeeds, review the text and click **Confirm**.

**The Cancel button:**

- The interface stops **immediately**.
- The local model may continue processing in the background, but its result is **discarded**.
- The message **“Cancelled by the user”** appears on the right.
- **Confirm** is disabled (there is no text to apply).

**Closing the dialog:**

- Silently cancels any translation in progress.
- A late response does **not** overwrite the field.

**Tips:**

- Translate text that has already been reviewed to avoid translating errors.
- If translation fails, try again—the application may choose another model.
- For long texts, cancelling is faster than waiting for the timeout.

### 8.5 Publication summary (AI)

**How to use it:**

1. Open/edit the publication.
2. Click **AI Summary**.
3. The application tries the cloud and, if necessary, the local service.
4. The summary is placed in the **Synopsis** field.
5. **Review and edit** it before saving.

**Best practices:**

- Check that the **title** and **author** are correct—they are the basis for the summary.
- If a synopsis already exists, AI may supplement or replace it; check the dialog to confirm what will happen.
- Generated summaries may contain inaccuracies: verify the facts.

### 8.6 AI edition year

1. Open the publication.
2. Use the **AI Edition Year** action.
3. The application suggests a year.
4. Accept it or correct it manually.

> ⚠️ AI may get the year wrong. Check it against a source (ISBN, publisher, Google Books).

### 8.7 AI media notes and metadata

**Notes:**

1. Open the media record.
2. Click **AI Notes**.
3. Review the generated text.
4. Save.

**Metadata:**

1. Open the record.
2. Click **AI Metadata**.
3. AI suggests fields (genre, year, artist, etc.).
4. Accept the suggestions that make sense.

> 💡 For YouTube, enter the correct title/artist to improve the suggestions.

### 8.8 Author’s books (AI)

1. Open the author profile.
2. Use **Author’s Books** / AI generation.
3. The application tries to retrieve or generate the list of works.
4. Review it and associate/create records.

**Important:** AI-generated lists may include nonexistent works or omit real ones. Verify them before creating records in bulk.

### 8.9 Author profile (AI)

1. Open the author profile.
2. Click **Generate Profile**.
3. The application tries the cloud first and then the local service.
4. The profile is displayed for review.
5. Edit and save it.

> ⚠️ Check dates, awards, and works—these are the areas where AI is most likely to make mistakes.

### 8.10 Cloud chat (OpenRouter)

Page: **AI → Chat** (`/ai/openrouter-chat`).

**Features:**

- Real-time streaming.
- No local fallback: if the cloud fails, a friendly error is displayed.
- Conversation history.
- 👍/👎 ratings and copying.
- A collapsible **reasoning** block (when the model provides one).

**Send a message:**

1. Type in the text box.
2. Press **Enter** (or click the send button).
3. The response appears progressively.
4. You can **stop** generation, if available.

**Reasoning (“💭 Reasoning”):**

- Appears when the model provides reasoning and the option is enabled.
- It is **collapsed by default**; expand it for each message.
- Your choice (open/closed) is respected and saved with the conversation.

**History:**

- Open the **History** panel.
- Select a conversation to resume it.
- Conversations are associated with your user account.

### 8.11 Local chat (Ollama)

Page: **AI → Chat (Ollama)** (`/ai/chat`).

**How it differs from cloud chat:**

- Runs **entirely on the machine** (Ollama).
- Can work without internet access.
- Speed depends on the hardware/model.
- Also supports history, 👍/👎, copying, and reasoning.

**Reasoning models:**

- Models such as Qwen3, DeepSeek-R1, and similar models may return a “thinking” block.
- When the reasoning option is **enabled**, the block appears separately from the answer.
- When the option is **disabled**, the block is discarded and **never** appears in the answer.
- Some models send their thinking “inline” (inside the text, between markers); the application separates it correctly and does not display the markers.

**Empty response:**

- If the model produces no text, the application displays a warning message (not a blank response).
- Try rephrasing the question or selecting another model in Settings.

### 8.12 History and deleting conversations

The **History** panel is shared by both chats.

**Actions:**

| Action | Effect |
|---|---|
| **New conversation** | Starts a clean conversation |
| **Select conversation** | Resumes the context |
| **Delete history** | Deletes conversations (with confirmation) |

**Delete history:**

1. Open **History**.
2. Click **Delete history** (the danger button on the right).
3. Confirm in the dialog, which indicates **how many conversations** will be deleted and warns that the action is **irreversible**.
4. The list is cleared and the current conversation is restarted.

**Important notes:**

- Deletion is performed **one conversation at a time**.
- For an **Admin**, deleted history includes that of **all users**.
- The conversation table does not distinguish between interfaces: history from **both chats** is deleted.
- Associated 👍/👎 ratings become orphaned and are ignored (no error occurs).
- The buttons are **disabled** when the list is empty.

> ⚠️ Deleted history cannot be recovered. Export anything you need first.

### 8.13 Rate responses (👍/👎) and copy them

**Rate a response:**

1. For each assistant response, click **👍** (like) or **👎** (dislike).
2. Click the same button again to **remove** the rating.
3. The status label shows **Approved / Not approved / Not rated**.
4. The rating is restored when you reopen the conversation.

**Copy a response:**

1. Click the **copy** icon.
2. The response text is copied to the clipboard.
3. Paste it wherever you need it (Ctrl+V).

> 💡 Ratings help measure response quality and are saved per conversation/message.

### 8.14 Free model limits

- By default, the application uses **free models**.
- There is a **daily limit** on free requests (configurable by the Admin).
- When the limit is reached:
  - **Cloud** operations return “no response,” and the app **falls back to Ollama** (where applicable).
  - **Cloud chat** displays a friendly message (it does not have a local fallback).

**How to check:** see the **Internal AI usage** section on the Dashboard and the usage cards in Settings.

> 💡 If you need more capacity, the Admin can enable **paid models** or increase the limit.

### 8.15 Model safety artifacts

Some free models occasionally return only a safety classification line (for example, “User Safety: safe;”) instead of an answer.

**What the application does:**

- **Removes** those lines from valid responses.
- If the response consists **only** of the artifact, replaces it with a message asking you to reply **“continue”**.

**What you should do:** reply **“continue”** or rephrase the question.

### 8.16 Prompt Editor

Page: **AI → Prompt Editor** (`/edit-prompts`).

**What it lets you do:**

- View and edit the **prompt templates** used by AI.
- View a description of each template.
- Save changes (with validation).

**Recommendations:**

- Change one prompt at a time.
- Keep the requested focus and format (for example, “respond with the translated text only”).
- Test a simple operation before using it in bulk.
- If something goes wrong, revert to the previous version.

> ⚠️ Poorly edited prompts degrade **all** operations that use them. If you are unsure of the effect, do not change them.

### 8.17 Configure AI (Admin overview)

Configuration is in the **AI** area of Settings (see [10.3](#103-área-ia)). It includes:

- **Common settings** — enable/disable reasoning.
- **OpenRouter** — API key, default model, allow paid models, use cloud for author publications, daily limit, usage cards.
- **Ollama (local)** — key (if applicable), model address, model name.

> 💡 Without an OpenRouter key, the application works only with local Ollama.

---

## 9. Lookup Tables {#9-tabelas-de-apoio}

Lookup tables (or “auxiliary” tables) provide the options shown in selectors and charts. Without them, forms have fewer options and statistics are incomplete.

Page: **Maintenance → Lookup Tables** (`/admin/managelookuptables`). A new version is also available at `/admin/managelookuptables-new`, with the same content and a different layout.

### 9.1 Publication categories

- **Purpose:** classify publications (for example, Romance, Technical, History).
- **Fields:** name/description.
- **Impact:** used in the publication form and category chart.

### 9.2 Media categories

- **Purpose:** classify audio/video (for example, Music, Documentary, Podcast).
- **Fields:** name/description (with its own details).
- **Impact:** media form and media category chart.

### 9.3 Publication formats

- **Purpose:** physical/digital publication format (for example, Hardcover, Paperback, eBook, PDF).
- **Impact:** publication form and format chart.

### 9.4 Media formats

- **Purpose:** media file format (for example, MP3, MP4, FLAC).
- **Impact:** media form and media format chart.

### 9.5 Publishers

- **Purpose:** publisher of a publication.
- **Fields:** name and contact details (where applicable).
- **Impact:** publication form, Google Books search, and publisher chart.

### 9.6 File locations

- **Purpose:** indicate where a copy/file is stored (for example, Shelf A, PDFs folder, Cloud).
- **Impact:** publication form and path diagnostics.

### 9.7 AI models

- **Purpose:** catalog of models available for local Ollama (and a configuration reference).
- **Fields:** model name, address/base, parameters.
- **Impact:** selecting the local model in Settings and local chat.

### 9.8 Lookup table maintenance rules

| Rule | Why |
|---|---|
| Do not delete values that are **in use** | This can break associations and leave records “orphaned” |
| Prefer **editing** to deleting and recreating | Existing associations are preserved |
| Avoid duplicates | They confuse charts and searches |
| Use short, consistent names | Improves filters and readability |
| Review after bulk imports | Imported data may create duplicates |

> 💡 If a category/publisher cannot be deleted, records are using it. Reassign them first.

### 9.9 Classic version vs. new version

Both versions do the same thing:

- **Classic** (`/admin/managelookuptables`) — tabs.
- **New** (`/admin/managelookuptables-new`) — sidebar with groups (Publications, Media, AI) and cards.

Use whichever you prefer; the data is the same.

---

## 10. Administration {#10-administração}

> The following sections apply to users with the **Admin** role. A GeneralUser does not see these items in the menu.

### 10.1 Users

Page: **Maintenance → Users** (`/admin/manageusers`).

**User list:** shows users, roles, and actions.

**Create a user:**

1. Click **New**.
2. Enter a name, email, and password.
3. Choose the **role**: **Admin** or **GeneralUser**.
4. **Save**.

**Edit:**

1. Click **Edit** in the row.
2. Change the details/role.
3. **Save**.

**Delete:**

1. Click **Delete**.
2. Confirm.

> ⚠️ Deleting a user may remove/hide associated data. Confirm the impact first. Avoid deleting the last Admin.

**Best practices:**

- Use one Admin account per person, not a shared account.
- Use GeneralUser accounts for day-to-day tasks.
- Disable/remove inactive accounts.
- Document who has privileges.

### 10.2 Settings — overview

Page: **Settings** (`/admin/appsettings2`, at the bottom of the menu). The classic version is also available at `/admin/appsettings`.

The new version organizes everything in a sidebar with icons and cards on the right. Sections include:

| Section | Contents |
|---|---|
| **Downloads** | Media download folder |
| **Backups** | Upload backup folder |
| **Storage** | Folders for lists, app images, publication covers, author images |
| **Images** | Image settings |
| **Contacts** | Support email |
| **Other settings** | Notification time, organize files, remove image after editing |
| **Language** | Default language |
| **AI** | Common settings, OpenRouter, Ollama (see 10.3) |
| **WebApi** | API address |
| **Services** | Service configuration |
| **Database** | Connection/status |

> 💡 Changes to folders affect **where files are read/written**. Change them carefully and make sure the folders exist.

### 10.3 AI area {#103-área-ia}

The **AI** area has three subsections:

#### Common settings

- **Reasoning** (on/off) — shared by both chats (cloud and local). When enabled, model reasoning is shown in the “💭 Reasoning” block.

#### OpenRouter

| Field | Purpose |
|---|---|
| **API key** | Authenticates with the cloud |
| **Default model** | Model to use by default |
| **Allow paid models** | If disabled, only free models are used |
| **Use cloud for author publications** | Enables cloud-based generation of author publications |
| **Free daily limit** | Maximum number of free requests per day |
| **Usage cards** | Current and historical usage |

**Notes:**

- Without a key, the cloud does not work and everything falls back to Ollama.
- With **paid models disabled**, the application does **not** use paid models: it chooses free equivalents or the free router.
- The limit is **enforced on the server**; when it is reached, cloud operations return “no response” and the app uses the local service.

#### Ollama (local)

| Field | Purpose |
|---|---|
| **API key** | If your Ollama requires one |
| **Base address** | Where Ollama is running |
| **Model name** | Local model to use |

**Notes:**

- Ollama must be installed and running on the machine (or accessible at the specified address).
- Larger models produce better results but require more memory and run more slowly.

### 10.4 Logs

Page: **Logs** (`/admin/viewapplogs`).

**What it shows:** application records (date, level, message), including errors.

**How to use it:**

1. Open **Logs**.
2. Sort by date (newest first).
3. Filter by level (error, warning, information).
4. Use the message to diagnose the issue.

> 💡 The Dashboard shows only recent errors; Logs contains the history.

### 10.5 File Manager

Page: **File Manager** (`/admin/filemanager`).

**What it lets you do:**

- Browse upload/download folders.
- View files and sizes.
- **Upload** files.
- **Download** files.
- **Create folders**.
- **Delete** files/folders (with confirmation).

**Cautions:**

- Deleting files referenced by records leaves those records without content.
- Do not move files manually without updating their paths in the application.
- Prefer this tool to working directly with the file system.

### 10.6 Backups

#### Database

Page: **Backup → Database** (`/backupdatabase`).

1. Open the page.
2. Click the **backup** button.
3. Confirm the operation.
4. Wait for the completion message.

**What it copies:** the database (records, users, settings, conversations, ratings).

**What it does not copy:** files (PDFs, covers, media).

#### Uploads

Three dedicated pages:

| Page | Contents | File created |
|---|---|---|
| `/backupuploads/1` | **Publication** uploads (PDFs and covers) | `PdfsAndCovers` (ZIP) |
| `/backupuploads/2` | **Media** uploads | `MediaFiles` (ZIP) |
| `/backupuploads/3` | **Image** uploads | `AllAppImages` (ZIP) |

**How to use them:**

1. Open the page for the type you want.
2. Start the backup.
3. Wait for it to finish.
4. Save the resulting file somewhere safe.

> ⚠️ A complete backup = **database + uploads**. Backing up only one of these leaves the system inconsistent if you need to restore it.

### 10.7 Application table configuration

The application includes auxiliary configuration pages (`/admin/configapptables`, `/admin/adminpanel`) for advanced scenarios. **They are not needed for normal use.** Change settings only if you understand the effect.

### 10.8 Administration best practices

- Make **regular** backups (database + uploads).
- Test restoration on a copy before you need it.
- Keep the OpenRouter key private.
- Review logs weekly.
- Carefully clean up unused lookup tables.
- Document configuration changes.
- Keep Ollama and its models up to date.

---

## 11. Reports {#11-relatórios}

The application generates reports from templates (RDLC) and displays them in the interface.

### 11.1 Author report

Page: **`/AuthorsPreview_Report`**.

**How to use it:**

1. Open the report page.
2. Choose/enter the parameters (for example, author or date range).
3. Click **Preview**.
4. The report is generated and displayed.
5. Use the available **export** options (PDF/Excel, depending on the template).

**What to expect:**

- The report uses the data you can access (data isolation is enforced).
- Large reports may take time.
- If there is no data, the report may be empty.

### 11.2 Troubleshooting reports

| Symptom | Likely cause | Solution |
|---|---|---|
| Empty report | No data for the parameters | Widen the date range/clear filters |
| Error during generation | Missing template or invalid data | Check Logs; contact the Admin |
| Export fails | Permissions or unsupported format | Try another format; check the folder |
| Takes too long | Too many records | Narrow the parameters |

> 💡 Report templates are maintained by the Admin. If a report is consistently wrong, that is where it should be fixed.

---

## 12. Tools {#12-ferramentas}

### 12.1 Translator

The translator appears as a **dialog** in various parts of the application and as a standalone tool.

**Features:**

- Choose the source/target languages.
- Progress indicator with the **model name**.
- **Cancel** (stops the UI immediately).
- **Confirm** (applies the text).
- **Chip** showing which model responded.

**Best practices:**

- Translate complete sentences, not fragments.
- Review proper names and technical terms.
- If you cancel, the original text remains unchanged.

### 12.2 AI benchmark

The application collects model performance metrics (latency, tokens/second, usage) and displays them on the Dashboard (**Activity** section) and in Settings (usage cards).

**What it is for:**

- Comparing models.
- Detecting performance degradation.
- Helping choose the default model.

> 💡 The benchmark is an **observational** tool; it does not change the app's behavior.

### 12.3 Chat as a work tool

In addition to conversation, chats can help with everyday tasks:

- Drafting synopses.
- Suggesting categories.
- Translating excerpts.
- Summarizing pasted text.
- Correcting spelling.

> 💡 For structured tasks, use the dedicated features (Summary, Translate, Metadata)—their results are applied directly to the fields.

### 12.4 Recipes and supplementary content

There are auxiliary recipe pages (`/recipes`, `/recipe_cards`) used as sample/demo content. They are not part of the main cataloging workflow.

---

## 13. Tips and Shortcuts {#13-dicas-e-atalhos}

### 13.1 Productivity

- Press **Enter** in chat to send a message.
- **Ctrl+V** pastes text/images into fields that support them.
- Use the **selector search** (author, publisher, category) instead of scrolling through long lists.
- Sort/filter the grid before exporting or reporting.
- Keep **one conversation per topic** in chat to make history more useful.

### 13.2 Data quality

- Always fill in **Title**, **Author**, and **Category**.
- Use the **ISBN** to distinguish publications.
- Avoid duplicates: search before creating a record.
- Keep names consistent (for example, “J. R. R. Tolkien,” not three different variations).
- Review the Dashboard **Health** indicators.

### 13.3 Files and folders

- Avoid problematic characters in file names (`/ \ : * ? " < > |`).
- Use one folder per type (PDFs, covers, audio, video).
- Back up before large imports.
- Do not move files outside the application without updating their paths.

### 13.4 AI

- **Always** review what AI generates.
- Prefer dedicated features for structured results.
- If the cloud is slow/unavailable, the app automatically uses the local service.
- With free models, the “User Safety” artifact may appear—reply “continue.”
- If the response is empty, rephrase or change models.

### 13.5 Security

- Do not share the OpenRouter key.
- Sign out on shared computers.
- Use strong, unique passwords.
- Do not use the Admin account for everyday tasks.
- Be wary of prompts/files that ask for sensitive data.

### 13.6 Theme and accessibility

- Light theme is better for bright environments; dark theme is better for low-light environments.
- The dark sidebar/top bar are intentional and do not change with the theme.
- Zoom in the browser (Ctrl + / Ctrl -) if the text is too small.
- Main contrast levels comply with accessibility standards (WCAG AA).

### 13.7 Quick actions table

| I want to… | Action |
|---|---|
| Open a publication quickly | Search in the publications grid |
| Open this manual | Help button (`?`) in the top bar |
| Change language | Settings → Language (the top bar only shows the current language) |
| Change theme | Sun/moon icon |
| Cancel a translation | **Cancel** button in the dialog |
| Copy an AI response | Copy icon on the message |
| Rate a response | 👍 / 👎 on the message |
| Delete history | History → Delete history |
| View recent errors | Dashboard → Audit / Logs |
| Make a backup | Backup → Database + Uploads |

---

## 14. Troubleshooting {#14-resolução-de-problemas}

This section covers the most common problems. Symptoms are described as users see them.

### 14.1 Authentication and session

| Symptom | Likely cause | Solution |
|---|---|---|
| “Invalid credentials” | Incorrect email/password | Check capitalization and keyboard layout; reset your password |
| Session expires quickly | Security policy | Sign in again; contact the Admin |
| Redirected to login on every page | Invalid cookie/session | Clear the site's cookies; sign in again |
| Password reset email does not arrive | SMTP not configured/spam | Check spam; contact the Admin |

### 14.2 Data does not appear

| Symptom | Likely cause | Solution |
|---|---|---|
| Publications/authors have “disappeared” | Wrong account (user isolation) | Check the user; ask an Admin to verify |
| Grid is empty, but records exist | Active filter/search | Clear filters and search |
| Charts are empty | Classification fields are not filled in | Fill in category/format/publisher |
| Another user's records are invisible | Expected behavior for GeneralUser | Request Admin access if needed |

### 14.3 AI does not respond

| Symptom | Likely cause | Solution |
|---|---|---|
| Translation/summary returns nothing | Cloud unavailable and Ollama stopped | Check Ollama; try again |
| “Daily limit reached” | Free model cap | Wait until the next day or ask the Admin to increase the limit/enable paid models |
| Cloud chat error | No key/limit/instability | Check the key in Settings; try again later; use local chat |
| Empty response | Model produced no content | Rephrase; choose another model |
| “User Safety: safe;” | Model artifact | Reply “continue” |
| Translation is very slow | Large local model | Cancel; use the cloud or a smaller model |
| Reasoning does not appear | Option is disabled | Enable “Reasoning” in Common settings |
| Unexpected model | Automatic sweep/fallback | Check the model chip; set the default model in Settings |

### 14.4 Files

| Symptom | Likely cause | Solution |
|---|---|---|
| “File not found” | File moved/deleted | Open `/pubsnotfoundondisk`; correct the path or upload again |
| Incorrect path | Folder moved | Open `/pubswithwrongpath`; update the location |
| Cover does not appear | Missing image/invalid format | Upload the image again |
| Upload fails | Size/format/permissions | Reduce the file size; check the format; review Logs |
| Player does not play media | Unsupported codec/invalid file | Convert to a supported format |

### 14.5 Reports

| Symptom | Likely cause | Solution |
|---|---|---|
| Empty report | No data/filters | Widen the criteria |
| Generation error | Template/data | Check Logs; contact the Admin |
| Export fails | Permissions/folder | Try another format/location |

### 14.6 Local environment (technical, for Admins)

| Symptom | Likely cause | Solution |
|---|---|---|
| Page does not load / connection refused | API or interface is stopped | Start both processes |
| Port 5001 conflict | API and interface use the same port by default | Use different profiles/ports (for example, API 4100, interface 4110) |
| Browser certificate error | Self-signed certificate | Accept the certificate; do not enable HSTS until a trusted certificate is available |
| `dotnet build` fails with MSB3027/MSB3021 | Running application locks the DLL | Stop the application and build again |
| Restore fails | NuGet/Syncfusion source | Check the source configuration and key |
| SQL login fails with `Trusted_Connection` | App pool name differs from the login | Align the app pool name with the SQL login |

> 💡 Deployment, ports, and SQL details are documented in the project's technical notes (`AGENTS.md`), not in this user manual.

### 14.7 When to ask for help

Collect and send:

1. **What you were doing** (page and action).
2. **What you expected** and **what happened**.
3. **The error message** (text or screenshot).
4. Approximate **date/time**.
5. **User account** (do not send passwords).
6. If possible, the corresponding **Logs**.

> ⚠️ Never send passwords or the OpenRouter key in screenshots.

---

## 15. Glossary {#15-glossário}

| Term | Meaning |
|---|---|
| **Admin** | User with full access, including administration |
| **GeneralUser** | Regular user; only sees their own data |
| **Publication** | A publication/document record |
| **Media** | An audio, video, or YouTube record |
| **Playlist** | An ordered collection of media items |
| **Lookup/auxiliary table** | A list of values used by selectors (categories, formats, publishers, etc.) |
| **Lookup** | Another term for a lookup table |
| **OpenRouter** | Cloud service providing access to multiple AI models |
| **Ollama** | Local service that runs AI models on the machine |
| **Fallback** | Automatic switch to the alternative provider when the primary one fails |
| **Cloud** | Remote provider (OpenRouter) |
| **Local** | On-machine provider (Ollama) |
| **Model** | AI engine (for example, llama-3.3-70b) |
| **Model chip** | Label showing which model responded |
| **Prompt** | Instruction/text that guides the AI |
| **Prompt template** | Reusable prompt, editable in the Prompt Editor |
| **Streaming** | A response that appears progressively |
| **Reasoning** | Some models' internal “thinking,” shown separately from the response |
| **History** | List of saved conversations |
| **Rating** | Personal rating (stars) |
| **Synopsis** | Summary/description of a publication |
| **ISBN** | International publication identifier |
| **RDLC** | Report template format |
| **Backup** | Safety copy |
| **Log** | Record of events/errors |
| **Data isolation** | Rule that prevents one user from seeing another user's data |
| **Theme** | Light or dark appearance |
| **Culture** | Interface language/region (pt, en, fr, es) |
| **Upload** | Uploading files to the application |
| **Download** | Transferring files from the application |
| **Cap** | Maximum limit (for example, free requests per day) |
| **Token** | Text unit used by AI models |
| **Latency** | Response time |
| **TPS / Tokens per second** | Model generation speed |

---

## 16. Complete scenarios (step by step) {#16-cenários-completos-passo-a-passo}

The following scenarios bring together several features and provide “recipes” for real tasks.

### 16.1 Catalog a publication using its ISBN

**Goal:** create a complete record with official data and a cover in just a few minutes.

1. Open **Publications → Management** (`/publicacoes`).
2. Click **New**.
3. Enter the number in the **ISBN** field (with or without hyphens).
4. Open **Search Online** and search by ISBN or title.
5. Choose the correct result from the list.
6. Check the populated fields: **Title**, **Author**, **Publisher**, **Year**, **Pages**, **Synopsis**.
7. If the **Author** is missing, create the author in **Authors** first and then return (form data will be lost if you leave without saving—copy what you have entered).
8. If **Category**, **Format**, or **File location** is missing, choose an existing option (or ask the Admin to create one).
9. If the synopsis is in another language, use **Translate** and confirm.
10. Upload the **cover** (if the search did not provide one).
11. Click **Save**.
12. Return to the Dashboard and confirm that the publication appears in the counts and is no longer listed as “without cover/pages” in the **Health** section.

**Common errors:**

| Error | Cause | Correction |
|---|---|---|
| “Author required” | No author selected | Select/create an author |
| “Category required” | No category selected | Select a category |
| “Invalid Edition Year” | Year has fewer/more than 4 digits or is in the future | Correct it to 4 digits ≤ current year |
| Duplicate record | ISBN already exists | Search before creating; consolidate records |

### 16.2 Import a folder of PDFs in bulk

**Goal:** create records from multiple files and then correct anything missing.

1. Before importing, back up the database (`/backupdatabase`).
2. Prepare the files in a folder with consistent names (`ISBN - Title.pdf`).
3. In **Publications → Management**, use **Import PDF/DOCX** and select the files.
4. Wait for processing to finish (it may take a while).
5. Open the grid and sort by **date** to see the new records.
6. For each record:
   - Check the **Title** and **Author**.
   - Complete the **Category**, **Publisher**, **Format**, and **File location**.
   - Add a **cover** and **synopsis** (use AI if necessary).
7. When finished, open `/pubsnotfoundondisk` and `/pubswithwrongpath` and correct the paths.
8. Review the **Health** section: duplicates, missing covers, missing page counts.
9. Make a new backup.

**Tips:**

- Import small batches (10–20) so you do not lose work if something goes wrong.
- If a file is not recognized, open it first and make sure it is not protected/corrupted.
- Do not delete the original files until you have validated the records.

### 16.3 Prepare a complete author profile

**Goal:** create an author profile with a photo, reviewed biography, and AI-generated profile.

1. Open **Publications → Authors** (`/autores`).
2. Click **New** → enter the **Name** → **Save**.
3. Open the author's profile.
4. Upload the **photo** (portrait, good resolution).
5. Write a short **base biography** (2–4 sentences) with facts you know.
6. Use **Generate AI Profile** to complete it.
7. **Review** the result: names, dates, works, and awards.
8. Correct anything that is wrong and **Save**.
9. Open **Author’s Publications** and associate/create the works.
10. If the biography is in another language, use **Translate** and confirm.

**Important:** AI may invent dates and works. Validate the profile before sharing it.

### 16.4 Build a music library and playlist

**Goal:** upload albums, fill in metadata, and listen using a playlist.

1. Open **Media → Management** (`/media`).
2. Click **Upload** and select the audio files.
3. For each created record, click **Edit** and check:
   - **Title**, **Author/Artist**, **Media format**, **Genre/Category**, **Year**.
4. Use **AI Metadata** / **AI Notes** to complete anything missing (always review the results).
5. Open **Media → Playlists** (`/media/playlists`).
6. Click **New** → enter a name (for example, “Jazz — evening”) → **Save**.
7. Open the playlist → **Add items** → select the tracks.
8. Reorder them if necessary.
9. Click **Play** to open the **Playlist Player**.
10. Confirm that all tracks play; correct any that fail.

**Tips:**

- One playlist per context is more useful than one huge list.
- If a track does not play, check the file in the **File Manager**.
- Check the correct **Media format** for the Dashboard charts.

### 16.5 Add and play a YouTube video

1. Open **Media → Management**.
2. Click **New** → choose the **YouTube** type.
3. Fill in **Title**, **Author/Artist**, **Genre/Category**, and **Media format**.
4. Paste the video **link** (full URL).
5. **Save**.
6. In the grid, click **▶** to play it.
7. If the player displays an error, check that:
   - The link is correct and the video is not private.
   - The video has not been removed.
8. To organize it, add the item to a **playlist** (depending on configuration, playlists can include audio, video, and YouTube items).

### 16.6 Translate and summarize without losing the original

**Goal:** use AI while keeping the original text intact.

1. Open the publication/author and locate the text (synopsis/biography).
2. **Copy the original text** to a text editor or chat as a reference.
3. Open **Translate**, choose the languages, and click **Translate**.
4. If it takes too long, click **Cancel**—the field remains unchanged.
5. When it succeeds, compare the result with the original.
6. Click **Confirm** only when you are satisfied.
7. For a summary, use **AI Summary** and review it before saving.
8. Save the record.

> ⚠️ **Confirm** applies the text to the field. If you want to keep both versions, save the original somewhere else first.

### 16.7 Backup and verification routine

**Goal:** make sure you can recover the system.

1. Open `/backupdatabase` and create a database backup.
2. Open `/backupuploads/1`, `/backupuploads/2`, and `/backupuploads/3` and create upload backups.
3. Copy the resulting files **off the machine** (external drive/cloud).
4. Record the date and what was included.
5. Periodically, **test restoration** on a copy—a backup that has not been tested is not a backup.
6. After any changes, confirm that the folder paths in **Settings** are still correct.

### 16.8 Recover after changing folders

**Goal:** get everything working again after moving files.

1. Update the folders in **Settings** (Storage, Images, Downloads, Backups).
2. Open `/pubsnotfoundondisk` and check which files are missing.
3. Open `/pubswithwrongpath` and correct the paths.
4. In the **File Manager**, confirm that the uploads are visible.
5. Open a few publications and check that **Read PDF** works.
6. Open the galleries and confirm that images appear.
7. Make a new backup.

---

## Appendix A — Main routes {#apêndice-a--rotas-principais}

| Route | Page |
|---|---|
| `/` | Dashboard |
| `/publicacoes` | Publication management |
| `/imagegallery` | Image gallery |
| `/autores` | Authors |
| `/media` | Media management |
| `/videogallery` | Video gallery |
| `/media/playlists` | Playlists |
| `/media/playlists/{id}/player` | Playlist player |
| `/mp3player` | MP3 player |
| `/youtubeplayer` | YouTube player |
| `/musicgallery` | Music gallery |
| `/edit-prompts` | Prompt Editor |
| `/ai/openrouter-chat` | Cloud chat |
| `/ai/chat` | Local chat (Ollama) |
| `/pubsnotfoundondisk` | Publications with missing files |
| `/pubswithwrongpath` | Publications with incorrect paths |
| `/AuthorsPreview_Report` | Author report |
| `/admin/manageusers` | Users |
| `/admin/managelookuptables` | Lookup tables (classic) |
| `/admin/managelookuptables-new` | Lookup tables (new) |
| `/admin/appsettings` | Settings (classic) |
| `/admin/appsettings2` | Settings (new) |
| `/admin/filemanager` | File Manager |
| `/admin/viewapplogs` | Logs |
| `/backupdatabase` | Database backup |
| `/backupuploads/1` | Publication uploads backup |
| `/backupuploads/2` | Media uploads backup |
| `/backupuploads/3` | Image uploads backup |

## Appendix B — Periodic maintenance checklist {#apêndice-b--checklist-de-manutenção-periódica}

**Daily**

- [ ] Check recent errors (Dashboard/Logs).
- [ ] Confirm that AI responds (cloud or local).

**Weekly**

- [ ] Review **Health** indicators.
- [ ] Check recent backups.
- [ ] Clean up old conversations if necessary.

**Monthly**

- [ ] Make a full backup (database + uploads) and store it off the machine.
- [ ] Review users and roles.
- [ ] Review lookup tables (duplicates/orphans).
- [ ] Update local models and check disk space.

**After imports/migrations**

- [ ] Run `/pubsnotfoundondisk` and `/pubswithwrongpath`.
- [ ] Check covers and files.
- [ ] Review duplicate titles/authors.

---

## Appendix C — Frequently asked questions (FAQ) {#apêndice-c--perguntas-frequentes-faq}

**Q: Why can’t another user see a publication I created?**  
A: Data isolation is enforced per user. Only an **Admin** can see everyone’s data. A GeneralUser only sees their own.

**Q: I changed the language, but my publications are still in Portuguese. Is that an error?**  
A: No. The language selector translates the **interface**. The **content** you entered (titles, synopses, biographies) is not translated automatically. Use **Translate** for those fields.

**Q: AI is taking a long time. What should I do?**  
A: You can **Cancel** the operation (the interface stops immediately). If this happens frequently, you are probably using the **local** model (Ollama); a smaller model or the cloud will be faster.

**Q: The AI response was empty.**  
A: The application displays a warning instead of a blank message bubble. Rephrase the question, try again, or choose another model in Settings.

**Q: “User Safety: safe;” appeared in a response.**  
A: This is an artifact from some free models. Reply **“continue”** or rephrase the question. The application already removes these lines from valid responses.

**Q: I reached the daily AI limit. What now?**  
A: Cloud operations will start to fail and the application will use **local Ollama** (when available). Cloud chat displays a friendly message. Wait until the next day or ask the Admin to review the limit/paid models.

**Q: The chip says “Automatic.” Which model responded?**  
A: It means the cloud router chose the model. The exact name may not be displayed in this case; the chip indicates the provider (OpenRouter) and that the choice was automatic.

**Q: Can I recover a deleted conversation?**  
A: No. Deleting history is **irreversible**. Associated ratings become orphaned and are ignored.

**Q: Why is the sidebar still dark in the light theme?**  
A: This is intentional. The sidebar and top bar have their own identity and do not change with the theme.

**Q: I deleted a category and the publications look strange.**  
A: The category was probably in use. Prefer **editing** rather than deleting. If necessary, recreate the category and reassign the publications.

**Q: The publication’s PDF file will not open.**  
A: Check the path at `/pubsnotfoundondisk` and `/pubswithwrongpath`; check the **File Manager**; upload the file again if necessary.

**Q: The report is empty.**  
A: Check the parameters/filters and whether data exists for the period. Reports respect user data isolation.

**Q: Can I use the application without internet access?**  
A: Partially. The application and local data work; **cloud** AI does not. **Local Ollama** allows you to use AI offline, provided it is running.

**Q: Where is the OpenRouter key?**  
A: In **Settings → AI → OpenRouter** (Admin only). Never share it or include it in screenshots.

**Q: How do I know which AI provider is being used?**  
A: Check the **model chip** (OpenRouter/Ollama/Automatic) and the Dashboard’s **Internal AI usage** section, which also shows fallbacks.

**Q: What is a “fallback”?**  
A: It is the automatic switch from cloud to local service when the cloud fails. Many fallbacks indicate instability, a missing key, or an exhausted limit.

---

*End of manual. For technical details about installation, deployment, and server configuration, see the project's technical documentation.*
