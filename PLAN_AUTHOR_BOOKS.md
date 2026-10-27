# PLAN — AuthorBooks sem resultados (Google Books + OpenRouter)

Diagnóstico verificado ao vivo (2026-10-01) com as chaves da BD `Publicacoes_Prod`.

- [x] **Investigação**: chave Google Books válida (`ApplicationSettings.GoogleBooksApiKey`, len=39, `GoogleBooksCountry=PT`); chave OpenRouter válida (`AIChatSettings.CloudChat:OpenRouterApiKey`, `is_free_tier=true`, `usage=0`, `total_credits=0`).
- [x] **Causa 1 — Google Books**: `inauthor:"…"` / `intitle:"…"` devolvem `HTTP 200` + `totalItems: 0` (sem erro). Sem aspas funciona. O Google deixou de aceitar frase exata nestes operadores.
- [x] **Causa 2 — OpenRouter (a)**: o router `openrouter/free` serviu `nvidia/nemotron-3.5-content-safety:free` → `"User Safety: safe"`; `ChatSafetyArtifactFilter` só existe nos dois chats.
- [x] **Causa 2 — OpenRouter (b)**: os modelos `:free` do catálogo devolvem `429 … temporarily rate-limited upstream (upstream_provider_shared_pool)`.
- [x] **Descartado**: tecto interno da app (6/50 hoje), falta de internet, ausência de chave. Confirmado que modelos pagos dão `402 Insufficient credits` → `AllowPaidModels` fica `false`.
- [x] **Descartado**: `X-RateLimit-*` já não é enviado pelo OpenRouter → `SyncFreeUsageFromHeadersAsync` é código morto.

## Implementação

- [x] **1. `Web.Blazor/Services/ApiWrappers/GoogleBooksClientService.cs`**
  - [x] `GetBooksByAuthorAsync` (linha 92): `inauthor:<autor>` sem aspas; pedir `volumeInfo/authors` no `fields`.
  - [x] `GetBookAsync` (linhas 228/230): `intitle:<título>` / `inauthor:<autor>` sem aspas; comentário das aspas removido (e o dead-code do retry de título traduzido eliminado).
  - [x] DTO `Volumeinfo`: novo `authors`; filtrar client-side por `Application/Utilities/AuthorNameMatcher` (novo ficheiro, testes próprios) com fallback para a lista não filtrada.
  - [x] Deixar de logar a chave em claro (usar o `MaskUrl` já existente) — o Serilog grava em `AppLogs`.
- [x] **2. `MediaOrganizerApp.WebApi/Services/CloudChat/CloudAuthorBooksService.cs`**
  - [x] Aplicar `ChatSafetyArtifactFilter` antes do parse (strip + detecção de "só safety" → segue o sweep).
  - [x] Excluir modelos classifier/guard do ranking do catálogo livre.
  - [x] 1 retry com backoff quando o 429 for *upstream* (delay de 6s, inyectável no construtor para os testes; 429 de conta não é repetido e o retry respeita o tecto diário).
- [x] **3. Docs** — `AGENTS.md` (linha 69) e `Web.Blazor/docs/ai-fallback-guidelines.md` (nova secção "Author books").
- [x] **4. Validação** — `WebApi` e `Web.Blazor` com 0 erros; `dotnet test MediaOrganizerApp.Tests`: **192/192 passed** (38 nos testes desta tarefa: 24 `AuthorNameMatcherTests` + `CloudAuthorBooksServiceTests`).

## Fora de âmbito (decidido)

- Não mexer em `CloudChat:AllowPaidModels` / `FreeDailyCap` (a conta tem 0 créditos; modelos pagos dão 402).
- Não activar o provider routing do OpenRouter.
- Remover o código morto `SyncFreeUsageFromHeadersAsync` fica para depois (é inofensivo).