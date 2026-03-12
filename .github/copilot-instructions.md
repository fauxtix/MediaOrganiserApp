# Copilot Instructions

## Project Guidelines
- User expects direct statements about tool limitations (e.g., when I cannot fully analyze the app from only SidebarData.cs) and wants to avoid repetitive/looping responses.
- User confirmed the proposed approach for adding an SfUploader-based cover image picker to EditMedia works.
- User’s main pain point with external metadata APIs is low hit-rate (often can’t find authors), while LLM prompts usually return some response. User wants AI prompts (LLM) to replace some external metadata API calls, e.g., fetching author profiles via AI instead of Google Books/other APIs. 
- AI fallback for metadata (e.g., author profiles) should run only when external API lookup fails.

## Code Snippets
- Include runnable code snippets in the guidance file to enhance understanding and usability.