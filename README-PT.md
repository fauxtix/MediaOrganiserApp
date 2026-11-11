**Sistema de Gestão de Coleções Multi-Utilizador**

<img width="800" height="600" alt="MediaOrganiser_pt" src="https://github.com/user-attachments/assets/04856c43-4502-484b-9d76-26674b7d5f7c" />
---

[Versão em Inglês](README.md) 

Já alguma vez sentiu frustração ao gerir centenas de ficheiros multimédia espalhados por diferentes discos e dispositivos de armazenamento? Não seria ótimo tê-los organizados e fáceis de gerir?

A Aplicação Media Organizer é uma **aplicação multi-utilizador** para organizar os seus ficheiros multimédia (vídeos e ficheiros MP3) e documentos (PDFs e documentos Word).
Também permite organizar coleções de livros, LPs, CDs, DVDs e documentos escritos em papel (armários de arquivo, por exemplo).

Esta aplicação foi concebida para simplificar a organização, gestão e recuperação de dados multimédia e de publicações com **controlo de acesso baseado em funções**, **isolamento de dados por utilizador** e um backend robusto.

**Perfeito para:** coleções pessoais, famílias, pequenas bibliotecas, projetos de arquivo, equipas de colecionadores ou qualquer cenário em que vários utilizadores precisem de manter catálogos separados num sistema partilhado.

> 📊 **[Ver Diagramas Interativos de Arquitetura](docs/architecture/USER_ROLE_DATA_FLOW.md)** — veja como funcionam o isolamento de dados e o controlo de acesso baseado em funções.

---

## ✨ Funcionalidades Principais

### 📚 Gestão de Publicações
- **Operações CRUD**: criar, consultar, atualizar e eliminar publicações com metadados detalhados.
- **Importação/upload em massa**:
  - Carregar publicações em massa através de ficheiros PDF, Excel ou Word.
  - Mapeamento automático e manual dos dados.
  - Validação de campos obrigatórios, deteção de duplicados e relatórios de erros.
- **Pesquisa e filtro**: pesquisa avançada por título, autor, categoria ou palavras-chave.
- **Deteção de duplicados**: sinalização automática de publicações duplicadas com base nos metadados.
- **Relatórios**: geração de relatórios detalhados de publicações, incluindo estatísticas por categoria, autor ou ano (PDF/Excel).
- **Pesquisa na Google Books API**: pesquisa integrada de metadados de livros.
- **Vista de livros do autor**: ver os livros de um autor diretamente do editor de publicações e da página de detalhes do autor (Google Books, Wikidata e fallback de IA quando necessário).
  - A partir do editor de publicações: abra uma publicação em `Publications` e clique no botão de pesquisa do autor junto ao campo do autor para abrir o diálogo; este mostra os detalhes do autor e uma lista "Author Books" com miniaturas, títulos e ligações.
  - A partir do editor de autores: abra `Manage Authors`, edite um autor e clique no botão "Author Books" para ver a mesma lista num diálogo.
- **Ferramentas de integridade de dados**: detetar publicações cujos ficheiros digitais faltam no disco (`PubsNotFoundOnDisk`), publicações com caminhos errados (`PubsWithWrongPath`) e guardar PDFs/PNGs marcados como "não eliminados".

### ✍️ Autores e Gestão de Autores
- **Manage Authors**: área dedicada para criar, editar e remover registos de autores. Cada autor guarda dados básicos de perfil (nome, biografia, ligações externas, fotografia) e ligações a publicações.
- **Author Books**: diálogo que combina resultados do Google Books, Wikidata e fallback de IA para anexar rapidamente metadados e capas corretas às publicações.
- **Limpeza de autores**: ao eliminar uma publicação, a aplicação pode remover automaticamente o autor se este não tiver outras publicações — configurável em App Settings.
- **Exportação PDF**: geração de uma lista PDF de autores a partir da página de autores.
- **Porque é importante**: registos de autores centralizados mantêm os metadados das publicações consistentes, aceleram a introdução de dados e permitem relatórios e filtros por autor em toda a aplicação.

### 🖼️ Gestão de Média
- **Upload de ficheiros**: arrastar e largar imagens, PDFs, vídeos e ficheiros de áudio.
  - **Progresso**: barra de progresso em tempo real para os uploads.
- **Pré-visualização**: pré-visualizações inline e leitores para os formatos suportados.
- **Categorização e etiquetas**: organizar ficheiros por categorias e tags.
- **Playlists**: criar, gerir e editar playlists de áudio/vídeo.
  - **Detalhes da playlist**: nomes, descrições, média associada e um leitor dedicado.
- **Galerias**: galeria de vídeo, galeria de música e galeria de imagens.
- **Títulos dinâmicos**: as páginas de média incluem títulos dinâmicos com badges e subtítulos.
- **Integração com YouTube**: adicionar registos/subscrições do YouTube e obter metadados de vídeos diretamente da área de média.
- **Recuperação de metadados com IA**: o botão **Get Info** obtém metadados (notas, metadados, ano de edição) através de IA.
  - **Atualizações de estado**: progresso e alertas durante a recuperação de metadados.

### 📄 Gestão de Documentos
- **Edição de documentos**: editar documentos Word (`.docx`) diretamente no browser com o `SfDocumentEditor`.
- **Impressão**: imprimir documentos diretamente do editor.
- **Visualizador de documentos**: componente dedicado com suporte a impressão/exportação.
- **Estilos personalizados**: estilos melhorados para as opções do documento.

### 🔄 Atualizações em Tempo Real
- **Integração SignalR**: atualizações de progresso em tempo real para operações longas, como importações em massa, recuperação de metadados e backups.

### 🤖 Integração de IA (cloud-first com fallback local)
- **Cloud-first com fallback local**: as funcionalidades de IA chamam primeiro o **OpenRouter (cloud)** e fazem fallback para um modelo **Ollama local** quando a cloud está indisponível (sem API key, offline, erros 429/HTTP). O perfil de autor também pode usar um cliente OpenAI-SDK apontado para o modelo local configurado.
- **Operações abrangidas**: tradução, notas de média, metadados de média, ano de edição, livros do autor, resumo de livro e perfil de autor. Cada uma tem um endpoint cloud (`*-cloud`) e um endpoint local Ollama.
- **Recuperação de metadados**: obtenção automática de metadados (perfis de autor, livros do autor, resumos de livros, notas/metadados de média, ano de edição) com IA quando as chamadas a APIs externas falham.
- **Modelos gratuitos vs pagos**: o toggle `AllowPaidModels` é aplicado no servidor. O uso de modelos gratuitos é contado e limitado por dia; quando o limite é atingido, os serviços cloud devolvem null para que o chamador faça fallback para o Ollama.
- **Indicação de provider/modelo**: o diálogo de tradução mostra qual o provider/modelo que respondeu (por exemplo `⊘ OpenRouter · llama-3.3-70b`), incluindo o modelo servido quando é usado o router `openrouter/free`.
- **Cancelamento**: as traduções podem ser canceladas na UI sem esperar pelo provider.
- **Prompts personalizáveis**: os templates de prompt estão em `MediaOrganizerApp.WebApi/Prompts/*.prompt.txt` e são editáveis em runtime na página de administração **Edit Prompts**.
- **Registo de uso**: cada chamada de IA fora do chat é registada na tabela `AIUsageLog` e apresentada no dashboard ("IA em uso interno").

### 💬 Chat de IA e Conversas
- **Cloud Chat** (`/ai/openrouter-chat`): chat para o utilizador suportado pelo OpenRouter, com respostas em streaming.
- **Local Chat** (`/ai/chat`): chat Ollama apenas local (exceção à regra cloud-first).
- **Conversas**: os chats são agrupados em conversas; cada conversa guarda mensagens e contexto e pode ser reaberta para perguntas de seguimento.
- **Avaliação do chat**: as respostas do assistente podem ser copiadas para o clipboard e avaliadas com 👍/👎. As avaliações persistem por conversa em `ChatFeedback`.
- **Eliminação do histórico**: os utilizadores podem eliminar conversas no painel de Histórico; a eliminação é conversa a conversa (o Admin elimina todas).
- **Raciocínio** ("💭 Raciocínio"): quando ativado nas definições comuns de IA, os modelos mostram um bloco de raciocínio recolhível por mensagem; a escolha é respeitada e persistida. Funciona para OpenRouter e Ollama local (incluindo blocos inline `<think>` ao estilo Qwen3).
- **Filtro de artefactos de segurança**: remove das respostas linhas que são apenas classificações de segurança do modelo; quando a resposta inteira é o artefacto, a UI pede ao utilizador para responder "continua".
- **Comportamento de fallback**: a UI prefere APIs externas (Google Books, Wikidata, Last.fm) e só recorre à IA quando as pesquisas externas não devolvem resultados aceitáveis. Isto reduz o uso desnecessário de LLM e melhora a precisão dos metadados.
- **Registo de uso**: as respostas de chat são guardadas em `AIResponses`; o uso de IA fora do chat vai para `AIUsageLog` (ver acima).

### 🗂️ Modelos de IA, Prompts e Uso (admin)
- **Modelos de IA**: tabela de lookup de modelos (`/aimodels`) com suporte para adicionar/editar, usada pelos seletores das definições.
- **Editor de prompts**: os administradores podem editar todos os templates de prompt usados pelos serviços de IA sem fazer deploy de código.
- **Dashboard de uso**: o dashboard mostra "IA em uso interno" com o uso por utilizador/operação a partir de `AIUsageLog`.
- **Benchmark**: as métricas dos pedidos de IA (latência, sucesso, provider) são expostas por `/api/ai-benchmark` e usadas pelo dashboard.

### 🎨 UI, Temas e Localização
- **Tema claro/escuro**: propriedades CSS personalizadas em toda a aplicação (`theme-tokens.css`) com um toggle na barra superior. A barra superior e o menu lateral mantêm-se escuros em ambos os temas, por decisão de design.
- **Localização**: português (predefinido), inglês, francês e espanhol, selecionável em Settings (rodapé do menu lateral).
- **Manual de ajuda integrado**: o componente `HelpDialog` renderiza `Web.Blazor/docs/Help*.md` nos quatro idiomas; abre com o botão `?` na barra superior sem sair da página atual.

### 🔧 Funcionalidades Adicionais de Servidor e Utilitários
- **Verificações de integridade de ficheiros**: APIs e ferramentas para detetar publicações cujos ficheiros digitais faltam no disco e executar operações de verificação ou reparação.
- **Retenção de PDF/PNG**: endpoints para guardar ficheiros marcados como "não eliminados" e preservar uploads importantes.
- **Limpeza em cascata de autores**: ao eliminar uma publicação, o sistema pode remover automaticamente o registo do autor se este não tiver outras publicações.
- **Gestão de prompts de IA**: página de UI para editar e gerir os templates de prompt de IA.
- **Serviços Ollama de autor e livros**: clientes Ollama no servidor geram perfis de autor e ajudam com metadados de livros quando as APIs externas falham.
- **Seletor de pastas**: o Syncfusion File Manager é usado em App Settings para escolher as pastas de armazenamento/backup.
- **Logs da aplicação**: visualizador de logs integrado com filtros de severidade, vista de inícios de sessão e eliminação.

### 📊 Analítica e Relatórios
- **Estatísticas de publicações**: número de publicações por categoria, autor ou ano.
- **Insights de uso de média**: análise do uso de ficheiros de média e identificação de ficheiros não utilizados ou obsoletos.
- **Relatórios personalizáveis**: relatórios PDF renderizados no servidor a partir de templates RDLC (`Reports_RDLC/`) através do `ReportViewerCore.NETCore`; exportação Excel para listas.
- **Dashboard**: cartões de resumo, gráficos circulares e painéis de uso de IA.

### 🔐 Sistema Multi-Utilizador e Isolamento de Dados
- **Arquitetura multi-utilizador**: vários utilizadores com contas individuais e coleções separadas.
- **Acesso a dados baseado em funções**:
  - **Admin**: acesso total ao sistema — ver e gerir os dados de TODOS os utilizadores, definições, logs e backups.
  - **GeneralUser**: acesso APENAS às suas próprias publicações, média e playlists — os dados são filtrados automaticamente por `UserID`.
- **Gestão de utilizadores**: os administradores podem criar, editar e eliminar contas na página "Manage Users".
- **Isolamento de dados**: os dados de cada utilizador são guardados com o seu `UserID`; os repositórios filtram automaticamente as consultas para o utilizador autenticado (exceto Admin). O isolamento é aplicado ao nível do repositório, não apenas na UI.
- **Autenticação**: login seguro com ASP.NET Core Identity, incluindo autenticação de dois fatores opcional (aplicação autenticadora + códigos de recuperação).
- **Casos de uso**: famílias, pequenas bibliotecas/arquivos, equipas de colecionadores e coleções pessoais com supervisão de admin.

---

## 🧩 Estrutura da Solução

| Projeto | Função | TFM |
|---|---|---|
| `MediaOrganizerApp.WebApi` | Backend REST API (controllers, serviços de IA, integração Ollama/OpenRouter, relatórios) | net10.0 |
| `Web.Blazor` | Frontend Blazor Server (UI, componentes Syncfusion, clientes API tipados) | net10.0 |
| `MediaOrganizerApp.Application` | Interfaces, DTOs, validação (FluentValidation), consultas Dapper | net10.0 |
| `MediaOrganizerApp.Domain` | Entidades de domínio | net10.0 |
| `MediaOrganizerApp.Persistence` | Repositórios EF Core + Dapper e acesso a dados | net10.0 |
| `MediaOrganizerApp.Identity` | Configuração do ASP.NET Core Identity | net10.0 |
| `MediaOrganizerApp.EmailService` | Envio de email com FluentEmail | net10.0 |
| `MediaOrganizerApp.Tests` | Testes xUnit + Moq | net10.0 |
| `Web.Blazor.ReportObjects` | Templates e helpers de relatórios RDLC | net10.0 |
| `PromptEditor` | Editor de prompts autónomo em Windows Forms (não faz parte da solução) | net10.0-windows |

**Dois processos independentes**: `MediaOrganizerApp.WebApi` e `Web.Blazor`. A aplicação Blazor chama a API por HTTP usando o `BaseUrl` da sua configuração; a API também tem ligação direta à base de dados.

---

## 📂 Visão Geral das Páginas

| Página | Descrição |
|---|---|
| **Home** | Dashboard com métricas principais, gráficos e atalhos. |
| **Publications** | Gerir publicações, ver detalhes e executar operações CRUD. |
| **GoogleBooksApiSearch** | Pesquisar e obter metadados de livros com a Google Books API. |
| **Media** | Carregar, pré-visualizar e gerir ficheiros de média. |
| **Video Gallery / Music Gallery** | Navegar e gerir ficheiros de vídeo e música. |
| **Playlists** | Criar e gerir playlists e leitores de áudio/vídeo. |
| **Backup Database** | Fazer backup da base de dados SQL Server. |
| **Backup Publications / Media / Images** | Fazer backup das pastas de upload correspondentes. |
| **Manage Authors** | Gerir informação de autores, livros e exportação PDF. |
| **Manage Users** | Gerir contas e funções de utilizador. |
| **Lookup Tables** | Gerir dados auxiliares (autores, categorias, editoras, formatos, locais de arquivo, modelos de IA). |
| **App Settings** | Configurar definições globais da aplicação (versão clássica com abas, `/admin/appsettings`). |
| **App Settings 2** | Configurar definições globais da aplicação (versão refatorada com barra lateral de ícones, `/admin/appsettings2` — ligada no rodapé do menu lateral). |
| **Manage Lookup Tables** | Gerir dados auxiliares (versão clássica com abas). |
| **Manage Lookup Tables New** | Gerir dados auxiliares (versão refatorada com grupos/cartões). |
| **File Manager** | Navegar por pastas e escolher caminhos de armazenamento/backup. |
| **Data Integrity** | Encontrar publicações com ficheiros em falta/errados e preservar PDFs/PNGs. |
| **View Logs** | Ver e filtrar logs da aplicação. |
| **Edit Prompts** | Editar templates de prompt de IA. |
| **AI Models** | Gerir a tabela de lookup de modelos de IA. |
| **Chat (Ollama)** | Chat Ollama apenas local. |
| **Chat (OpenRouter)** | Chat cloud com conversas, avaliação e histórico. |
| **Help** | Abrir o manual de utilizador integrado através do botão `?` da barra superior. |
| **Reports** | Gerar e transferir relatórios de publicações e média. |

---

## 🧭 Navegação / Visão Geral do Menu Lateral

A aplicação usa um menu lateral persistente acessível a partir do layout principal. Os principais grupos e páginas incluem:

- **Backup**
  - Backup Database
  - Backup Publications / Media / Images
- **Manutenção / Admin**
  - Manage Users
  - Manage Lookup Tables
  - File Manager
  - View Logs
- **AI**
  - Edit Prompts
  - Chat (Ollama)
  - Chat (OpenRouter)
- **Livros / Publicações**
  - Manage Publications
  - Image Gallery
  - Authors
- **Média**
  - Manage Media
  - Video Gallery
  - Playlists
- **Rodapé**
  - Settings (`/admin/appsettings2`, inclui idioma e tema)
  - Ajuda (`?` na barra superior)
  - Sair

As etiquetas são localizadas através dos recursos do Blazor (`pt`, `en`, `fr`, `es`).

---

## ⚙️ Tecnologias Utilizadas

- **Frontend**: Blazor Server (componentes Syncfusion Blazor)
- **Backend**: ASP.NET Core Web API
- **Base de dados**: SQL Server
- **ORM**: Dapper (consultas principais) + EF Core (Identity e alguma persistência)
- **Comunicação em tempo real**: SignalR
- **Integração de IA**: OpenRouter (cloud-first) com fallback local Ollama; Cloud Chat com registo de uso de IA
- **Autenticação**: ASP.NET Core Identity
- **Registos**: Serilog (sink SQL Server, tabela `AppLogs`)
- **Relatórios**: RDLC através de `ReportViewerCore.NETCore`
- **Outras ferramentas**: FluentValidation, AutoMapper
- **Contentores opcionais**: Docker Compose (contentores Linux)

---

## 🛠️ Primeiros Passos

### Pré-requisitos
1. **.NET SDK 10.0** instalado.
2. **SQL Server** com a base de dados `Publicacoes_Prod` (ou uma base de dados com o mesmo esquema).
3. **Ollama** (opcional) a correr localmente em `http://localhost:11434` para o fallback de IA local.
4. **IIS** (opcional) para deployment.
5. **Node.js** apenas se usar ferramentas npm (não é necessário para executar a aplicação).

### Clonar, restaurar e compilar
```powershell
git clone https://github.com/fauxtix/MediaOrganiserApp.git
cd MediaOrganizerApp
dotnet restore MediaOrganizerApp.sln
dotnet build MediaOrganizerApp.sln
```

> **Nota NuGet**: o `nuget.config` inclui um feed Syncfusion com uma API key placeholder. Se o restore falhar, configure a sua chave/fonte Syncfusion (ou use os pacotes a partir da cache local).

### Configuração e segredos
A chave principal da connection string é **`ConnectionStrings:SqlServerCon`**. Exemplo:
```json
{
  "ConnectionStrings": {
    "SqlServerCon": "Server=DESKTOP-SLIMNUD\\SQLEXPRESS;Database=Publicacoes_Prod;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
  },
  "Ollama": { "BaseUrl": "http://localhost:11434" }
}
```

Use os user-secrets do .NET para desenvolvimento local:
```powershell
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "ConnectionStrings:SqlServerCon" "Server=.;Database=Publicacoes_Prod;Trusted_Connection=True;"
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "Ollama:BaseUrl" "http://localhost:11434"
```

As chaves dos serviços externos (Last.fm, Google Books, IMDb, YouTube, OpenRouter) são **guardadas na base de dados**, em `ApplicationSettings`, e editadas nas áreas **Serviços / IA** de App Settings. São lidas em runtime; não existe `Keys.cs`.

A Web API está protegida por um middleware `ApiKey`. Defina o valor `ApiKey` em `appsettings.json` (ou nos user-secrets) e envie-o no header `ApiKey` a partir dos clientes.

### Base de dados
- O esquema da aplicação vive em `Publicacoes_Prod`.
- As tabelas/procedimentos de funcionalidades são criados com os scripts em `scripts/` (execute-os na base de dados de destino):
  - `CreateConversationsTable.sql` e `CreateConversationsStoredProcedures.sql`
  - `CreateAIChatSchema.sql`
  - `CreateAIUsageLog.sql`
  - `CreateChatFeedback.sql`
  - `AddGoogleBooksSettings.sql`
  - `AddYouTubeSettings.sql`
- O esquema do ASP.NET Core Identity é gerido por migrações EF Core em `Web.Blazor/Data/Migrations`:
  ```powershell
  dotnet ef database update --project .\Web.Blazor\Web.Blazor.csproj
  ```

### Executar os dois processos
A solução tem **dois processos independentes**:
```powershell
# Terminal 1 — Web API
dotnet run --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj

# Terminal 2 — Blazor Server
dotnet run --project .\Web.Blazor\Web.Blazor.csproj
```
Ou execute `scripts/start-all.ps1` a partir da raiz do repositório.

> **Conflito de portas**: ambos os perfis de arranque usam por omissão `https://localhost:5001` / `http://localhost:5000`. Execute-os com perfis diferentes ou substitua as portas (por exemplo, `dotnet run --urls "https://localhost:5001"` num e `--urls "https://localhost:5002"` no outro) e mantenha o `BaseUrl` do Blazor a apontar para o endereço da API.
>
> **Portas de produção** usadas pelo pipeline IIS: Web API `http://localhost:4100` (loopback), Blazor `https://localhost:4110`.

### Testes
```powershell
dotnet test MediaOrganizerApp.sln
```
Os testes fazem mock das chamadas HTTP (não precisam de Ollama nem de base de dados ao vivo) com xUnit + Moq.

---

## 🔌 API e Integrações

A Web API expõe endpoints REST para as principais áreas:

| Área | Endpoint |
|---|---|
| Publicações | `/api/publications` |
| Média | `/api/media` |
| Autores | `/api/autores` |
| Playlists | `/api/playlists` |
| Utilizadores | `/api/users` |
| Definições da aplicação | `/api/appsettings` |
| Dashboard / estatísticas | `/api/dashboard`, `/api/stats` |
| Operações de IA | `/api/ai` (tradução, notas/metadados de média, livros/perfil de autor, resumo de livro) |
| Cloud chat (streaming) | `/api/cloud-chat` |
| Conversas | `/api/conversations` |
| Avaliação do chat | `/api/chat-feedback` |
| Uso/benchmark de IA | `/api/ai-usage`, `/api/ai-benchmark` |
| Relatórios | `/api/report` |
| Google Books / Wikidata / Wikipedia | `/api/googlebooks`, `/api/wikidata`, `/api/wikipedia` |
| Operações PDF | `/api/pdf` |

Os pedidos à Web API têm de incluir o header `ApiKey` (validado pelo middleware de API key). A UI Blazor autentica os utilizadores com ASP.NET Core Identity e reencaminha a API key configurada.

### Integrações
- **Armazenamento local de ficheiros**: os uploads e ficheiros de publicações são guardados em `Web.Blazor/wwwroot/Files` (os caminhos `Pasta*` em `ApplicationSettings` podem apontar para outro local).
- **Metadados externos**: Google Books, Wikidata, Wikipedia, Last.fm e YouTube.
- **Providers de IA**: OpenRouter (cloud) e Ollama (fallback local), configurados a partir das definições guardadas na base de dados.

### Exemplos de smoke test
```bash
curl -X GET "https://localhost:5001/api/publications" \
  -H "ApiKey: <API_KEY>"

curl -X POST "https://localhost:5001/api/media/upload" \
  -H "ApiKey: <API_KEY>" \
  -F "file=@/path/to/file.mp3" \
  -F "title=My Track"
```

---

## 🔐 Funções de Utilizador e Isolamento de Dados

### Funções de utilizador
| Função | Acesso a dados | Acesso ao sistema |
|---|---|---|
| **Admin** | **Vê os dados de TODOS os utilizadores** — publicações, média e playlists de todos. | Acesso total: gestão de utilizadores, definições da aplicação, logs, backups e prompts. |
| **GeneralUser** | **Vê APENAS os seus dados** — publicações, média e playlists filtrados por `UserID`. | Funcionalidades padrão: criar/editar/eliminar os seus próprios dados. Não acede às páginas de admin. |

### Como funciona o isolamento de dados
Cada publicação e registo de média é etiquetado com um `UserID`. A camada de repositório filtra as consultas com base na função do utilizador autenticado:

```csharp
if (userRole == UserRole.GeneralUser)
{
    // Apenas as publicações deste utilizador
    sbData.Append(" WHERE IdFormato = @IdFormato AND UserID = @UserID");
}
else // Admin
{
    // Publicações de todos os utilizadores
    sbData.Append(" WHERE IdFormato = @IdFormato ");
}
```

**Pontos-chave**
- **GeneralUser** vê um catálogo personalizado apenas com os seus itens.
- **Admin** tem supervisão e pode ver/gerir dados de todos os utilizadores.
- O isolamento de dados é aplicado ao nível do **repositório**, não apenas na UI.

> 📊 Ver diagramas detalhados e exemplos de código na [Documentação de Arquitetura](docs/architecture/USER_ROLE_DATA_FLOW.md).

---

## ⚙️ Opções de Administração

### 1. App Settings
O App Settings (`/admin/appsettings` e `/admin/appsettings2`) está organizado em quatro áreas:

#### **Pastas**
- Pastas de downloads (PDF e média).
- Pastas de backup (base de dados e uploads).
- Pastas de publicações, imagens, listagens, capas de autores e imagem de fundo.
- Seletores de pasta suportados pelo File Manager.

#### **Outras Definições 1**
- Contactos (telemóvel do autor, email de suporte).
- Tempo de notificação.
- Flag de ficheiros organizados e "remover imagem após edição".
- Idioma predefinido.

#### **Outras Definições 2**
- URLs base da API (produção/desenvolvimento) para a Web API e File Manager.
- Credenciais de Last.fm, Google Books, IMDb e YouTube.
- Connection string da base de dados.

#### **IA (área única de IA)**
- **Definições comuns**: toggle de raciocínio (partilhado por OpenRouter e Ollama).
- **OpenRouter**: API key, seletor de modelo, toggle de modelos pagos, toggle de livros do autor na cloud, limite diário gratuito e cartões de uso.
- **Ollama (local)**: API key, URL base e nome do modelo.

### 2. Logs
A página **View Logs** mostra os logs da aplicação (Serilog → SQL Server `AppLogs`) com:
- Filtros por severidade (informação, aviso, erro) e eventos de início de sessão.
- Vista de tabela consolidada.
- Detalhes e eliminação de entradas.

### 3. Backups
Os backups são efetuados a partir das páginas de Backup e usam as pastas configuradas em App Settings:
- **Base de dados**: backup SQL Server de `Publicacoes_Prod`.
- **Uploads**: publicações, média e imagens.
- O restauro é feito ao nível da base de dados/ficheiros com os ficheiros de backup gerados (não existe um agendador de backups).

### 4. Tabelas de Apoio
A área de tabelas de apoio gere dados partilhados como autores, categorias, editoras, formatos de publicações/média, locais de arquivo e modelos de IA.

---

## 🐳 Docker (opcional, contentores Linux)

O `docker-compose.yml` arranca `sql` + `db-init` + `ollama` + `webapi` + `webblazor`. É uma **stack local opcional** e **não** substitui o deploy IIS de produção.

```bash
# a partir da raiz do repositório (não de dentro de docker/)
docker compose up --build
```

- Portas publicadas: **5000** (Web API) e **5001** (Blazor). `sql` e `ollama` são apenas internos (o host já usa 1433/11434); acede-se com `docker exec`.
- O `db-init` espera pelo SQL Server, restaura o `.bak` de `Web.Blazor/wwwroot/Files/Backup/Database` e normaliza `ApplicationSettings` para os contentores (`\` → `/`, `ModelBaseUrl=http://ollama:11434`, URLs da API a apontar para `webapi:5000`). É idempotente.
- O `.env` define `SQL_SA_PASSWORD` (a password **não** deve terminar em `;` — é o separador da connection string). Depois de a alterar, recrie o volume: `docker compose down -v && docker compose up -d`.
- O Ollama arranca sem modelos. Descarregue um quando necessário: `docker exec -it <contentor-ollama> ollama pull qwen3:1.7b`.
- **Limitação conhecida**: o `System.Drawing.Common` é exclusivo do Windows em .NET 6+. Caminhos de redimensionamento de imagens e de geração de alguns PDFs lançam `PlatformNotSupportedException` em contentores Linux. Navegação, CRUD e a maioria das funcionalidades de IA funcionam.

Nota de memória: o SQL Server 2022 precisa de cerca de 2 GB. Se o contentor `sql` sair com código 137, aumente a memória do WSL2/Docker Desktop antes de `docker compose up`.

---

## 🚀 Deploy com IIS

### Pipeline automatizado (GitHub Actions)
O `.github/workflows/main.yml` publica os dois projetos **self-contained** (`win-x64`, `PublishReadyToRun=false`) e faz deploy em IIS na mesma máquina:

- **Web API**: HTTP `http://localhost:4100` (loopback, usado pelo Blazor no mesmo host).
- **Blazor**: HTTPS `https://localhost:4110` com certificado auto-assinado.
- **Sites/app pools**: `MediaOrganiserApi` → `C:\inetpub\MediaOrganizerAPI`; `OrganiseIt` → `C:\inetpub\OrganiseIt`.
- `ASPNETCORE_ENVIRONMENT=Production` é definido explicitamente nos dois app pools.
- As identidades dos app pools têm de coincidir com os logins SQL: `IIS APPPOOL\MediaOrganiserApi` e `IIS APPPOOL\OrganiseIt` (ambos `db_owner` em `Publicacoes_Prod`). Com `Trusted_Connection=True`, renomear um pool sem criar o login SQL correspondente remove o acesso à base de dados.
- As ACLs concedem `Modify` nas duas pastas de deploy às identidades dos pools (necessário para uploads).
- **O HSTS está desligado em produção** enquanto o certificado é auto-assinado (`Hsts:Enabled = false`). Ativar apenas depois de passar para um certificado de confiança.
- O pipeline termina com um smoke test: `GET /api/dashboard/health` com o header `ApiKey` e `GET https://localhost:4110/` com validação de certificado desligada; em caso de falha faz dump de diagnósticos de IIS/eventos e falha o job.
- O runner tem de estar **elevado** (IIS e `Cert:\LocalMachine\My`) e a máquina precisa do **ASP.NET Core Hosting Bundle** (o publish usa `AspNetCoreModuleV2`, in-process).

### Publish manual
1. No Visual Studio, publique **MediaOrganizerApp.WebApi** e **Web.Blazor** para pastas (por exemplo `C:\Publish\WebApi` e `C:\Publish\Web.Blazor`).
2. No IIS Manager, crie um site para a API (porta `4100` ou a sua escolha) e outro para o Blazor (porta `4110`/HTTPS).
3. Configure os app pools como **No Managed Code** e `ASPNETCORE_ENVIRONMENT=Production`.
4. Conceda `Modify` às identidades dos pools nas pastas de deploy.
5. Configure o binding HTTPS com um certificado de confiança (ou auto-assinado para uso local) e registe-o com `netsh http add sslcert` / na UI de bindings do IIS.

### Logins SQL Server
Com autenticação Windows, a identidade do processo é `IIS APPPOOL\<nome do pool>`:
```sql
CREATE LOGIN [IIS APPPOOL\MediaOrganiserApi] FROM WINDOWS;
CREATE LOGIN [IIS APPPOOL\OrganiseIt] FROM WINDOWS;

USE Publicacoes_Prod;
CREATE USER [IIS APPPOOL\MediaOrganiserApi] FOR LOGIN [IIS APPPOOL\MediaOrganiserApi];
CREATE USER [IIS APPPOOL\OrganiseIt] FOR LOGIN [IIS APPPOOL\OrganiseIt];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\MediaOrganiserApi];
ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\OrganiseIt];
```

---

## 🧯 Resolução de Problemas (Deploy IIS)

### HTTP Error 500.19 (`0x8007000d` ou `0x80070002`)
Este erro ocorre quando o IIS não consegue ler a secção `<aspNetCore>` do `web.config` porque o **ASP.NET Core Module V2** não foi registado na configuração global do servidor (`applicationHost.config`) — mesmo depois de instalar o *.NET Hosting Bundle* (muitas vezes por bloqueio do antivírus, falta de privilégios de administrador ou ordem de instalação).

> **Nota:** se executar a aplicação localmente via Visual Studio ou `dotnet run` (Kestrel/IIS Express), pode ignorar esta secção.

#### Como corrigir
1. **Reparar o Hosting Bundle**
   - Feche o *IIS Manager*.
   - Execute o instalador do **.NET 10 Hosting Bundle** como **Administrador** e escolha **Repair**.
   - Reinicie o IIS a partir de um terminal elevado: `iisreset`.

2. **Registo manual (se o Repair falhar)**
   Se o instalador estiver bloqueado por políticas de segurança ou antivírus e não conseguir atualizar o IIS, execute o seguinte em **PowerShell como Administrador** para registar a secção e o módulo (script de referência — reveja antes de aplicar):

   ```powershell
   $configPath = "$env:windir\System32\inetsrv\config\applicationHost.config"
   $content = [System.IO.File]::ReadAllText($configPath)

   # 1. Declarar a secção aspNetCore (corrige 0x8007000d)
   if ($content -notlike '*name="aspNetCore"*') {
       $content = $content -replace '(<sectionGroup name="system.webServer">)', "`$1`r`n            <section name=`"aspNetCore`" overrideModeDefault=`"Allow`" />"
   }

   # 2. Remover entradas antigas do módulo e injetar o módulo global (corrige 0x80070002)
   $content = $content -replace '<add name="AspNetCoreModuleV2"[^>]*>', ''
   $moduleLine = '<add name="AspNetCoreModuleV2" image="%ProgramFiles%\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll" />'
   $content = $content -replace '(<globalModules>)', "`$1`r`n        $moduleLine"
   $content = $content -replace '(<modules[^>]*>)', "`$1`r`n        <add name=`"AspNetCoreModuleV2`" />"

   [System.IO.File]::WriteAllText($configPath, $content)
   iisreset
   ```

3. **Permissões de pasta**
   Garanta que a identidade do app pool (`IIS APPPOOL\<NomeDoPool>`) tem **Read & Execute** (ou **Modify**, se a aplicação escrever uploads/backups) na pasta de deploy.

### HTTP Error 500.30 (*ASP.NET Core app failed to start*)

A aplicação Blazor configura o **Serilog com um sink MSSQL Server durante o arranque** (`WriteTo: MSSqlServer` com `autoCreateSqlTable: true`), pelo que **não consegue arrancar enquanto o SQL Server estiver inacessível**. Quando um reinício do SQL Server coincide com o deploy — uma **atualização do Windows** para o SQL Server (cumulativa/GDR), um reinício manual ou um failover — o worker arranca, rebenta e deixa o site preso:

- o processo `w3wp` arranca, lança `Microsoft.Data.SqlClient.SqlException` (`Win32Exception (258)` — timeout TCP) e **fica vivo com o CLR descarregado**;
- todos os pedidos seguintes devolvem **`500.30` de imediato, sem eventos novos no Event Log** — o crash fica registado apenas uma vez (`1007` *failed to load coreclr*, `1018` *hit unexpected managed exception*);
- a API pode continuar a responder normalmente, porque arrancou antes do reinício e reutiliza ligações em pool.

Diagnóstico (PowerShell):

```powershell
# worker de cada site e quando foi criado
Get-CimInstance Win32_Process -Filter "Name='w3wp.exe'" | Select-Object ProcessId, CreationDate

# motivo do crash (um evento por crash, não por pedido)
Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='IIS AspNetCore Module V2'; StartTime=(Get-Date).Date} |
    Where-Object Id -in 1007, 1018, 1026 | Select-Object TimeCreated, Id, Message
```

Um worker criado perto da hora do crash e sem qualquer `1032 started successfully` depois disso é um **zombie**: a aplicação nunca mais arranca dentro desse processo, ou seja o `500.30` **não é um erro de configuração**.

#### Como corrigir
1. Confirme que o motor do SQL Server está em execução e que a base de dados de deploy responde:
   ```powershell
   Get-Service 'MSSQL$SQLEXPRESS'
   # ou, em SQL: SELECT CONVERT(varchar(30), sqlserver_start_time, 121) FROM sys.dm_os_sys_info
   ```
2. Recicle o app pool a partir de um **PowerShell elevado** — o IIS cria um worker novo e a aplicação arranca no pedido seguinte:
   ```powershell
   Import-Module WebAdministration
   Restart-WebAppPool -Name 'OrganiseIt'        # Blazor
   Restart-WebAppPool -Name 'MediaOrganiserApi' # API
   ```
3. Volte a testar o site. Um arranque saudável escreve `1032 Application '...' started successfully` no log *Application*:
   ```powershell
   curl.exe -k -sS -o NUL -w "%{http_code}`n" https://localhost:4110/
   ```
4. Volte a correr o pipeline. O smoke test valida a API **e** a base de dados (`/api/dashboard/health`), por isso um deploy que colidiu com a indisponibilidade do SQL fica vermelho até ambos estarem bons.

> **Dica:** agende as atualizações do SQL Server fora das janelas de deploy, ou volte a correr o pipeline depois de a atualização terminar. Este `500.30` é precisamente o que o smoke test existe para apanhar — o deploy só fica verde quando a API devolve `200` **e** o Blazor devolve `2xx/3xx`.

---

## 🤝 Como Contribuir

1. Faça fork do repositório.
2. Crie um branch: `git checkout -b feature/your-feature-name`.
3. Faça as alterações e execute `dotnet build` e `dotnet test`.
4. Faça commit: `git commit -m "Add your message here"`.
5. Faça push: `git push origin feature/your-feature-name`.
6. Abra um pull request.

---

## 📜 Licença

Este projeto está licenciado sob a **MIT License**. Consulte o ficheiro [LICENSE](LICENSE.txt) para detalhes.

---

## 📞 Contacto e Suporte

- **Email**: support@mediaorganizerapp.com
- **Issues no GitHub**: [Submeter uma Issue](https://github.com/fauxtix/MediaOrganiserApp/issues)

---

## 🖼️ Capturas de Ecrã

_Adicione capturas de ecrã aqui para mostrar a aplicação._
