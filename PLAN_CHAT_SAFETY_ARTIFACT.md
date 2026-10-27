# PLAN — Chat "User Safety: safe;" artifact

Status: IN PROGRESS

## Context
Both chats (Ollama `Chat.razor` and OpenRouter `OpenRouterChat.razor`) sometimes return
only a safety-classifier line (`User Safety: safe;`, `Response Safety: ...`,
`Safety Categories: ...`) instead of an answer. This is a model artifact (guard models /
free router), NOT the internal agents' `system.prompt.txt` (chats do not use it).

Approach (user-approved): sanitizer + "continua" hint + prompt hardening + docs.

## Checklist
- [x] 1. Shared detector `ChatSafetyArtifactFilter` in `Application/Utilities` (IsSafetyOnly / StripSafetyLines)
- [x] 2. Apply in `Chat.razor.cs` finalization (Ollama chat)
- [x] 3. Apply in `OpenRouterChat.razor.cs` finalization (cloud chat)
- [x] 4. Prompt hardening: OpenRouter `_systemPrompt` + Ollama `AiController.Chat` system message
- [x] 5. resx key `ChatSafetyArtifactHint` in App.resx / App.en / App.fr / App.es
- [x] 6. xUnit tests for the detector
- [x] 7. Docs: AGENTS.md AI Integration section
- [x] 8. `dotnet build` + `dotnet test MediaOrganizerApp.Tests` (Web.Blazor build 0 errors; 143/143 tests pass)
- [ ] 9. Delete this plan file only after commit

## Follow-up — intermittent empty Ollama chat answer (user-reported regression)
Cause: reasoning refactor dropped `thinking` when content empty; toast fired even with no text;
the always-on system message in `AiController` perturbed tiny models.

- [x] 10. `OllamaChatService.ExtractChunk`: never lose the answer — content wins; if content empty and `thinking` present, toggle ON → Reasoning, toggle OFF → Text fallback; re-added ` thinking`/`</think>` stripping. Model-agnostic (no reasoning support assumed).
- [x] 11. `AiController.Chat`: reverted to conditional system message (only when `Language` present), original wording (no safety clause).
- [x] 12. `Chat.razor.cs` finalization: empty answer → `ChatEmptyResponseHint` + warning toast; success toast only when there is real content; `IsError` when no answer.
- [x] 13. `OpenRouterChat.razor.cs` finalization: same defensive handling.
- [x] 14. `ChatSafetyArtifactFilter.StripSafetyLines`: never blanks a real answer (guard).
- [x] 15. resx `ChatEmptyResponseHint` in the 4 `App*.resx`.
- [x] 16. Tests: thinking-fallback + content-wins cases (145/145 pass); Web.Blazor build 0 errors.
- [ ] 17. Delete this plan file only after commit
