# PLAN — Chat (Ollama) preservar espaços/quebras nos tokens

Bug: `Chat.razor` mostra a resposta sem espaços ("Oh,thisischallenging...") e sem
quebras de linha, independentemente do modelo.

Causa: `OllamaChatService.ExtractChunk` faz `.Trim()` em **cada** chunk do Ollama
(token-a-token) e descarta tokens só-espaço via `IsNullOrWhiteSpace`. Regressão
introduzida nas alterações não commitadas (o antigo `Clean` não fazia trim).
`FormatMessage` está intacto e não é a causa.

## Progress

- [x] 1. `OllamaChatService.ExtractChunk`: remover `.Trim()`; devolver `Text`/`Reasoning`
      crus, incluindo tokens whitespace-only (preservar espaços e `\n`).
- [x] 2. Teste de regressão `MediaOrganizerApp.Tests/OllamaChatServiceTests.cs` (2 testes).
- [x] 3. Confirmado: FormatMessage / Chat.razor / AiClientService / AiController
      inalterados (OpenRouterChatService não faz trim).
- [x] 4. `dotnet test MediaOrganizerApp.Tests` — 127 passaram, 0 falhas.
- [x] 5. Docs: sem alteração necessária.

Ficheiro de trabalho local — não commitar; apagar no fim.
