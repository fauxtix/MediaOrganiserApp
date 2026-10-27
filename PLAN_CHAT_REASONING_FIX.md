# PLAN — Corrigir reasoning perdido no chat Ollama

Regressão introduzida em `OllamaChatService.ExtractChunk`: `Text` e `Reasoning` eram tratados
como mutuamente exclusivos (early-return no `content is { Length: > 0 }`). Quando o Qwen3
emite tokens `content` whitespace-only durante/em volta do raciocínio, o `thinking` era
abandonado → não aparecia reasoning nenhum (só a resposta).

**Escopo aprovado (fase 2):** aplicar as diferenças do repo de referência que funciona
(`fauxtix/OllamaArena` — `Chat.razor.cs` / `ChatComposerService.cs`) ao `OllamaChatService`:
parser aceita `reasoning_content` OU `thinking`, re-parse da mensagem `done:true`, e campo
`think` no payload para modelos thinking.

## Progresso

- [x] 1. `OllamaChatService.ExtractChunk`: extrair `Text` e `Reasoning` de forma independente (sem early-return); devolver `null` só se ambos vazios.
- [x] 2. Teste `OllamaChatServiceTests`: novo caso com `content` whitespace-only + `thinking` → chunk com `Reasoning` preenchido (`StreamChatAsync_WhitespaceContentDoesNotHideThinking_WhenReasoningEnabled`).
- [x] 3. `AGENTS.md`: atualizar bullet reasoning com semântica independente Text/Reasoning.
- [x] 4. `dotnet test MediaOrganizerApp.Tests` (146 passaram, 0 falhas).
- [x] 5. Build Debug + Release (0 erros em ambos).
- [x] 1F. `ExtractReasoning` lê `reasoning_content` (se não-vazio) com fallback `thinking`; `showReasoning=false` → `null`.
- [x] 2F. Loop de streaming: re-parse da linha `done:true` — se o chunk final não trouxer Reasoning, extrair do JSON final (sem duplicar quando já presente).
- [x] 3F. Payload passa a `Dictionary<string,object?>`, com `payload["think"] = showReasoning` apenas quando `SupportsThinking(model)` (qwen3*, deepseek-r1*, qwq*, *:reasoning) — omitido para modelos comuns.
- [x] 4F. 6 novos testes `OllamaChatServiceTests` (reasoning_content; reasoning só no done:true; sem duplicação; think presente/omitido; content sem reasoning).
- [x] 5F. `dotnet build` sln (0 erros) + suite completa `dotnet test MediaOrganizerApp.Tests` (151/151 passam; 10 só de `OllamaChatServiceTests`).
- [x] 6F. `AGENTS.md` bullets reasoning atualizados (reasoning_content OR thinking; re-parse done:true; campo think com `SupportsThinking`).
- [x] 6. Validado em runtime (WebApi IIS Express a correr): sonda a `https://localhost:44303/api/ai/chat` (ApiKey header) com `language:"pt"` e Qwen3:1.7B → NDJSON OK, 148 chunks com `"Reasoning"` + final `{"Text":"4","Reasoning":null}` — reasoning flui e a resposta não é roubada. Confirmado na UI fica ao critério do user.
- [x] 7. **Inline thinking fix (25/09):** sessão do user às 10:45 mostrou que o Qwen3 pode emitir o think INLINE no `content` (wire: `{"Text":"\u003C/think\u003E","Reasoning":null}`) em vez do campo `message.thinking` → `Reasoning` null no cliente e o breakpoint `Chat.razor` L90 nunca dispara. `CleanThinkingMarkers` (só remove tokens nus `" thinking"`/`" response"`) não cobria os marcadores com tags. Novo `ProcessInlineThinking` + estado cross-chunk (`pendingInlineReasoning` + `inInlineThinking`) em `StreamChatAsync`: extrai o bloco ` thinking`...` response` para `Reasoning` (toggle ON) ou descarta-o (OFF); flush no `done:true` se bloco ficou aberto; campo `thinking`/`reasoning_content` mantém prioridade sobre o inline. 7 novos testes `OllamaChatServiceTests`, suite 158/158, build sln 0 erros. AGENTS.md bullet atualizado.
- [ ] 8. Validar E2E na UI (WebApi a correr) com Qwen3: reasoning aparece em "💭 Raciocínio" e marcadores não vazam para a resposta.
- [ ] 9. Apagar `PLAN_CHAT_REASONING_FIX.md` (e os planos antigos) só após commit (feito pelo user).

## Passagem atual — correção do fluxo Ollama (25/09/2026)

Scope confirmado: apenas `Chat.razor.cs`, `OllamaChatService.cs`, testes e documentação. OpenRouter e restantes chats ficam intocados.

- [x] 10. `Chat.razor.cs`: excluir o placeholder assistant `"..."` do payload enviado ao Ollama (linha 90) — o Qwen3 via confirmação do user.
- [x] 11. `OllamaChatService`: parser inline com carry entre chunks, descarte de `</think>` órfão e flush no fim do stream.
- [x] 12. `OllamaChatServiceTests`: regressões para marcadores partidos, marcador órfão e flush sem `done:true` (+ ` response` fora do bloco e marcadores mixed-case).
- [x] 13. `AGENTS.md`: alinhar a documentação com o parser cross-chunk (bullet "Reasoning display" descreve `inlineMarkerCarry`, órfãos e flush em `done:true`/EOF).
- [x] 14. Build da solução Debug + Release (0 erros) + suite completa `dotnet test MediaOrganizerApp.Tests` (163/163, 22 em `OllamaChatServiceTests`).
- [ ] 15. Validação E2E na UI com Qwen3 (WebApi e Web.Blazor a correr) — pendente: nenhum serviço estava à escuta (verificado via `Get-NetTCPConnection`).

## Notas

- Não commitar nada — o user faz commit.
- Não mexer em `OpenRouterChat.razor` (cliente de referência que funciona).
- `AGENTS.md` bullet reasoning já descreve: `content` sempre ganha e é extraído independentemente de `thinking`; `thinking` → `Reasoning` com toggle ON, descartado com OFF.
- Ficheiros tocados:
  - `MediaOrganizerApp.WebApi/Services/OllamaChatService.cs`
  - `MediaOrganizerApp.Tests/OllamaChatServiceTests.cs`
- Planos anteriores por apagar pós-commit: `PLAN_CHAT_TOKEN_SPACING.md`, `PLAN_CHAT_SAFETY_ARTIFACT.md`.