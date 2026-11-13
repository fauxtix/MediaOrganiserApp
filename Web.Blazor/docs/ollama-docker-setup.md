# Ollama in the Docker stack (Linux containers)

This guide explains how to get the **local Ollama AI** working when Media Organizer runs from
`docker-compose.yml`, and why the chat / AI fallbacks may report that no model is available.

Scope:
- Stack: `sql` + `db-init` + `ollama` + `webapi` + `webblazor` (`docker compose up --build`)
- App: `MediaOrganizerApp.WebApi` calls Ollama server-side; `Web.Blazor` only calls the WebApi
- For a native Windows install (no Docker) see [`ollama-local-setup.md`](ollama-local-setup.md)

---

**Database `.bak` prerequisite:** the Docker stack needs a `Publicacoes_Prod` backup in
`Web.Blazor/wwwroot/Files/Backup/Database/` (mounted as `/backup` into the `sql` service). If its name is not `28102026_123256.Bak`, set `SQL_BACKUP_FILE` in `.env`. Without the `.bak`, `db-init` fails and the stack will not start.

## 1) There are two independent Ollamas

The compose file starts an **Ollama container**. That is a separate installation from any Ollama
running on the host:

| | Host Ollama (Windows) | Ollama container |
|---|---|---|
| Process | `ollama` / `ollama app` on the host | `ollama serve` inside the container |
| Listens on | `127.0.0.1:11434` | `ollama:11434` (compose network, not published) |
| Model store | `C:\Users\<you>\.ollama` | compose volume `ollama-data` (`/root/.ollama`) |
| Models | whatever you pulled on the host | **none** on a fresh volume |

Consequences:
- The container **cannot see** the host's models: different filesystem, and a host Ollama bound to
  `127.0.0.1` is not reachable from inside the container.
- The app in Docker talks to the **container**, so the host's models are irrelevant to it (unless you
  point the app at the host — see §5).

The image ships the Ollama **engine** only. Model files are not part of any Ollama image; they are
downloaded at runtime and stored in `/root/.ollama`, which is why the `ollama-data` volume exists:
it keeps the models across container recreation.

---

## 2) The model is a user setting, not a project default

The local AI (translate fallback, media notes/metadata, edition year, author books, book summary,
author profile and the local chat) always uses **one** model at a time, read from
`ApplicationSettings.AiModelName` through `AiSettingsProvider` (60 s cache, invalidated when the
settings are saved).

`AiModelName` is **user state**: it is changed in **Admin → App Settings → IA → Ollama (local)** and
applies immediately. The sample database restored by `db-init` carries whatever model was selected
when the backup was taken (currently `Qwen3:1.7B`) — treat it as an example, not as a requirement.

The only invariant is: **`AiModelName` must name a model installed in the Ollama reachable at
`ModelBaseUrl`**. `db-init` sets `ModelBaseUrl=http://ollama:11434`; a name that is not installed
makes every local call fail with `404 model "..." not found`.

The app never lists models "loaded in RAM" (`/api/ps`). It lists **installed** models
(`/api/tags`). Ollama loads a model on the first request and evicts it later, so there is no
"loading" step to perform.

---

## 3) Install a model in the container

```bash
# from the repository root (not from docker/)
docker compose up -d ollama

# pull any model you want (tags are case-insensitive)
docker compose exec ollama ollama pull qwen3:1.7b

# verify it is installed
docker compose exec ollama ollama list
```

Reference sizes (download + volume usage):

| Model | Approx. size |
|---|---|
| `qwen3:1.7b` | ~1.4 GB |
| `llama3.2:3b` | ~2.0 GB |
| `phi4-mini:3.8b` | ~2.5 GB |

Pulls are resumable: if the download is interrupted, run the same command again.

Models persist across `docker compose down` / `docker compose up`. `docker compose down -v` deletes
the volume — and with it both the models **and** the database (which `db-init` restores again on the
next `up`, including the `.bak`'s `AiModelName`).

---

## 4) Select the model in the app

1. Open **Admin → App Settings → IA → Ollama (local)**.
2. The **model** dropdown is fed by `GET /api/AIModels`, which proxies the container's `/api/tags`.
   Pick the model you pulled.
3. Save. `AppSettingsController` invalidates the AI settings cache, so the change is used by the next
   request.

Models can also be installed/removed from **Admin → Lookup tables → AI Models** (the same
`/api/pull` and `/api/delete` endpoints). Note that the Blazor client for that page uses the default
100 s HTTP timeout, so very large models are better pulled from the CLI (§3).

> Caveat: if the model list cannot be loaded (Ollama down), the settings page may render empty
> `ModelBaseUrl` / `AiModelName` fields. Do **not** save in that state — pull the model from the CLI
> first and reload the page.

---

## 5) Use the Ollama installed on the host instead (optional)

If you prefer a single model store (the host's), point the app at it:

1. Start the host Ollama listening on all interfaces (PowerShell):
   ```powershell
   setx OLLAMA_HOST "0.0.0.0:11434"
   ```
   Restart Ollama, and allow port `11434` through the Windows firewall.
2. If `host.docker.internal` does not resolve from the app containers, add the host gateway to
   `webapi` and `webblazor` in `docker-compose.yml` (Docker Desktop usually resolves it already):
   ```yaml
   extra_hosts:
     - "host.docker.internal:host-gateway"
   ```
3. In **Admin → App Settings → IA → Ollama (local)**, set `ModelBaseUrl` to
   `http://host.docker.internal:11434` and pick one of the host's models.
   Do this **after** `docker compose up`: `db-init` rewrites `ModelBaseUrl` back to
   `http://ollama:11434` on every start, so the change must be re-applied after each `up` (or made
   permanent in `docker/init-db.sh`).

With this setup the `ollama` service is unused and the `ollama-data` volume stays empty. The trade-off
is that the stack now depends on the host process and on the firewall rule.

---

## 6) Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Model dropdown empty in App Settings | no models in the container | §3 (pull a model) |
| `404 model "..." not found` in logs / AI usage | `AiModelName` is not installed in that Ollama | pull that name (§3) or select another (§4) |
| Local chat returns "model returned no response" | same 404: the request never produced a chunk | §3 / §4 |
| Local chat shows "could not get a response" with a connection error | `ollama` container not running / wrong `ModelBaseUrl` | `docker compose up -d ollama`; check §5 if using the host |
| AI features work only through OpenRouter | cloud-first path is fine; the local fallback is the one failing | fix the model as above |
| `sql` container exits with code 137 | not enough memory for SQL Server 2022 | raise WSL2/Docker Desktop memory (~2 GB for SQL; a 3B model needs ~2.5–3 GB more) |

Useful checks:

```bash
docker compose ps
docker compose exec ollama ollama list
docker compose logs --tail 100 ollama webapi
```

Optional — confirm what the app has configured in the database:

```bash
docker compose exec sql bash -c '/opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d Publicacoes_Prod \
  -Q "SELECT AiModelName, ModelBaseUrl FROM ApplicationSettings"'
```

---

## 7) Current limitations (as of this writing)

- **No automatic model provisioning**: `docker compose up` does not download models; each user
  chooses and pulls their own. An opt-in `OLLAMA_PULL_MODELS` / `ollama-init` service is not
  implemented yet.
- **No automatic model switch**: if `AiModelName` is not installed the app does not silently pick
  another model — it fails and the user must choose one.
- **`Ollama__BaseUrl` is not the source of truth**: the compose variable only sets the shared
  Ollama `HttpClient` base address. Every AI call builds an absolute URI from the database
  `ModelBaseUrl`, so changing the variable alone does not redirect the app.
- **The settings dropdown requires Ollama to be reachable**; there is no free-text fallback yet.
