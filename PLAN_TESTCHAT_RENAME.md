# PLAN — Remover o prefixo legado "Test" / "TestChat"

## Objetivo

O chat OpenRouter chamava-se historicamente "TestChat". O rasto `TestChat*` ficou
no código e confunde cloners do repo (parece um ecrã/grupo "Test"). Remover todas
as ocorrências do legado (resx keys em 4 idiomas, classe `TestChatMessage`,
títulos de toast hardcoded `"Test Chat"`, comentário SQL) sem alterar valores
visíveis nem o JSON/BD de `Conversations` (usa DTOs, não nomes de classe).

## Progresso

- [x] Análise/plano aprovado (confirmado pelo utilizador, 2026-09-24)
- [x] 1. Resx `App.{resx,en,fr,es}.resx`: renomear 22 chaves `TestChat*` → `Chat*`; apagar 3 chaves mortas (`TestChatSelectModel`, `TestChatReasoning`, `TestChatReasoningTitle`) + `CloudChatTab` (morto, valor "OpenRouter ( Chat)")
- [x] 2. `OpenRouterChat.razor.cs`: classe `TestChatMessage` → `CloudChatUiMessage` (13 refs); chaves `TestChat*` → `Chat*`; toasts `"Test Chat"` (6×)` → `Localizer["Chat"]`
- [x] 3. `OpenRouterChat.razor`: chaves `TestChat*` → `Chat*`
- [x] 4. `Chat.razor.cs`/`Chat.razor` (chat local Ollama): chaves partilhadas `TestChat*` → `Chat*`; toast hardcoded `"Chat"` → `L["ChatRoomTitle"]` (opcional aceite)
- [x] 5. `HistoryPanel.razor` + `SidebarData.cs:60`: chaves `TestChat*` → `Chat*`
- [x] 6. `scripts/CreateAIChatSchema.sql:2`: comentário `"Test Chat" feature` → `Cloud Chat (OpenRouter)`
- [x] 7. Verificação: `dotnet build MediaOrganizerApp.sln` (0 erros), `dotnet test` (125/125), grep `TestChat|Test Chat|TestChatMessage|CloudChatTab` = 0 fora de bin/obj/PLAN_*.md

## Notas

- `App.Designer.cs` NÃO expõe `TestChat`/`CloudChatTab` — renomear chaves resx é seguro.
- Sem impacto em BD/JSON (`HistoryMessage` em ambos os chats já sem prefixo "Test").
- Trabalho em curso não-commitado nos ficheiros AI chat (streaming) — o rename intercala-se no working tree; não alterar lógica.
- `Web.Blazor\Web.Blazor.Tests\` = só bin/obj órfãos (gitignored, não está no .sln) — fora de âmbito.

## Renomeações de chaves resx

`TestChat->Chat`, `TestChatWelcome->ChatWelcome`, `TestChatContextExceeded->ChatContextExceeded`,
`TestChatContextExceededDetail->ChatContextExceededDetail`, `TestChatResponseReceived->ChatResponseReceived`,
`TestChatCancelled->ChatCancelled`, `TestChatError->ChatError`, `TestChatLanguageWarning->ChatLanguageWarning`,
`TestChatNewChat->ChatNewChat`, `TestChatHistory->ChatHistory`, `TestChatNoConversations->ChatNoConversations`,
`TestChatAssistantName->ChatAssistantName`, `TestChatUserName->ChatUserName`, `TestChatTokensUsed->ChatTokensUsed`,
`TestChatTokensRemaining->ChatTokensRemaining`, `TestChatExport->ChatExport`, `TestChatImport->ChatImport`,
`TestChatTemperature->ChatTemperature`, `TestChatExportOk->ChatExportOk`, `TestChatImportOk->ChatImportOk`,
`TestChatImportError->ChatImportError`, `TestChatExportError->ChatExportError`, `TestChatImportInvalid->ChatImportInvalid`.

Sem colisões: `ChatReasoning` já existe (a antiga `TestChatReasoning` é morta e é apagada).