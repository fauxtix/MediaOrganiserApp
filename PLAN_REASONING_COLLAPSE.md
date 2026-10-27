# PLAN — Corrigir colapsar/expandir do bloco "Raciocínio" nos dois chats

## Problema

Nos dois chats (`OpenRouterChat`, `/ai/openrouter-chat`; `Chat` Ollama, `/ai/chat`),
o toggle 💭 Raciocínio não colapsa/expande a mensagem em streaming nem a última
mensagem.

Causas:
1. `currentAssistantMessage` nunca é posto a `null` no fim do `Send()` (`finally`);
2. `IsReasoningExpanded = _expandedReasoning.Contains(i) || msg == currentAssistantMessage`
   força expandido na mensagem atual/última, ignorando o toggle do utilizador;
3. `ToggleReasoning` (HashSet de "expandidos") está invertido para esse caso.

## Comportamento pretendido (confirmado pelo utilizador)

- Bloco de reasoning **colapsado por defeito**, inclusive durante o streaming.
- O utilizador expande/colapsa manualmente com o toggle; a escolha é respeitada.
- Sempre que não há override do utilizador, o bloco aparece colapsado.

## Alterações

- [x] 1. `OpenRouterChat.razor.cs` + `Chat.razor.cs`: substituir `HashSet<int> _expandedReasoning`
      por `Dictionary<int, bool> _reasoningToggled` (override por índice).
- [x] 2. `IsReasoningExpanded`: passar a devolver apenas o override do utilizador
      (default = colapsado).
- [x] 3. `ToggleReasoning`: flip do override; guard quando `index` fora de `messages`.
- [x] 4. `Send()` (ambos): no `finally`, pôr `currentAssistantMessage = null` após
      setar `ElapsedTime`.
- [x] 5. `NewChat`/`LoadConversationAsync` (ambos): `.Clear()` de `_reasoningToggled`.
- [x] 6. `OpenRouterChat.razor.cs`: remover chamada duplicada a
      `FlushPendingReasoning` (linha 297) — artefacto de merge.
- [x] 7. `AGENTS.md` (doc drift): the reasoning paragraph deixa de dizer
      "auto-open while streaming" → passa a "collapsed by default (inclusive durante
      o streaming), o utilizador expande/colapsa manualmente".
- [x] 8. Verificação: `dotnet build Web.Blazor` (isolado) + `dotnet test MediaOrganizerApp.Tests`. Build 0 erros (430 warnings pré-existentes); testes 125/125 OK.

Apagar este ficheiro no fim (não commitar), como os restantes PLAN_*.md.