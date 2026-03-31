# Implementation Plan — MediaOrganizerApp

Summary
- Goal: Incrementally improve metadata accuracy, media UX, and maintainability focused on the Blazor frontend (`Web.Blazor`) and backend Web API.
- Scope: feature tasks, priorities, effort estimates, acceptance criteria, and implementation notes tailored to the repo (examples reference `EditMedia.razor`, `FileUploadImageComponent.razor`, `AiMediaClientService.cs`, `AiAuthorProfileService.cs`, `GetYoutubeVideoMetadata.cs`, `ServiceRegistration.cs`).

Important constraints (repo rules)
- AI fallback for metadata must run only when external APIs fail.
- When registering services that depend on `HttpClient` use `AddHttpClient` inside `ServiceRegistration.AddServicesInfrastructure` (do not register in `Program.cs`).
- Run a full build after code changes.
- SignalR work should remain in services (no UI hub changes).
- Admin-editable prompts exist (`/edit-prompts`) — use them for prompt tuning.

Priority legend
- Critical — blocks core flows or data integrity
- High — strong user value / UX improvements
- Medium — useful features / reliability
- Low — nice-to-have / incrementals

Tasks (issues)

1) Feature: Image crop & preview for cover uploads
- File references: `EditMedia.razor`, `FileUploadImageComponent.razor`
- Priority: High
- Estimate: 6 - 10 hours
- Description: Add client-side cropper step after image choose and before upload. Show live preview and allow rotate/fit. Persist cropped image to API endpoint (or existing uploader).
- Acceptance criteria:
  - Users can crop/preview before hitting upload.
  - Cropped image is stored in same `CoverFile` field as current flow and displays in `EditMedia` preview.
  - No UI regressions in other `SfUploader` usages.
- Implementation notes:
  - Use a small JS cropper + Blazor JS interop or a Syncfusion cropper if available.
  - Keep existing `SfUploader` component; pass final bytes to backend.
  - Add unit/integration test for bytes->cover save pipeline.

2) Feature: Robust duplicate detection on import/upload
- Priority: Critical
- Estimate: 12 - 20 hours
- Description: Compute SHA256 hash at upload/import; store in DB; when importing check for exact duplicate and run fuzzy metadata match to detect probable duplicates.
- Acceptance criteria:
  - Exact duplicate uploads are blocked/flagged with UI message.
  - Probable duplicates are presented to user with confidence score and merge/skip options.
- Implementation notes:
  - Add `FileHash` column to media table (migration).
  - Add endpoint in `MediaController` for batched duplicate check.
  - Use existing UI import flows to show duplicate results.

3) Feature: AI-assisted metadata orchestration (external-first)
- File references: `AiMediaClientService.cs`, `AiAuthorProfileService.cs`, `GetYoutubeVideoMetadata.cs`
- Priority: High
- Estimate: 20 - 30 hours
- Description: Implement orchestrator that:
  1. Queries external APIs (Google Books, YouTube, OpenLibrary).
  2. If external results present and confidence >= threshold → use them.
  3. Only if external results fail or confidence low → call AI fallback (`Ai*` services).
  4. Persist `Confidence` and `Notes` for traceability.
- Acceptance criteria:
  - Author/book/video lookups try external APIs first.
  - AI fallback runs only when external lookups fail or low confidence.
  - Stored DTOs include `Confidence` number and `Notes` text.
  - When multiple plausible author candidates exist, return only single candidate (highest confidence). If tie difference < 0.15, set notes to indicate ambiguity and lower confidence.
- Implementation notes:
  - Reuse existing prompt templates in `/Prompts`.
  - Ensure AI `Notes` content for media relates to LP/CD/DVD only (per repo rule).
  - Add tests mocking both external API success and fallback.

4) Feature: Bulk edit & tagging UI for media/publications
- File references: media list pages and `MediaBase.razor.cs`
- Priority: Medium
- Estimate: 8 - 14 hours
- Description: Allow multi-select and apply category/tags/ratings in batch. Backend batch endpoint to apply updates transactionally.
- Acceptance criteria:
  - Multi-select available on grid/list.
  - Batch update updates all selected items and returns per-item status.

5) Feature: OCR for scanned publications (searchable text)
- Priority: Medium
- Estimate: 24 - 40 hours
- Description: Add OCR pipeline for PDFs/images (Tesseract or cloud OCR); store extracted text for full-text search & metadata extraction.
- Acceptance criteria:
  - OCR runs in background worker; extracted text is searchable on publications page.
  - Errors are logged and retriable.

6) Feature: Full-text search with faceted filters
- Priority: Medium
- Estimate: 16 - 30 hours
- Description: Add a search index for metadata + OCR text (Azure Cognitive Search or Elastic/SQLite FTS). Add UI filters.
- Acceptance criteria:
  - Search returns relevant results across title, author, notes, and OCR text.
  - Facets: category, format, year, rating.

7) Feature: YouTube playlist + captions import
- File references: `GetYoutubeVideoMetadata.cs`
- Priority: Low
- Estimate: 10 - 18 hours
- Description: Add playlist import, fetch captions, and store as `Notes` or captions resource for media items.
- Acceptance criteria:
  - Playlist import produces multiple media entries.
  - Captions stored and viewable in media detail.

8) Infrastructure: Background worker & job dashboard
- Priority: High
- Estimate: 12 - 20 hours
- Description: Move long-running jobs (OCR, embedding generation, bulk import) to an `IHostedService` job queue. Expose job statuses to UI via SignalR.
- Acceptance criteria:
  - Jobs run reliably in background, progress shown in UI, and jobs are cancelable.
- Implementation note: Keep SignalR usage in services, not UI logic (per repo guidelines).

9) Developer: Tests + CI pipeline
- Priority: High
- Estimate: 8 - 12 hours
- Description: Add unit tests for AI orchestration, YouTube parsing, and duplicate detection. Add GitHub Actions to build and run tests on PRs.
- Acceptance criteria:
  - CI builds on push/PR; unit tests executed and passing.

10) UX: Shareable public read-only galleries
- Priority: Low
- Estimate: 10 - 16 hours
- Description: Tokenized share links that expose read-only views of selected collections.
- Acceptance criteria:
  - Token URL displays collection without login.
  - Token can be revoked.

Implementation & code snippets
- Register new `HttpClient` service (add to `ServiceRegistration.cs` under `AddServicesInfrastructure`):
```
// Example: adding a new embeddings client
services.AddHttpClient<IEmbeddingsService, EmbeddingsService>();
```

- After DB schema changes run:
  - `dotnet ef migrations add <Name>` and `dotnet ef database update`
  - Run build: `dotnet build`

Testing & QA
- Always run a full build after code changes: `dotnet build`.
- Run unit tests: `dotnet test`.
- For heavy tasks (OCR/embeddings) run with a local dev queue and small sample dataset before full import.

Security & secrets
- Store API keys either in environment variables or use a secret store.
- Add UI rotation + validation in `admin/appsettings` (existing page referenced in `SidebarData.cs`).

Notes & heuristics for AI metadata (must follow)
- AI fallback only after external APIs fail.
- Return a single highest-confidence author candidate. If multiple candidates have confidence within 0.15 of each other, mark `Notes` with ambiguity and lower confidence.
- `Confidence` scale: 0.0 to 1.0 (low < 0.5).
- `Notes` generated by AI that describe media must refer only to LPs, CDs, or DVDs (never individual songs).

Suggested next steps
1. Approve top-3 tasks to start (Image crop flow, duplicate detection, and AI orchestration).
2. Create issues in repo from the above task list and assign owners.
3. Implement CI + tests before large changes.
