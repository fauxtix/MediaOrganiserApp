# PLAN_TRANSLATE_MODEL

Mostrar, junto da confiança, **que provider e que modelo** responderam a uma tradução, e
**qual o modelo que estava a ser tentado** no momento de um cancelamento. Gravar o modelo em
`AIUsageLog.Modelo` (coluna que existe e nunca era escrita).

Porquê: o sweep de até 12 candidatos (`CloudTranslateService.BuildCandidateModelsAsync`) e o
fallback silencioso para o Ollama tornam o modelo imprevisível por construção. Hoje não há
qualquer forma de o saber — nem no UI, nem na BD.

Transportes (complementares, respondem a perguntas diferentes):
- **DTO** (estado final): "quem respondeu?" — autoritativo, sobrevive a falhas de SignalR.
- **Progresso** (modelo em voo): "quem estava a ser tentado agora?" — único canal possível
  quando o cancelamento aborta a request e o servidor nunca devolve resposta.

## Checklist

### DTOs
- [x] `ProgressUpdateDto` — `+ Provider`, `+ Model` (`string?`)
- [x] `TranslateResponseDto` — `+ Provider`, `+ Model`

### Servidor
- [x] `CloudTranslateService` — injeta `IProgressPublisher`; publica `cloud_try` por candidato
      e `cloud_success` no vencedor (`Provider`/`Model` + `Message` = modelo)
- [x] `CloudTranslateService.TryRawCompletionAsync` — devolve também o modelo **servido**
      (`ExtractServedModel` do body) e carimba o DTO vencedor
- [x] `OllamaTranslateService` — resolve o modelo uma vez (`GetAiSettingsAsync`, `protected`),
      propaga `Provider`/`Model` no helper `PublishAsync` e carimba os 3 DTOs
- [x] `AiController.RecordUsageAsync` — parâmetro `modelo`; `Translate`/`TranslateCloud` passam
      `resp?.Model`. `AiUsageRecorderService` trunca a 128 chars (coluna `NVARCHAR(128)`)
- [x] Bónus descoberto: o diálogo **nunca mostrava que estava a tentar o OpenRouter** —
      `CloudTranslateService` não publicava progresso nenhum. O handler de `cloud_try` e as
      chaves `AIProcessingCloud*` já existiam no cliente, sem nunca dispararem.

### Blazor
- [x] `TranslateText.razor.cs` — `Result*` (DTO) e `Attempted*` (progresso) + `TrackModelInFlight`,
      `ClearModelInfo`, `ProviderLabel`/`ProviderIcon`/`ModelLabel`/`ModelTooltip`;
      reset no início de `Translate()` e no fecho/dispose
- [x] `TranslateText.razor` — chip **por baixo do botão Traduzir** (o slot direito fica só com
      a nota de cancelamento ou o `Confidence` — os ids longos sobrepunham texto): cancelado →
      modelo tentado; sucesso → modelo servido; senão nada. Enquanto traduz, nada
- [x] CSS `.translate-model-chip` / `.translate-model-label` / `.translate-model-name`
      (tokens `--app-info-*`, truncado com `min-width: 0`, `margin-top` sob o botão)
- [x] resx pt/en/fr/es — `TraducaoModelo`, `TraducaoModeloTentado` (reutiliza
      `CloudChatAutomaticModel` para o router)

### Docs e verificação
- [x] `AGENTS.md` + `ai-fallback-guidelines.md` no mesmo passe
- [x] `dotnet build` da solução → 0 erros; `dotnet test` → **170/170** (163 + 7 novos)
- [x] Testes: modelo servido do body, fallback para o candidato pedido, `provider`/`model`
      falsificados pelo modelo (ambos os providers), `cloud_try`/`cloud_success`/`cloud_failed`
      publicados por candidato

## Fora de âmbito
- `AuthorBooks` (já mostra modelo; mantém o hack `Message = "{model}|{count}"`)
- Os 8 call sites de `TranslateFunc` (não mudam — consomem o mesmo DTO)
- `AIUsageLog.Modelo` nas outras 6 operações (fill-only para `translate`)
