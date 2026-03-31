# Copilot Instructions

## Project Guidelines
- User expects direct statements about tool limitations (e.g., when I cannot fully analyze the app from only SidebarData.cs) and wants to avoid repetitive/looping responses.
- User confirmed the proposed approach for adding an SfUploader-based cover image picker to EditMedia works.
- User’s main pain point with external metadata APIs is low hit-rate (often can’t find authors), while LLM prompts usually return some response. User wants AI prompts (LLM) to replace some external metadata API calls, e.g., fetching author profiles via AI instead of Google Books/other APIs. 
- AI fallback for metadata (e.g., author profiles) should run only when external API lookup fails.
- When multiple plausible author candidates are found, choose the single candidate with the highest internal confidence and return only that candidate's profile; if only one candidate is found, return it normally. Explain ambiguity in Notes and lower Confidence. Use a 0.0–1.0 scale; thresholds: low confidence < 0.5, tie difference < 0.15 indicates ambiguity.
- Always run a build after any code change to ensure correctness and functionality.

## Service Registration Guidelines
- When registering new services that depend on HttpClient, always use `AddHttpClient` in the `AddServicesInfrastructure` method (ServiceRegistration.cs) and never register directly in `Program.cs`. Follow the pattern of other services in the project to avoid duplication and lifecycle issues.

## SignalR Guidelines
- Avoid modifying the UI for SignalR or Hubs-related operations. All SignalR-related work should be handled in the service layer, not the UI.

## UI Implementation Guidelines
- Apply requested UI changes directly without presenting multiple options. For example, ensure cards are the same height, images fill the container, and titles are clamped to two lines.
- When rendering author/book UI, prioritize styling of book cover images (thumbnails). Treat the author's portrait as unimportant for book-card layout; it's okay if the author's portrait is cropped. Focus component changes on book image styling (thumbnail sizing, object-fit, card layout).

## Code Snippets
- Include runnable code snippets in the guidance file to enhance understanding and usability.

## AI Metadata Rules
- AI responses for "Notes" must always pertain to LPs, CDs, or DVDs and should never describe songs. Ensure that AI-generated content aligns with these formats.

## Prompt and Modification Guidelines
- Do not modify the solution or files unless the user explicitly requests changes; prefer making suggestions and asking before applying edits. User prefers I do not change files unless they explicitly ask; they want diagnostics and explanations rather than automatic edits.
- Do not invent tasks or actions modifying the repo. Only update prompts and provide suggestions.
- When correcting the assistant's inference about JS interop, avoid asserting JS interop exists in a file unless explicitly present; ask for exception stack trace or offer diagnostics instead of making unsupported assumptions.
- When the user corrects the assistant's inference about component internals, analyze provided files before asserting implementation details and avoid changing files unless the user explicitly requests modifications. User expects the assistant to analyze provided files before asserting implementation details and prefers no file changes unless explicitly requested.
- Do not continue proposing fixes for the spinner label issue unless the user explicitly asks to resume; further attempts are considered a waste of time.
- When the assistant is uncertain or lacks evidence, do not hallucinate fixes or assert implementation details; ask for more information (stack trace, file contents) before proposing code changes. Avoid unsolicited file edits and repeated speculative solutions.