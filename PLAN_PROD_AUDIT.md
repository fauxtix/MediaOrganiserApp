# PLAN — Auditoria de produção e redesenho do workflow de deploy

> Documento de trabalho local. **Não fazer commit.** Eliminar só quando o trabalho estiver concluído e commitado.

## Contexto

Auditoria de URLs/configuração que podem falhar em produção, seguida do redesenho de
`.github/workflows/main.yml` para deploy em IIS local (self-hosted Windows).

Ambiente confirmado por inspecção:

- Máquina `DESKTOP-SLIMNUD`, SQL Server Express **instância `SQLEXPRESS`** a correr, `TcpPort=1433` estática.
  `Server=DESKTOP-SLIMNUD\SQLEXPRESS` e `Server=DESKTOP-SLIMNUD,1433` resolvem **para a mesma instância**.
  Base `Publicacoes_Prod` existe e responde com Windows auth (`Trusted_Connection=True`).
- Topologia alvo: WebApi **HTTP** em `http://localhost:4100`; Blazor **HTTPS** em `https://localhost:4110`.
- Logins SQL **já existem** e são `db_owner`: `IIS APPPOOL\MediaOrganiserApi` e `IIS APPPOOL\OrganiseIt`.
  ⚠️ O yml actual criaria pools `MediaOrganiserApiPool`/`OrganiseItPool`, que **não têm login** → falha no arranque.
- Key do OpenRouter presente em `dbo.AIChatSettings` (`CloudChat:OpenRouterApiKey`, 73 chars) → IA em nuvem funcional.
- Publicação **self-contained** (o deploy antigo tinha `Web.Blazor.exe` + `runtimes/` de 617 MB).
- Pastas `C:\inetpub\OrganiseIt` e `C:\inetpub\MediaOrganizerAPI` são de tentativas antigas → **limpar e recomeçar**.
  `C:\inetpub\OrganiseIt\wwwroot\Files\Uploads` tem 183 MB / 64 ficheiros — feito backup antes de limpar.

## Decisões do utilizador

- Publicar self-contained.
- Limpar as pastas (Blazor e Api) e recomeçar.
- Publicar o yml actualizado/refeito.

## Fase 0 — Arranque

- [x] Auditar configs, connection strings, registos de DI e rotas
- [x] Confirmar instância SQL e acessibilidade da base
- [x] Confirmar logins SQL dos app pools
- [x] Confirmar presença da key do OpenRouter
- [x] Fazer backup de `wwwroot\Files` (183 MB)
- [x] `EditMedia.razor.cs` — escolher `ApiBaseUrl_Production` em produção
- [x] `MediaThumbailsService.cs` — corrigir `/dashboard` → `/api/media`
- [x] `PdfGeneratorService.cs` — corrigir `/authors` → `/api/authors`
- [x] `PubsWithWrongPath.razor` — `OrganizeIt` → `OrganiseIt`
- [x] `ASPNETCORE_ENVIRONMENT=Production` explícito nos pools

## Fase 1 — Endurecimento

- [x] `DetailedErrors` só em Development
- [x] `BaseUrl` validado em **todos** os ambientes
- [x] WebApi: `UseExceptionHandler` + `AddProblemDetails` em produção
- [x] WebApi: `UseHttpsRedirection` só em Development (a API é HTTP-only)
- [x] `TastyDbConnection`: `;` em falta corrigido
- [x] `robocopy` com exclusão de `wwwroot\Files`
- [x] Binding HTTPS 4110 com certificado auto-assinado
- [x] Smoke test sobre HTTPS (ignorar validação do certificado)

## Fase 2 — Workflow

- [x] Pools renomeados para `MediaOrganiserApi` / `OrganiseIt` (alinhados com os logins SQL)
- [x] Publicação self-contained
- [x] Limpeza das pastas de destino
- [x] Reescrever `.github/workflows/main.yml`

## Fase 3 — Verificação

- [x] `dotnet build` da solução
- [x] `dotnet test`
- [x] Actualizar AGENTS.md na mesma passagem

## Fora do âmbito (decidido NÃO alterar, para não partir nada)

| Item | Razão |
|---|---|
| `[Authorize]` no `FileManagerController` | A app **não tem `[Authorize]` em lado nenhum**. Introduzir autorizacao num so controller tornaria o modelo de acesso inconsistente e bloquearia utilizadores. Exige decisao previa sobre o modelo de autenticacao. |
| Password do LastFM em `MediaController.cs:320` | E' a unica copia da credencial. Remove-la sem destino seguro faz o LastFM falhar em producao. Exige primeiro decidir onde guardar. |
| `ConnectionManager` reler so `appsettings.json` | Nao esta a falhar (as duas connection strings sao equivalentes). E' um `static` com blast radius largo. Refactor de risco para um ganho teorico. |
| Rotar a `ApiKey` em plaintext no git | Exige coordenacao (git + BD + env). Sem destino de armazenamento decided, qualquer rotacao parte a app. A sobreposicao por variavel de ambiente ja funciona sem codigo. |
| `db_owner` para os app pools | Sobre-privilegiado, mas a app precisa de escrita (inclui `AppLogs` e auto-criacao de tabelas do Serilog). Reduzir pode partir o arranque. |
