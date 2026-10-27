# PLAN_TRANSLATE_CANCEL

**Objetivo:** permitir cancelar a tradução em curso (bio de autor e os restantes locais que usam
`TranslateText`), e impedir que uma tradução terminada depois de o utilizador ter fechado o diálogo
escreva por cima do texto.

**Decisões do utilizador:** (1) botão "Cancelar" visível **e** `OnExit`/`Dispose` a cancelar em
silêncio; (2) incluir o timeout de 6 min no `HttpClient`; (3) o cancelamento no UI é **imediato** —
não espera por confirmação do servidor (o Ollama local pode continuar a inferir sozinho).

## Checklist

- [x] 1. `Web.Blazor/Pages/Tools/TranslateText.razor.cs` — `CancellationTokenSource` + `CancelTranslate()` + guarda de corrida
- [x] 2. `Web.Blazor/Pages/Tools/TranslateText.razor` — botão "Cancelar" + mensagem "Cancelado pelo utilizador" à direita (o botão Traduzir mantém-se sempre disponível à esquerda; insister em traduzir limpa a mensagem)
- [x] 3. `MediaOrganizerApp.WebApi/Services/OllamaTranslateService.cs` — rethrow de `OperationCanceledException`
- [x] 4. `Web.Blazor/Services/ApiWrappers/TranslateClientService.cs` — rethrow do cancelamento no fallback local
- [x] 5. `Web.Blazor/ServiceRegistration.cs` — `Timeout` de 6 min no `TranslateClientService`
- [x] 6. resx — chave `TituloTraducaoCancelada` em pt/en/fr/es
- [x] 7. Verificação — build da solução (0 erros) + 163/163 testes + notas em `AGENTS.md` e `ai-fallback-guidelines.md`

## Notas

- Não se reutiliza `AIProcessingCancelled` ("Pedido à IA cancelado **ou expirado**") porque é ambíguo
  para um cancelamento do utilizador e é também usada para o progresso do servidor.
- A mensagem de cancelamento vive no slot do `Confidence` (à direita), não no lugar do botão: o
  botão Traduzir nunca desaparece e `Confidence` é `null` quando se cancela, logo não colidem.
  A componente de UI ficou com dois estados (a traduzir / não a traduzir) em vez de três.
- Largura do diálogo inalterada (900px): ≈110px de botão + ≈195px de mensagem cabem nos ≈395px da
  coluna `col-md-6`, com folga para as 4 traduções da chave. `white-space: nowrap` garante uma
  linha só.
- `Confirmar` permanece desativado após cancelar (não há texto traduzido).
- `ProgressMessage` é limpo ao cancelar, para não ficar "A aguardar resposta da IA…" ao lado de
  "Cancelado pelo utilizador".
- O estado "cancelado" é reposto no próximo `Translate()` (basta insistir no botão), no fecho do
  diálogo (`OnExit` / `Visible` a false) e no dispose. **Decisão de âmbito:** NÃO é reposto ao editar
  o texto de entrada (exigiria um `oninput` no `SfTextBox` para um ganho marginal).
- O `finally` de `Translate()` foi endurecido: `LeaveCorrelationAsync` e o `StateHasChanged` final
  podem correr depois de o diálogo já ter sido fechado (cancelar + fechar é rápido), por isso
  ambos toleram `ObjectDisposedException`/falha do progress hub.
- `TranslateText` é usado em 8 páginas (todas passam `TranslateFunc`, logo não precisam de alteração):
  `CategoriasMedia`, `CategoriasPublicacoes`, `EditAuthor`, `Formatos`, `FormatosMedia`,
  `LocaisArquivo`, `EditMedia`, `EditPublicacoes`.
- Fora de âmbito: `EditAuthor.razor.cs:77 TranslateBio()` (hook de teste morto, sem botão associado).

## Caminho do cancelamento (verificado ponta-a-ponta)

`CancelTranslate()` -> aborta a `HttpClient` da Blazor -> o ASP.NET Core entrega o
`RequestAborted` ao controller -> `CloudTranslateService.ThrowIfCancellationRequested()` /
`SendAsync(..., ct)` e `OllamaBaseClient` (CTS ligado, distingue timeout de cancelamento) param o
trabalho real.
