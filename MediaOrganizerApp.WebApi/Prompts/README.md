## Web API prompt templates (Ollama / OpenAI)

This folder contains **prompt templates** used by `MediaOrganizerApp.WebApi` AI fallback services.

### Why templates (not hard-coded strings)

- Prompts need frequent iteration (format strictness, hallucination reduction, language rules).
- Keeping them as files makes changes reviewable and easier to maintain.
- Code stays focused on:
  - selecting the right template
  - injecting variables (e.g., `language`, `title`, `author`)
  - enforcing timeouts / parsing / validation

### How templates are loaded

- Templates are loaded at runtime by a server-side provider and treated as **content files**.
- They must be deployed alongside the Web API.

### Template format

- Templates are plain UTF-8 text files.
- Use placeholders in `{braces}`.

### Supported placeholders

Common placeholders used in this solution:

- `{language}`: UI language code (e.g., `pt`, `en`)
- `{artist}` / `{title}`: media prompts
- `{author}` / `{bookTitle}`: book prompts
- `{name}`: author-profile prompts

## Editing guidelines

1. Keep the "Return ONLY valid JSON" instruction if the endpoint expects JSON.
2. Keep JSON schema keys **exactly** matching the DTO contract.
3. Avoid adding extra keys—parsers may ignore, but clients may not.
4. Avoid adding Markdown/code fences.
5. Keep the prompt language-neutral; rely on `{language}`.

## Adding a new prompt

1. Add a new template file here.
2. Ensure the Web API service references it by name.
3. Add/adjust variables passed to the template.
4. Test the endpoint end-to-end (Blazor → Web API → AI provider).

---

## Examples

### Example 1: Edit an existing prompt safely

Scenario: you want the media notes to be *shorter*.

1) Open `Prompts/media-notes.prompt.txt`
2) Change only the relevant rule, keep everything else (schema, key names, placeholders).

For example, change this line:

`- notes: plain-text liner notes / awards / key highlights, 300-800 characters.`

to:

`- notes: plain-text liner notes / awards / key highlights, 200-400 characters.`

Guidelines:

- Do **not** rename JSON keys (e.g., keep `warnings`, not `warning`).
- Do **not** add new keys unless you also update the DTO contract and parsing.
- Keep placeholders exactly as `{language}`, `{artist}`, `{title}`.

### Example 2: Add a new prompt template file

Scenario: you want an AI endpoint that generates *tags/keywords* for a media item.

1) Create a new template, e.g. `Prompts/media-tags.prompt.txt`:

```text
You are a media tagging assistant.
Return ONLY valid JSON (no markdown, no code fences).
JSON schema: {"artist":string,"title":string,"tags":string,"confidence":number,"warnings":string}
Rules:
- Echo the EXACT input values in the JSON fields 'artist' and 'title'.
- tags: comma-separated list of 5-12 tags.
- Do NOT invent factual claims; keep tags generic if unsure.
- Provide a confidence score between 0.0 and 1.0.

INPUT (must match exactly):
Language: {language}
Author: {artist}
Title: {title}
```

2) Reference it from your Web API service:

- Load it via `IPromptTemplateProvider.GetTemplateAsync("media-tags.prompt.txt")`
- Render variables via `PromptTemplateRenderer.Render(template, ...)`

3) Add DTOs and endpoint (if needed)

- Add request/response DTOs in `Application/DTOs`
- Add an endpoint in `MediaOrganizerApp.WebApi/Controllers/AiController.cs`
- Add/extend an interface in `Application/Interfaces`

4) Test end-to-end

- Verify JSON parsing and that `tags` is present and well-formed.
- Verify language changes (e.g., `pt` vs `en`) influence the output.

### Example 3: Add a new placeholder

Scenario: you want to include a year hint in a prompt.

1) Add it to the template:

```text
Year hint: {year}
```

2) Ensure the service passes it when rendering:

- Add `["year"] = yearValue` to the render dictionary.

If `{year}` is missing from the render values, it will remain unchanged in the final prompt, which usually confuses the model.

---

## Deployment note

These prompt files must be deployed with the Web API under the `Prompts/` folder (relative to the Web API content root). If you publish and see "Prompt template not found", ensure the prompt files are included as content in publish output.
