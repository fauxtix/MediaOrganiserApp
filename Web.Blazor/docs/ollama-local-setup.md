# Ollama (Local LLM) Setup — Windows (for MediaOrganizerApp)

This guide documents the **local setup steps** to run an LLM via **Ollama** on your dev machine, so you can test the AI fallback without paying for a hosted API.

Scope:
- Host OS: Windows
- App: `MediaOrganizerApp.WebApi` (server-side call)
- Client: `Web.Blazor` (calls WebApi; do **not** put LLM secrets in Blazor)

---

## 1) Install Ollama

1. Download and install Ollama for Windows from the official site:
   - https://ollama.com/
2. After installing, open a new PowerShell window.

---

## 2) Verify the Ollama service is running

In PowerShell:

```powershell
ollama --version
```

Then verify the local server responds (default port `11434`):

```powershell
Invoke-RestMethod -Method Get -Uri "http://localhost:11434/api/tags"
```

Expected:
- A JSON response (may be an empty `models` list if you haven’t pulled a model yet).

---

## 3) Pull a model (one-time)

Choose a small model first to keep download size reasonable.

Examples:

```powershell
# Smaller / faster (good for testing)
ollama pull llama3.2:1b

# Alternative
ollama pull phi3:mini
```

List installed models:

```powershell
ollama list
```

---

## 4) Quick local prompt test (CLI)

```powershell
ollama run llama3.2:1b "Write a short bio for an author named José Saramago in Portuguese (pt-PT)."
```

If this prints a response, the local model runtime is working.

---

## 5) Quick local API test (HTTP)

Ollama exposes an HTTP API. Test it directly:

```powershell
$body = @{
  model  = "llama3.2:1b"
  prompt = "Write a short bio for an author named José Saramago in Portuguese (pt-PT)."
  stream = $false
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Uri "http://localhost:11434/api/generate" -ContentType "application/json" -Body $body
```

Expected:
- A JSON response containing a `response` field with generated text.

---

## 6) Next step (wiring into this solution)

Once Ollama is working locally, the typical integration is:

- Add a new `IAiAuthorProfileService` implementation in `MediaOrganizerApp.WebApi` that calls `http://localhost:11434/api/generate`.
- Select between OpenAI vs Ollama via configuration (e.g., `Ai:Provider = "Ollama"`).

If you want, I can implement the Ollama-backed server-side service and a config switch, keeping the existing OpenAI path intact.
