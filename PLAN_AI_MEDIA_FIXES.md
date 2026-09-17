# Plano — Correções do fluxo AI no módulo Media

Data de criação: 2026-09-17
Status: concluído (ver histórico de commits)

## Contexto

Arquitetura Cloud-first (OpenRouter) → fallback Ollama nos 3 fluxos AI do módulo Media:
Notas (`media-notes`), Metadata (`media-metadata`) e Ano de Edição (`media-edition-year`).
O fluxo é funcional, mas foram identificadas 6 correções.

## Itens corrigidos

### 1. `shouldAIBeUsed` (dead code)
- Ficheiro: `Web.Blazor/Pages/Media/EditMedia.razor.cs`
- A variável existia mas nunca era utilizada → a IA era chamada para todos os formatos.
- Ação: envolver os blocos de fallback AI de `GetMediaInfo()` (Notas e AnoEdicao) com `if (shouldAIBeUsed)`.
  Formatos: `1=Download internet`, `2=CD`, `3=LP`, `4=DVD`, `5=AUDIO-DVD`, `9=Local`.
  A guarda cobre a condição `IdFormato_Media > 1 && IdFormato_Media < 6` (CD/LP/DVD/Audio-DVD).
- As APIs externas (OMDb/AlbumInfo) continuam a correr para todos os formatos.

### 2. CancellationToken em falta no fallback Ollama de Notas
- Ficheiro: `Web.Blazor/Pages/Media/EditMedia.razor.cs`
- `GetMediaNotesAsync` (fallback local) não recebia `mediaInfoCts.Token`.
- Ação: passar o token como `correlationId: null, mediaInfoCts.Token`.

### 3. `ReadFromJsonAsync` sem cancellation token (Blazor client)
- Ficheiro: `Web.Blazor/Services/ApiWrappers/AiMediaClientService.cs`
- `GetMediaMetadataAsync` não passava `cancellationToken` à desserialização.
- Ação: adicionar `cancellationToken` ao `ReadFromJsonAsync`.

### 4. Edition Year cloud sem varredura multi-modelo
- Ficheiro: `MediaOrganizerApp.WebApi/Services/CloudChat/CloudMediaService.cs`
- `GetMediaEditionYearAsync` usava modelo único; Notas/Metadata já faziam sweep via
  `ExecuteExtractionAsync`. 
- Ação: novo `ExecuteExtractionRawAsync` (espelho do `ExecuteExtractionAsync` mas com
  `isJsonOnly: false` + normalização por `Func<string?, string?>`), usado pelo edition year.
  O gate `AllowPaidModels` e a varredura de candidatos passam a aplicar-se igualmente.

### 5. Precedência explícita Tempo/Runtime e CoverFile/Poster
- Ficheiro: `Web.Blazor/Pages/Media/EditMedia.razor.cs`
- Havia dupla escrita (Tempo depois Runtime; CoverFile depois Poster).
- Ação: `else if` explícito. Precedência final preservada: `Runtime > Tempo`, `Poster > CoverFile`.

### 6. `return;` prematuro na validação do ano
- Ficheiro: `Web.Blazor/Pages/Media/EditMedia.razor.cs`
- Os casos "N/A" e ano não numérico faziam `return` do método inteiro, impedindo o
  `finally` de mostrar o `aiProviderMessage`.
- Ação: flag `invalidYearReturned` + `else if`; apenas a atribuição de `AnoEdicao` é saltada.

## Fora de âmbito (sem mudança)

- Toggle `AllowPaidModels` já está corretamente ligado:
  `ExecuteExtractionAsync` lê `CloudChat:AllowPaidModels`;
  `BuildCandidateModelsAsync` alterna modelos pagos recomendados vs catálogo live free;
  `ApplyPaidModelGate` degrada modelos pagos para `:free`.

## Verificação

1. `dotnet build MediaOrganizerApp.sln`
2. `dotnet test MediaOrganizerApp.Tests` (inclui `CloudMediaServiceTests`)
3. Nota: enquanto a app estiver a correr, `dotnet build` pode falhar com
   MSB3027/MSB3021 por lock do `Web.Blazor.dll` — esperado, não é erro de código.