# PLAN — Streaming OpenRouterChat: coalescing + TTFT (WIP)

Goal: make the OpenRouterChat page stream with the same fluid rendering cadence as
Chat.razor (Ollama), by coalescing tiny SSE deltas into larger render batches, and
cut the pre-stream (TTFT) catalog fetch.

## Progress

- [x] 1. Coalescer no cliente — `OpenRouterChat.razor.cs`: accumulate `chunk.Text` in a
      pending buffer; `StateHasChanged` only when accumulated >= 400 chars OR >= 50ms
      elapsed (with pending text); forced flush at end. Metadata (model/usage) still
      applied per chunk. No marker/metadata lost.
- [x] 3. Diferir o pré-flight do catálogo — `OpenRouterChatService.StreamChatAsync`:
      `BuildChatCandidatesAsync` só é chamado quando um candidato termina sem texto
      (lazy), em vez de antes do primeiro token. `initialModel` capturado antes do loop.
- [x] 4. Saltar `StateHasChanged` em chunks sem texto/delta — coberto pelo coalescing
      (só rende quando há texto pendente).
- [x] 5. Build + testes (`dotnet build`, `dotnet test MediaOrganizerApp.Tests` — 125 ok,
      10 OK no filtro OpenRouterChat) + teste novo `StreamChatAsync_DoesNotFetchCatalog_BeforeFirstToken`.

## Notas

- Chat.razor (Ollama) fica intacto.
- `CloudChatClientService` / protocolo NDJSON inalterados (metadados preservados).
- Ficheiro de trabalho local — não commitar; apagar quando o trabalho estiver feito e committed.