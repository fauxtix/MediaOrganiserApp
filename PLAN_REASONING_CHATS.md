# PLAN: Mostrar "reasoning"/"thinking" nos dois chats (OpenRouter + Ollama)

Objetivo: quando o toggle global admin `CloudChat:EnableReasoning` está ON, os dois
chats (OpenRouter `/ai/openrouter-chat` e Ollama `/ai/chat`) mostram o raciocínio do
modelo (se o modelo o produzir) num bloco colapsável por mensagem. Quando OFF, o
raciocínio é descartado (não é injetado na resposta).

Trigger único: o toggle existente `CloudChat:EnableReasoning` (só admin). Preferência
do utilizador: não existe switch por-chat.

## Alterações

- [x] 1. DTO `AiChatChunk` (`Application/DTOs/AI/AiChatChunk.cs`) — `Text` + `Reasoning?`.
- [x] 2. `IAiChatService.StreamChatAsync` passa a `IAsyncEnumerable<AiChatChunk>`.
- [x] 3. `OllamaChatService`: injetar `IAIChatSettingsRepository`, ler o toggle
      (`CloudChat:EnableReasoning`, default OFF), e:
        - `message.content` → chunk `Text`;
        - `message.thinking` → chunk `Reasoning` **apenas** quando toggle ON;
        - remover o `Clean()` que estripava " thinking"/" response".
- [x] 4. `DevAiChatService`: yield de `AiChatChunk` (dev placeholder).
- [x] 5. `AiController.Chat`: output NDJSON (uma linha JSON por chunk).
- [x] 6. `AiClientService.StreamChatAsync`: ler linhas NDJSON → `IAsyncEnumerable<AiChatChunk>`.
- [x] 7. `Chat.razor.cs` (Ollama): acumular `chunk.Text`+`chunk.Reasoning`; `ChatUiMessage.Reasoning`;
      persiste/restaura `Reasoning` no histórico (`HistoryMessage`).
- [x] 8. `Chat.razor`: bloco colapsável "💭 Raciocínio" acima da resposta + CSS tokenizado.
- [x] 9. `OpenRouterChat.razor.cs`: acumular `chunk.Reasoning` (coalescing);
      `CloudChatUiMessage.Reasoning`; persiste/restaura no histórico.
- [x] 10. `OpenRouterChat.razor`: bloco colapsável "💭 Raciocínio" + CSS.
- [x] 11. `CopilotChat.razor.cs`: usar `chunk.Text`.
- [x] 12. resx `ChatReasoning`/`ChatReasoningThinking` em pt/en/fr/es.
- [x] 13. `AGENTS.md` — nota sobre display de reasoning + gating do Ollama (doc drift).
- [x] 14. `dotnet build MediaOrganizerApp.sln` (0 erros) + `dotnet test` (125/125 OK).

Apagar este ficheiro no fim (não commitar).