# PLAN_DOCKER.md — Corrigir a stack Docker local (webapi + blazor + sql + ollama)

> Documento de trabalho local. **Não commitar.** Apagar quando o trabalho estiver concluído e commitado.

## Contexto / diagnóstico

`docker-compose up --build` (executado em `docker/`) falhou primeiro por o daemon estar parado.
Com o daemon a funcionar, a stack **arrancaria mas não funcionaria** por 5 problemas:

1. `SQLCONNECTIONSTRING` não é lida pela app — o código usa `ConnectionStrings:SqlServerCon`
   (`WebApi/Program.cs:37`, `Web.Blazor/Program.cs:58`). Os contentores caem no
   `appsettings.Production.json` (`DESKTOP-SLIMNUD\SQLEXPRESS` + `Trusted_Connection`), inválido em Linux.
2. `OLLAMA__BASEURL` também não é lida — o HttpClient está hardcoded para `http://localhost:11434`
   (`WebApi/Program.cs:81`, `Web.Blazor/Program.cs:145`).
3. `BaseUrl=http://webapi:5000/api` no compose: o código acrescenta `/api`
   (`Web.Blazor/Program.cs:189` + todos os typed clients), logo gerava `/api/api`.
4. BD vazia: o contentor `mssql` não tem `Publicacoes_Prod` nem schema.
5. Portas `1433:1433` e `11434:11434` colidem com o SQL Server e o Ollama do host (ambos a escutar).

Além disso: contexto de build de 1,3 GB (`Web.Blazor/wwwroot/Files`) sem `.dockerignore`, e o
volume nomeado montado em `/app/wwwroot/Files` esconderia os assets versionados (ícones, bandeiras).

## Plano de execução

- [x] 1. Criar `.dockerignore` (bin/obj/.git/wwwroot/Files/Publish) e `docker/nuget.config` (só nuget.org).
- [x] 2. Reescrever `docker-compose.yml`: env vars corretas, `BaseUrl` sem `/api`, healthchecks,
      `db-init`, sem publicar 1433/11434, bind mount de `wwwroot/Files`.
- [x] 3. Criar `docker/init-db.sh`: restaura o `.bak` mais recente com `MOVE`, normaliza
      `ApplicationSettings` (paths `\`→`/`, `ModelBaseUrl`, `ApiBaseUrl_Production`, FileManager) e
      é idempotente.
- [x] 4. Atualizar `docker/Dockerfile.WebApi` e `Dockerfile.WebBlazor` (env corretas, restore com
      `docker/nuget.config`, publish framework-dependent).
- [x] 5. Tornar a base URL do Ollama configurável (`Ollama:BaseUrl`, default `http://localhost:11434`)
      em `WebApi/Program.cs` e `Web.Blazor/Program.cs`.
- [x] 6. Atualizar `AGENTS.md` (secção Docker) — regra de documentação viva.
- [x] 7a. Corrigir `Web.Blazor.ReportObjects/Web.Blazor.Reporting.csproj`: a `ProjectReference` usava
      `..\..\..\MediaOrganizer\MediaOrganizerApp\Application\...`, que só resolve quando o repo está
      exatamente em `C:\NewProjects\MediaOrganizer\MediaOrganizerApp`. Em `/src` (Docker) apontava para
      `/MediaOrganizer/...` e o build do Blazor falhava. Passou a `..\Application\...`.
- [x] 7. Validar (`docker compose config -q`) e correr a stack (`docker compose up -d --build`),
      confirmar restauro da BD e arranque dos 5 serviços.
      - [x] 7b. Montar `Backup/Database` também no serviço `sql` (o RESTORE corre no processo do
            SQL Server, não no `db-init`) e corrigir o `.env` (`SQL_SA_PASSWORD` terminava em `;`,
            gerando `Password=...;;` e login `sa` falhado). Volumes recriados.
      - [x] 7c. Verificação final: 5 serviços Up/Exited(0), `Publicacoes_Prod` com 41 tabelas,
            `GET /api/AppSettings/settings` e `/api/Dashboard/health` → 200, Blazor `/` → 200.
- [x] 8. Documentar a limitação `System.Drawing.Common` (Windows-only) no `AGENTS.md`.

## Notas

- Backup usado no restore: `Web.Blazor/wwwroot/Files/Backup/Database/28102026_123256.Bak`
  (override por `SQL_BACKUP_FILE`). Restaurar num volume novo; se o volume `sql-data` persistir,
  o restore é ignorado (idempotência).
- Ollama no contentor não tem modelos: `docker exec -it <ollama> ollama pull qwen3:1.7b`.
- O `wwwroot/Files` do host é montado nos dois serviços; os uploads escrevem no working tree
  (comportamento atual do repo, que versiona essa pasta).
