# PLAN_Docker_Deploy.md

Status: **Confirmado pelo utilizador** — criar documento de planeamento formal para deploy Docker (não Windows).

Objetivo: tornar a aplicação acessível sem Windows, substituindo IIS/self-hosted `win-x64` por containers Linux.

## Contexto atual (verificado)
- `README.md` e `README-PT.md` atualizados (`c24c84a`).
- `AGENTS.md` atualizado (`ec639c6`).
- `Web.Blazor/docs/Help.fr.md` traduzido completo (`c24c84a`).
- Deploy atual: `.github/workflows/main.yml` publica `self-contained` (`win-x64`, `PublishReadyToRun=false`), `hostingModel="inprocess"`, `processPath=".\Web.Blazor.exe"`, pools SQL `IIS APPPOOL\...`, certificado `Cert:\LocalMachine\My`, `ASPNETCORE_ENVIRONMENT=Production` no app pool.
- `Ollama` hardcoded `http://localhost:11434` (`Program.cs`).
- `wwwroot\Files` misto: assets da app + dados do utilizador (~740 MB uploads + `PdfsAndCovers.zip` 493 MB).

## Passos e checklist

- [x] Confirmar requisitos (utilizador pediu: criar plano formal + manter pipeline IIS)
- [x] Confirmar que `.github/workflows/main.yml` NÃO é alterado (pipeline IIS mantido)
- [ ] Criar `Dockerfile` para `WebApi` (Linux)
- [ ] Criar `Dockerfile` para `Web.Blazor` (Linux)
- [ ] Adaptar `appsettings.json` / connection string (remover `Trusted_Connection=True` ou usar autenticação SQL para SQL Server container)
- [ ] Criar `docker-compose.yml` (WebApi + Blazor + SQL Server + Ollama opcional)
- [ ] Montar volume para `wwwroot/Files`
- [ ] Adaptar `web.config` / `Program.cs`: remover dependências IIS (`hostingModel`, `inprocess`), garantir Kestrel direto
- [ ] Testar localmente (`docker-compose up`)
- [ ] Atualizar `.github/workflows/main.yml` (publicar container em vez de IIS deploy, ou criar workflow paralelo)
- [ ] Documentar no `README.md` e `README-PT.md`
- [ ] Verificar `AGENTS.md`
- [ ] Confirmar com utilizador (commit final)

## Observações de risco (do AGENTS.md / código)
- `Trusted_Connection=True` + login SQL `IIS APPPOOL\...` não funciona em Linux. Resolver com `User ID`/`Password` ou `SA`.
- Certificado auto-assinado: em Linux, usar `ASPNETCORE_Kestrel__Certificates__Default__Path` e `Password` em vez de `Cert:\LocalMachine\My`.
- `Ollama` local: precisa ser acessível via rede Docker (não `localhost` dentro do container).
- `wwwroot\Files`: volume persistente obrigatório; `robocopy /MIR` não aplica — usar cópia manual ou volume montado.
- `Web.Blazor.Reporting` / RDLC: `ReportViewerCore.NETCore` funciona no Linux? Verificar.
- `publicacoes-prod` DB: `autoCreateSqlTable: true` (Serilog) precisa funcionar no container SQL.

## Próximo passo
Aguardar instrução do utilizador para avançar com a implementação (Dockerfiles) ou apenas documentar.
