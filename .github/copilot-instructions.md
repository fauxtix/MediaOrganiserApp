# Copilot Instructions

## Project Guidelines
- User expects direct statements about tool limitations (e.g., when I cannot fully analyze the app from only SidebarData.cs) and wants to avoid repetitive/looping responses.
- User confirmed the proposed approach for adding an SfUploader-based cover image picker to EditMedia works.
- User’s main pain point with external metadata APIs is low hit-rate (often can’t find authors), while LLM prompts usually return some response. User wants AI prompts (LLM) to replace some external metadata API calls, e.g., fetching author profiles via AI instead of Google Books/other APIs. 
- AI fallback for metadata (e.g., author profiles) should run only when external API lookup fails.
- When multiple plausible author candidates are found, choose the single candidate with the highest internal confidence and return only that candidate's profile; if only one candidate is found, return it normally. Explain ambiguity in Notes and lower Confidence. Use a 0.0–1.0 scale; thresholds: low confidence < 0.5, tie difference < 0.15 indicates ambiguity.
- Always run a full build after any code change to ensure correctness and functionality.

## User Environment Preferences
- User is using Visual Studio Community 2026 (18.4.2) with a workspace root at `C:\NewProjects\MediaOrganizer\MediaOrganizerApp`.
- User prefers the terminal shell to be `powershell.exe`.
- The repository is located at the specified path on the `master` branch.
- Projects target .NET 10 and .NET 7.
- The workspace contains a Blazor project; prioritize Blazor-focused answers when making suggestions or code changes.
- User runs the API first then the Blazor app as multiple startup projects and expects the assistant to verify both projects are running before diagnosing; assistant must confirm this before making configuration or port changes.
- Ollama will always run on port 11434 in the user's environment.
- If PowerShell errors about missing '-InFile', ask where it occurs and search the repository for '-InFile' usage to locate the failing command.
- README uses 'Media Organizer App' as the application name and does not include a 'Publications Management' heading; do not modify README without explicit request.
- User passes the current culture to AI services so the UI responds in that language; app language is English but user secrets show Language = pt-PT. Assistant should respect and propagate culture settings to AI requests and ask before modifying code. AI responses should also be in the app language (English).

## Service Registration Guidelines
- When registering new services that depend on HttpClient, always use `AddHttpClient` in the `AddServicesInfrastructure` method (ServiceRegistration.cs) and never register directly in `Program.cs`. Follow the pattern of other services in the project to avoid duplication and lifecycle issues.
- Register the existing `OllamaChatService` as the `IAiChatService` in the Web API project, and avoid creating placeholders without asking the user first. The assistant should ask before modifying files.
- Register persistence services in `MediaOrganizerApp.Persistence.ServiceRegistration.AddPersistenceInfrastructure`.
- Entities should be placed in `Core/Domain/Entities`, interfaces in `Core/Application/Interfaces`, and services in `Persistence/Services/Chat`.
- Blazor API clients should be registered via `AddHttpClient` in `Web.Blazor.ServiceRegistration.AddServicesInfrastructure`.

## API Client Guidelines
- Prefer using a typed API client wrapper for chat; UI components should not call API endpoints directly. Use `IConfiguration["BaseUrl"]` and include the ApiKey header from configuration in the client wrapper.

## SignalR Guidelines
- Avoid modifying the UI for SignalR or Hubs-related operations. All SignalR-related work should be handled in the service layer, not the UI.

## UI Implementation Guidelines
- Apply requested UI changes directly without presenting multiple options. For example, ensure cards are the same height, images fill the container, and titles are clamped to two lines.
- When rendering author/book UI, prioritize styling of book cover images (thumbnails). Treat the author's portrait as unimportant for book-card layout; it's okay if the author's portrait is cropped. Focus component changes on book image styling (thumbnail sizing, object-fit, card layout).
- Provide an in-app admin UI to toggle feature flags instead of requiring administrators to run PowerShell scripts for changes.
- When comparing UI components, explicitly compare how Syncfusion DropDownList is used across the app (Fields, ValueChange, @bind-Value, DropDownListFieldSettings) before recommending changes; avoid asserting behaviors without evidence and ask for diagnostic outputs first.

## Code Snippets
- Include runnable code snippets in the guidance file to enhance understanding and usability.

## AI Metadata Rules
- AI responses for "Notes" must always pertain to LPs, CDs, or DVDs and should never describe songs. Ensure that AI-generated content aligns with these formats. LPs, CDs, and DVDs are managed by the Media Management feature; they are not part of Publications or 'Publications Management'.
- Ensure that the `ChatRequest` DTO includes a Language property to propagate language settings for chat streaming, as the `AiAuthorProfileService` passes language into the prompt (it uses `["language"] = language`). Note that the Chat API currently does not include a language parameter. The assistant should recommend adding Language to ChatRequest and inserting a system message in AiController.Chat or have the client prepend a system message.

## Prompt and Modification Guidelines
- Do not modify the solution or files unless the user explicitly requests changes; prefer making suggestions and asking before applying edits. User prefers I do not change files unless they explicitly ask; they want diagnostics and explanations rather than automatic edits.
- Do not invent tasks or actions modifying the repo. Only update prompts and provide suggestions.
- When correcting the assistant's inference about JS interop, avoid asserting JS interop exists in a file unless explicitly present; ask for exception stack trace or offer diagnostics instead of making unsupported assertions.
- When the user corrects the assistant's inference about component internals, analyze provided files before asserting implementation details and avoid changing files unless the user explicitly requests modifications. User prefers that the assistant analyze provided files before asserting implementation details and not change files unless explicitly requested.
- Do not continue proposing fixes for the spinner label issue unless the user explicitly asks to resume; further attempts are considered a waste of time.
- When the assistant is uncertain or lacks evidence, do not hallucinate fixes or assert implementation details; ask for more information (stack trace, file contents) before proposing code changes. Avoid unsolicited file edits and repeated speculative solutions.
- User expects concrete, actionable steps (runnable PowerShell scripts and exact file locations) and precise explanations rather than vague/high-level answers.
- Do not modify server/port configuration or other infrastructure settings without user confirmation; always confirm before making such edits.
- Avoid including remarks that might confuse the user; do not add confusing page comments or unsolicited configuration changes. Prefer concise, direct instructions and always ask before modifying files or changing infrastructure settings.
- When the user says 'don't bother' or corrects the assistant to stop, stop further unsolicited actions and wait for explicit instructions before modifying files and ask before applying any code edits or diagnostic UI changes.

## User Secrets Management
- User keeps the OpenAI API key in the Blazor project's user-secrets; the Web API process needs the key in the Web API project's user-secrets. Assistant should verify which project has the OpenAI secret before suggesting commands and provide explicit PowerShell commands to copy/set the key in the Web API secrets, including the provided API key value directly in the commands, and ask before applying changes. User prefers explicit PowerShell commands that include the provided API key directly when setting the Web API project's user-secrets and prefers the assistant not reference the Blazor secrets file.

## Host File Guidelines
- User means the host file App.Razor when referring to the host page; assistant must check the workspace for App.Razor before referencing or proposing edits to it. Prefer importing JS modules from components via IJSRuntime without requiring host-file edits unless the user requests it.
- When the user refers to the host/global JS loader, they mean a file like `app.blazor`; always check the workspace for that file before referencing or proposing edits, and ask the user to point to the exact file path. Do not assume host files exist.

## Debugging Guidelines
- When diagnosing missing dropdown/conversation entries, focus only on the UI dropdown data path: `Chat.razor` -> `ConversationsClientService` -> `/ai/conversations` API -> `ConversationsController` -> `ConversationService` -> `ConversationRepository`; avoid inspecting unrelated files (app.js, AiController) unless explicitly requested.
- When asked about API routes, confirm controller route attributes in code before asserting; ConversationsController uses [Route("api/ai/conversations")] while AiController uses [Route("api/[controller]")] which yields api/ai. Always confirm controller route attributes before asserting routes. When explaining route segments, point out the exact controller attribute (e.g., ConversationsController uses [Route("api/ai/conversations")]) and that AiController uses [Route("api/[controller]")] which resolves to api/ai.
- When the user corrects route handling, confirm controller route attributes before asserting routes; ConversationsController explicitly uses [Route("api/ai/conversations")] and AiController uses [Route("api/[controller]")] which resolves to api/ai. Do not conflate controllers; always check route attributes.

## Code Update Preferences
- User prefers full replacement file contents when requesting code updates (wants explicit file replacements rather than suggestions).