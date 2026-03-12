# Copilot Instructions (Consolidated)

This file consolidates instruction rules present in:

- `.github\copilot-instructions.md`
- `..\..\..\Users\User\copilot-instructions.md`

## Project Guidelines

- User expects direct statements about tool limitations (e.g., when a complete analysis is not possible from limited context) and wants to avoid repetitive/looping responses.
- User confirmed the proposed approach for adding an `SfUploader`-based cover image picker to `EditMedia` works.
- User’s main pain point with external metadata APIs is low hit-rate (often can’t find authors), while LLM prompts usually return some response.
- User wants AI prompts (LLM) to replace some external metadata API calls (e.g., fetching author profiles via AI instead of Google Books/other APIs).
- AI fallback for metadata (e.g., author profiles) should run only when external API lookup fails.

## Code Snippets

- Include runnable code snippets in the guidance file to enhance understanding and usability.

## Additional Rules (from user-level instructions)

- Ensure to use the correct project name, `CalinicaSoft.Backend`, in all references.
- Avoid using incorrect names such as `AbsencesValitor.cs`.
- Follow the code style guidelines and naming conventions required by the project.
- Project-specific rules: Custom requirement A, Custom requirement B.
