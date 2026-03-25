[➡️ English version / Versão em Inglês](README.md)

# MediaOrganizerApp

Alguma vez sentiu frustração ao tentar gerir sessões incontáveis de media espalhadas por diferentes discos ou unidades de armazenamento? Não seria ótimo ter tudo organizado, permitindo uma gestão fácil?

A Media Organiser App é uma aplicação para organizar a sua media (vídeos e ficheiros mp3) e documentos (PDFs e ficheiros Word). Também permite organizar coleções de livros, LPs, CDs, DVDs e documentos em papel (como arquivos e dossiês).

# Demonstração

https://github.com/fauxtix/MediaOrganiserApp.Backend/assets/49880538/7eb85ef6-0a38-4d9e-af2f-1c374fde4801

## Execução do Renderer e Web API (desenvolvimento)

Este projeto inclui dois serviços para a renderização de relatórios RDLC:
- `ReportsRenderer` — serviço de renderização independente (pode gerar RDLC em PDF)
- `MediaOrganizerApp.WebApi` — API principal que pode encaminhar a renderização para o serviço renderer

Abaixo estão os comandos exatos e dicas para desenvolvimento local.

Pré-requisitos
- SDK do .NET instalado (os projetos exigem .NET 7 e .NET 10). Verifique com:
  - `dotnet --info`

Iniciar o serviço de renderização (renderer)
(abra um terminal, mantenha-o em execução):

PowerShell
```
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project ./ReportsRenderer/ReportsRenderer.csproj
```

Bash (Linux / macOS / WSL)
```
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project ./ReportsRenderer/ReportsRenderer.csproj
```

Iniciar a Web API
(abra outro terminal):

PowerShell
```
$env:ASPNETCORE_ENVIRONMENT = 'Development'
# opcional: URL do renderer se não usar launchSettings/appsettings
$env:RDLC_RENDERER_URL = 'http://localhost:5000/'
dotnet run --project ./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj
```

Bash
```
export ASPNETCORE_ENVIRONMENT=Development
export RDLC_RENDERER_URL='http://localhost:5000/'
dotnet run --project ./MediaOrganizerApp.WebApi/MediaOrganizerApp.WebApi.csproj
```

Utilizar o Visual Studio / perfis de lançamento
- O projeto Web API contém `Properties/launchSettings.json` com `ASPNETCORE_ENVIRONMENT=Development` e `RDLC_RENDERER_URL` configurados para o perfil de desenvolvimento. Execute esse perfil na IDE para utilizar as mesmas configurações.

Verificar o fluxo de renderização
- O renderer expõe `POST /api/render` — a Web API encaminha para este endpoint quando `RDLC_RENDERER_URL` está definido. Exemplo: inicie ambos os serviços e chame um endpoint que desencadeie um relatório (como `GET /api/report/publications`).

Resolução de problemas
- NU1101: Unable to find `Microsoft.Reporting.NETCore`
  - Certifique-se que consegue aceder ao `nuget.org`. Execute:
    - `dotnet nuget locals all --clear`
    - `dotnet restore ./ReportsRenderer/ReportsRenderer.csproj -v diag`
  - Se estiver atrás de um feed corporativo, adicione o seu feed ao `NuGet.Config` ou inclua credenciais.
  - Como solução temporária, o repositório inclui `ReportsRenderer/Lib/ReportingStubs.cs` (stubs de compilação). Remova esse ficheiro após restaurar o pacote oficial com sucesso.

- Erros no arranque: RDLC não encontrado
  - Certifique-se que os ficheiros RDLC estão em `ReportsRenderer/Reports_RDLC` ou defina a variável de ambiente `REPORTS_PATH` para a pasta onde estão os `.rdlc`.
  - Exemplo: `export REPORTS_PATH=/caminho/para/Reports_RDLC` (bash) ou ` $env:REPORTS_PATH='C:\caminho\para\Reports_RDLC'` (PowerShell).

- Renderer inacessível / falha ao encaminhar pela API
  - Confirme que o renderer está a correr e disponível no endereço base configurado em `RDLC_RENDERER_URL` (por defeito `http://localhost:5000/`).
  - Use `curl -v http://localhost:5000/api/render` (POST com payload JSON) para testar a conectividade.

- Conflitos de portas
  - Altere o `ASPNETCORE_URLS` ou atualize o `launchSettings.json` para utilizar portas diferentes. Verifique conflitos com `netstat` / `ss`.

- Excepções em tempo de execução
  - Inicie a Web API com `ASPNETCORE_ENVIRONMENT=Development` para ver a página de exceções do programador e para que os logs mostrem as stack traces completas.
  - Consulte os logs da aplicação (Serilog) impressos na consola para erros a montante ao encaminhar para o renderer.

Se surgir um erro que não consegue resolver, cole o output completo da consola ou o comando que falhou (`dotnet restore -v diag`) / stack trace e eu analiso.