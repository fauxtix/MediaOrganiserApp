# PLAN — Hardening do deploy IIS (`main.yml`)

Origem: run de 10/11/2026 12:00 terminou "Succeeded" mas o resultado ficou **inutilizável**:

- `http://localhost:4100/` → **500.19 / 0x8007000d** (config do site não interpretada; o `web.config` em disco está válido).
- `https://localhost:4110/` → **handshake TLS abortado** (HttpEvent 15021); o certificado nunca ficou registado no HTTP.sys
  porque `Set-WebBinding -PropertyName iisCertificateThumbprint` não é uma propriedade do binding.
- Nenhum `w3wp` de `MediaOrganiserApi`/`OrganiseIt` em execução.
- `C:\inetpub\MediaOrganizerAPI` sem ACE para `IIS APPPOOL\MediaOrganiserApi` (o `OrganiseIt` tinha-a adicionada à mão).
- Access logs de IIS desligados desde setembro → 500.x sem rasto.
- Aviso 2303 (W3SVC-WP, `0x80070002`) em 11:40/11:41 = preload automático a falhar; o pipeline não o controla.

## Passos

- [x] 1. Novo passo **Smoke test** depois do deploy: `GET /api/dashboard/health` (com `ApiKey` lida do
      `appsettings.Production.json` publicado) tem de devolver **200** e `https://localhost:4110/` tem de
      devolver **2xx/3xx** sem falha de TLS; retry até 180 s. Em falha: dump do corpo, estados de pools/sites,
      `netsh http show sslcert` e eventos IIS/WAS, e **job falha**.
- [x] 2. **Certificado**: remover `Set-WebBinding ... iisCertificateThumbprint`; registar no HTTP.sys com
      `$binding.AddSslCertificate($thumb,'My')` (após `netsh http delete sslcert` do endpoint) e **verificar**
      com `netsh http show sslcert` (falha o passo se o hash não ficar lá).
- [x] 3. **ACLs**: `icacls <API_PATH|WEB_PATH> /grant "IIS APPPOOL\<pool>:(OI)(CI)M"` (idempotente, sem `/T`).
- [x] 4. **Auto-start/preload determinístico**: `startMode=OnDemand` + `autoStart=true` nos pools e
      `preloadEnabled=false` nas apps (via `appcmd`) → elimina a causa do aviso 2303.
- [x] 5. **Access logs de IIS** ligados nos dois sites (`logFile.enabled=true`), para 500.x futuros terem rasto.
- [x] 6. `environmentVariables` do pool: **preservar as existentes** e só garantir
      `ASPNETCORE_ENVIRONMENT=Production` (hoje o `Set-ItemProperty -Value @{...}` substitui a coleção toda).
- [x] 7. Arranque verificado: `Start-WebAppPool`/`Start-Website` só se não estiverem `Started` + validar o estado.
- [x] 8. Documentar em `AGENTS.md` (secção "Deploy de producao") o que o pipeline passou a garantir.
- [x] 9. Validação estática: YAML parseado (`js-yaml`) e os 8 blocos `run:` passaram o parser da linguagem
      PowerShell (`[Parser]::ParseInput`) sem erros.

## Estado

Implementado e validado estaticamente. Falta **executar o workflow** uma vez para ver o smoke test a passar
(é o único passo que não pode ser validado sem IIS elevado).

## Fora do âmbito (precisa de diagnóstico elevado)

- Causa raiz do **500.19** no host: `iis-diag.ps1` (pools/preload, registo+schema ANCM, config efetiva
  `appcmd`, `netsh http show sslcert`, ACLs, eventos) — o smoke test do passo 1 transforma qualquer recaída
  num **job vermelho com evidência**, em vez de um verde silencioso.

## Diagnóstico do 500.19 (10/11/2026) — evidência recolhida sem elevação

O erro **não é do pipeline nem das ACLs do projeto**:

| Verificação | Resultado |
|---|---|
| `http://localhost:4100/` | 500.19, `0x8007000d`, config `\\?\C:\inetpub\MediaOrganizerAPI\web.config`, **Origem da Configuração `-1:` / `0:` vazia** |
| `http://localhost:4550/` (LLMLocalCloudArena, **outro** site Core) | 500.19 `0x8007000d`, mesma assinatura |
| `http://localhost:4800/` (OllamaArena) | 500.19 `0x8007000d`, mesma assinatura |
| ACL de `C:\inetpub\wwwroot\LLMLocalCloudArena` | **tem** `IIS APPPOOL\LLMLocalCloudArena:(OI)(CI)(M)` → ACL descartada como causa |
| `web.config` publicado (API) | 549 bytes, BOM UTF-8, parse XML **OK**, estrutura igual à do template MS |
| `applicationHost.config` (snapshot `C:\inetpub\history\CFGHISTORY_0000000711`) | site `MediaOrganiserApi` → pool + `physicalPath C:\inetpub\MediaOrganizerAPI` + binding `*:4100:` **corretos** |
| `globalModules` (mesmo snapshot) | `<add name="AspNetCoreModuleV2" image="%ProgramFiles%\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll" />` **presente** |
| `schema\aspnetcore_schema_v2.xml` (snapshot) | presente (2729 bytes) e declara `hostingModel` |
| `aspnetcorev2.dll` | existe (421 KB, 23/10/2026); ANCM V2 `110.0.26296` instalado; runtime `10.0.14` presente |
| portos 4100/4110 | à escuta (HTTP.sys, PID 4); há `w3wp` a correr |
| Eventos | apenas os 2 avisos 2303 (11:40/11:41) e HttpEvent 15021/15300 do 4110; **nenhum** evento de 500.19 |

Conclusão: **falha ao nível do IIS/host, comum a todos os sites ASP.NET Core** — "Invalid data" com
origem de configuração vazia é assinatura de ficheiro de configuração que o processo de trabalho não
consegue ler/interpretar (não de uma linha inválida). Suspeitos, por ordem: (1) ACL/leitura da pasta
`config\schema` para a identidade do worker → `<aspNetCore>` deixa de ser reconhecido; (2) módulo ANCM a
falhar a carga por dependência em falta; (3) interferência do AVG (registado em 09/11 11:48).

- [ ] 10. Correr `C:\Users\User\AppData\Local\Temp\opencode\iis-diag.ps1` **como Administrador** (agora
      inclui `appcmd list config` por secção — que mostra o erro exato do 500.19 —, ACLs de `config`/`schema`,
      parse de `applicationHost.config`, `list module`, dependências da ANCM e probe a todos os sites) e
      corrigir o host. Enquanto isto não estiver resolvido, o smoke test **vai falhar de propósito**.

> Nota: o smoke test assume que a falta de BD deve falhar o deploy (a app sem BD não é utilizável).

---

# Smoke test: callback de certificado inválido (11/11/2026)

Run `43019450448` (12:20) falhou outra vez no smoke test com `API=200  Blazor=0` nas **36** tentativas.
A app está saudável: `curl -k https://localhost:4110/` → **200**, `1032 started successfully` às 12:27:23,
API health **200**, pools/sites `Started`.

A causa é o próprio probe, na linha 545 do `main.yml`:

```powershell
[Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
```

O `ScriptBlock` é invocado na thread do handshake TLS (não é a thread do pipeline), onde **não existe
runspace**; o callback lança e a ligação é fechada **antes de o pedido chegar ao IIS**:

```
Web -> code=0 erro=A ligação subjacente foi fechada: Erro inesperado ao enviar.
     inner: There is no Runspace available to run scripts in this thread. …
            The script block you attempted to invoke was: $true
```

Consequência: o endpoint de Blazor **nunca é pedido** durante o smoke test, o worker on-demand nunca arranca
(ficou a faltar até um `curl` real o acordar) e todas as tentativas dão `0`. `curl` ao mesmo URL passa em
TLS 1.0/1.1/1.2 (TLS 1.3 é recusado, mas irrelevante: o `ServicePointManager` pede 1.2).

## Passos

- [x] 1. Substituir o `ScriptBlock` por um **delegate compilado** (`Add-Type` com
      `ServicePointManager.ServerCertificateValidationCallback = (s, c, ch, e) => true`), que não precisa de
      runspace. Mantém o `Tls12` e todo o resto do probe intacto.
- [x] 2. Manter o `curl.exe -k` como plano B documentado em comentário (se o `Add-Type` falhar no runner).
- [x] 3. Script extraído do YAML executado localmente contra os sites reais: `Tentativa 1: API=200 Blazor=200`,
      exit 0 (YAML parseado com js-yaml, PowerShell com `[Parser]::ParseInput`).
- [x] 4. Workflow corrido: run `43020982420` **verde** em 5m08s — `Tentativa 1: API=200  Blazor=200`,
      `Smoke test OK`.
- [x] 5. Atualizar `AGENTS.md` (secção "Deploy de producao", bullet do smoke test) com a nota do callback.

## Estado

Correção aplicada, validada estaticamente, executada manualmente e confirmada no workflow (`43020982420` verde).
