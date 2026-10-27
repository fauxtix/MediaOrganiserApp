# PLAN — Apagar histórico nos dois chats

## Objetivo
Botão "Apagar histórico" no painel **Histórico** (`Web.Blazor/Components/Chat/HistoryPanel.razor`,
único consumidor: `Chat.razor` e `OpenRouterChat.razor`), à direita das ações existentes
(Exportar/Importar), com cor de perigo nos dois temas, confirmação explícita e aviso de que
o processo é irreversível.

## Decisões (com o utilizador)
- **Âmbito**: apaga tudo o que está na lista (Ollama + OpenRouter; para Admin inclui as
  conversas dos outros utilizadores). Reutiliza o endpoint já existente
  `DELETE /api/conversations/{id}` → `usp_Conversation_Delete` (que valida `@UserID`/`@IsAdmin`).
  Sem migração de BD, sem endpoint novo.
- **Estilo**: botão tonalizado em perigo (fundo/borda/texto `--app-danger-*`), não sólido.

## Notas técnicas (investigação)
- A tabela `Conversations` não tem coluna de interface → a lista é partilhada pelos dois chats.
- O `SfDialog` precisa de `ZIndex` > 9999 (o drawer usa 9999, o overlay 9998) e tem de ser
  renderizado **como irmão** de `.chat-history-aside`: o aside tem `transform: translateX(0)`,
  que cria containing block e prenderia o `position: fixed` do diálogo dentro do drawer.
- Depois de apagar, `selectedConversationId` ficaria a apontar para uma linha inexistente →
  o próximo `PUT` faria update de 0 linhas e a conversa deixaria de ser guardada. Daí o
  `EventCallback OnHistoryCleared` → cada chat faz `NewChat()` + `LoadConversations()`.
- `ChatFeedback` (👍/👎) fica órfão (sem FK) — inofensivo, fora do âmbito.

## Checklist
- [x] Criar este ficheiro de plano
- [x] `HistoryPanel.razor`: `@using Web.Blazor.Components`, 3.º botão com `margin-left: auto`,
      `ConfirmAppWarning` fora do aside (Error / `OpcoesRegisto.Apagar` / 440px / ZIndex 10010),
      `DeleteAllConversationsAsync()` (loop de deletes + `_status` + `Conversations.Clear()` +
      `OnHistoryCleared.InvokeAsync()`), novo `OnHistoryCleared`, CSS com tokens `--app-danger-*`
- [x] `ConfirmAppWarning.razor`: `[Parameter] public int ZIndex { get; set; } = 1000;` → `SfDialog ZIndex="@ZIndex"`
- [x] `Chat.razor(.cs)` e `OpenRouterChat.razor(.cs)`: `OnHistoryCleared="OnHistoryClearedAsync"` +
      `private async Task OnHistoryClearedAsync() { await NewChat(); await LoadConversations(); }`
- [x] 6 chaves novas em `App.resx`, `App.en.resx`, `App.fr.resx`, `App.es.resx`:
      `ChatDeleteHistory`, `ChatDeleteHistoryTitle`, `ChatDeleteHistoryMessage`,
      `ChatDeleteHistoryWarning`, `ChatDeleteHistoryOk`, `ChatDeleteHistoryError`
- [x] `AGENTS.md`: uma linha na secção dos chats
- [x] `dotnet build Web.Blazor` → 0 erros (verificado com `-p:OutDir=<temp>`: a app em
      execução no IIS Express bloqueia `bin\Debug\Web.Blazor.dll`, o lock conhecido do AGENTS.md)

## Ficheiros tocados
- `Web.Blazor/Components/Chat/HistoryPanel.razor`
- `Web.Blazor/Components/ConfirmAppWarning.razor`
- `Web.Blazor/Pages/AI/Chat.razor` + `Chat.razor.cs`
- `Web.Blazor/Pages/AI/OpenRouterChat.razor` + `OpenRouterChat.razor.cs`
- `Web.Blazor/Resources/App.resx`, `App.en.resx`, `App.fr.resx`, `App.es.resx`
- `AGENTS.md`

> Ficheiro de trabalho local: NÃO versionar. Apagar só depois de o trabalho estar concluído e
> commitado.
