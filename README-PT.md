# 📚 Aplicação Media Organizer

[Versão em Inglês](README.md)

Já alguma vez sentiu frustração ao gerir os seus ficheiros multimédia, com centenas de ficheiros espalhados por diferentes discos ou dispositivos de armazenamento? Não seria ótimo tê-los organizados, facilitando a sua gestão?

A Aplicação Media Organizer é uma aplicação para organizar os seus ficheiros multimédia (vídeos e ficheiros MP3) e documentos (PDFs e documentos Word).  
Também permite organizar coleções de livros, LPs, CDs, DVDs e documentos escritos em papel (armários de arquivo, por exemplo).

Esta aplicação foi concebida para simplificar a organização, gestão e recuperação de dados multimédia e de publicações com uma interface amigável e um backend robusto.

---

## ✨ Funcionalidades Principais

### 📚 Gestão de Publicações
- **Operações CRUD**: Criar, consultar, atualizar e eliminar publicações com metadados detalhados.
- **Importação/Upload em Massa**:
  - Carregue publicações em massa utilizando PDF, Excel ou Word.
  - Mapeamento automático e manual para integração.
  - Validação de campos obrigatórios, deteção de duplicados e relatórios de erros.
- **Pesquisa & Filtro**: Pesquisa avançada por título, autor, categoria ou palavras-chave.
- **Deteção de Duplicados**: Sinalização automática de publicações duplicadas com alertas para o utilizador.
- **Relatórios**: Gerar relatórios detalhados por categoria, autor ou ano (PDF/Excel).
- **Integração Google Books**: Pesquisa integrada para metadados de livros com interface dedicada no frontend.
- **Visualização "Author Books"**: Visualize os livros associados a um autor diretamente do editor de publicações ou da página de autor. A listagem combina resultados do Google Books com fallback de IA quando aplicável.

### ✍️ Gestão de Autores (visão para utilizadores)
- **Gerir Autores**: Área dedicada para criar, editar e remover registos de autores. Cada autor contém dados básicos (nome, biografia, links externos) e relações com publicações.
- **Author Books**: Ao abrir o diálogo "Author Books" pode navegar por livros do autor com thumbnails, datas de publicação e ligação direta ao Google Books.
- **Limpeza de Autores**: Opção para remover autores sem publicações catalogadas; operação disponível no UI com confirmação e notificações.
- **Exportar Autores**: Geração de PDF com a lista de autores via PDF generator integrado.

### 🖼️ Gestão de Média
- **Uploads**: Drag-and-drop para imagens, PDFs, vídeos e ficheiros de áudio com barra de progresso.
- **Pré-visualização**: Visualizações inline e players para formatos suportados.
- **Categorização & Etiquetas**: Organize ficheiros por categorias e tags para pesquisa e filtragem rápida.
- **Playlists**: Criação e gestão de playlists (áudio/vídeo) com metadados e ordenação.
- **Integração YouTube**: Adicione registos do YouTube diretamente na gestão de media.
- **Recuperação de Metadados com IA**: Botão "Obter Informações" que invoca serviços de IA (fallback) para preencher metadados e sugerir capas/descrições.

### 📄 Gestão de Documentos
- **Edição de .docx no browser**: Integração com SfDocumentEditor para editar documentos diretamente na aplicação.
- **Visualizador e Impressão**: Visualizador dedicado com opções de impressão/export.

### 🔄 Atualizações em Tempo Real
- **SignalR**: Progresso em tempo real para operações longas (importações em massa, extração de metadados, backups).

### 🤖 IA, Chat e Fallback
- **Cloud-first com fallback local**: as funcionalidades de IA chamam primeiro o **OpenRouter (cloud)** e fazem fallback para um modelo **Ollama local** quando a cloud está indisponível (sem API key, offline, erros 429/HTTP). O perfil de autor pode também usar um cliente OpenAI-SDK apontado para o modelo local configurado.
- **Recuperação de Metadados**: Busca automática de metadados (perfis de autor, livros do autor, resumos, notas/metadados de mídia, ano de edição) usando IA quando APIs externas falham.
- **Prompts Personalizáveis**: Consultas dinâmicas; os templates de prompt são editáveis na página "Edit Prompts" do admin.
- **Manual Integrado**: O componente `HelpDialog` renderiza o manual (`Web.Blazor/docs/Help*.md`) em 4 idiomas, aberto pelo botão `?` na barra superior — sem sair da página.
- **Histórico e Avaliação**: Chats com histórico por conversa, avaliação 👍/👎 persistida, cópia de resposta, exclusão de histórico (conversa a conversa; Admin remove todas).
- **Recuperação de metadados com IA**: A aplicação tenta primeiro APIs externas (ex.: Google Books) e, caso não obtenha resultados suficientes, utiliza IA para obter perfis de autor, livros do autor, resumos e metadados.
- **Página de Chat / Conversas**: Interface para conversas com a IA (Cloud Chat) para consultas ad-hoc — p. ex. "obter perfil de autor" — com suporte a conversas encadeadas e streaming de respostas. As respostas podem ser avaliadas 👍/👎 e copiadas.
- **Editar Prompts**: Administradores podem editar templates de prompts através da interface "Edit Prompts" sem necessidade de deploys.
- **Registo de uso de IA**: Cada operação de IA fora de chat é registada na tabela `AIUsageLog` e apresentada no dashboard ("IA em uso interno").

### 🔧 Funcionalidades do Servidor e Utilitários
- **Verificações de integridade de ficheiros**: Ferramentas para detetar publicações com ficheiros ausentes e executar operações de reparação.
- **Retenção PDF/PNG**: Helpers para marcar ficheiros como ‘não eliminados’ e preservar uploads importantes.
- **Limpeza em Cascata de Autores**: Ao apagar uma publicação, a app pode remover o autor se este não tiver mais publicações.
- **Gestão de Prompts**: UI para editar prompts usados pelos serviços de IA.
- **File Manager**: API e UI para navegar, carregar e gerir ficheiros no servidor.

### 📊 Analítica & Relatórios
- **Estatísticas de Publicações**: Nº de publicações por categoria, autor, ano.
- **Insights de Uso de Média**: Identificar ficheiros não utilizados ou obsoletos.
- **Relatórios Personalizáveis**: Exportar relatórios em PDF/Excel.

### 🔐 Gestão de Utilizadores & Autorização
- **Controlo de Acesso baseado em Funções (RBAC)**: Perfis Admin, Editor e Viewer.
- **Perfis de Utilizador**: Gerir detalhes pessoais e preferências.
- **Logs de Atividade**: Auditoria de ações através de Serilog.
- **Autenticação**: ASP.NET Core Identity com suporte a MFA opcional.

---

## 📂 Visão Geral das Páginas

| Nome da Página                  | Descrição                                                                 |
|---------------------------------|-----------------------------------------------------------------------------|
| **Home**                        | Dashboard com métricas principais e atalhos.                              |
| **Publications**                | Gestão de publicações: listar, editar, importar, e exportar.              |
| **GoogleBooksApiSearch**        | Pesquisar livros e importar metadados do Google Books.                    |
| **Media**                       | Upload, pré-visualização e gestão de ficheiros multimédia.                |
| **Video Gallery**               | Galeria dedicada para vídeos.                                             |
| **Playlists**                   | Criar e gerir playlists de áudio e vídeo.                                 |
| **Backup Database**             | Backup da base de dados.                                                  |
| **Backup Publications**         | Backup dos ficheiros de publicações.                                      |
| **Backup Media**                | Backup dos ficheiros de média.                                            |
| **Backup Images**               | Backup das imagens.                                                       |
| **Manage Authors**              | Gestão completa de autores e ligação com publicações.                     |
| **Manage Users**                | Gestão de utilizadores e permissões.                                      |
| **Lookup Tables**               | Gestão de tabelas auxiliares (categorias, editores, etc.).                |
| **Application Settings**        | Configurações globais da aplicação (APIs, storage, flags).                |
| **File Manager**                | Navegar e gerir ficheiros no servidor.                                    |
| **View Logs**                   | Visualizar logs de aplicação para debugging.                              |
| **Edit Prompts**                | Editor de templates de prompts para IA.                                   |
| **Reports**                     | Gerar e transferir relatórios.                                            |
| **Data Integrity**              | Ferramentas para localizar publicações com ficheiros faltantes.           |

---

## ⚙️ Tecnologias Utilizadas

- **Frontend**: Blazor Server (Web.Blazor)
- **Backend**: ASP.NET Core Web API (MediaOrganizerApp.WebApi)
- **Base de Dados**: SQL Server
- **ORM**: Dapper
- **Comunicação em Tempo Real**: SignalR
- **Integração com IA**: OpenRouter (cloud-first) com fallback local Ollama; Cloud Chat e registo de uso de IA
- **Autenticação**: ASP.NET Core Identity
- **Registos**: Serilog
- **Outras Ferramentas**: Syncfusion (SfDocumentEditor, Grids), FluentValidation, AutoMapper

---

## 🛠️ Instalação (Desenvolvedor) — focado em .NET 10 / Blazor

### Pré-requisitos
1. **.NET SDK 10.0** instalado.
2. **SQL Server** (local ou remoto) com uma base de dados disponível (exemplo: `Publicacoes_Prod`).
3. **Ollama** (se usar fallback IA) executando localmente em `http://localhost:11434`.
4. **IIS** (opcional para deployment) e **Node.js** apenas se usar ferramentas npm.

### Setup rápido
```powershell
git clone https://github.com/fauxtix/MediaOrganiserApp.git
cd C:\NewProjects\MediaOrganizer\MediaOrganizerApp
dotnet restore
```

### Configurar segredos (exemplos)
```powershell
# Web API project
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "ConnectionStrings:DefaultConnection" "Server=.;Database=Publicacoes_Prod;Trusted_Connection=True;"
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "Ollama:BaseUrl" "http://localhost:11434"
dotnet user-secrets --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj set "OpenAI:ApiKey" "<YOUR_OPENAI_KEY>"

# Blazor project (opcional)
dotnet user-secrets --project .\Web.Blazor\Web.Blazor.csproj set "OpenAI:ApiKey" "<YOUR_OPENAI_KEY>"
```

### Executar migrações
```powershell
dotnet ef database update --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj --startup-project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj
```

### Iniciar API e Blazor (dois terminais)
```powershell
# Terminal 1: API
dotnet run --project .\MediaOrganizerApp.WebApi\MediaOrganizerApp.WebApi.csproj

# Terminal 2: Blazor Server
dotnet run --project .\Web.Blazor\Web.Blazor.csproj
```

Ou abra a solução no Visual Studio e configure múltiplos projetos de arranque (MediaOrganizerApp.WebApi e Web.Blazor).

---

## 🔗 Integrações & Endpoints Principais

- **Publicações**: `GET/POST/PUT/DELETE /api/publications`
- **Media**: `GET/POST/PUT/DELETE /api/media` (uploads multipart em `/api/media/upload`)
- **Autores**: endpoints de autor expostos via `AutorService` client no Blazor (veja `IAutorService` e `AutoresController` no WebApi)
- **AI**: `POST /api/ai` (AiController) — endpoints de IA genéricos
- **Conversas (Chat)**: `POST /api/ai/conversations` (ConversationsController) — iniciar/continuar conversas e streaming
- **File Manager**: `api/filemanager` (opções para navegar e gerir ficheiros)

Note que alguns controladores usam atributos de rota explícita; confirme no código-fonte os atributos `[Route(...)]` antes de automatizar chamadas.

---

## 💬 Exemplos rápidos (curl) — testes de fumo
Atualize host/port e token conforme a sua configuração.

1) Criar uma publicação
```bash
curl -X POST "https://localhost:5001/api/publications" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Livro Exemplo","authors":[{"name":"Maria Silva"}],"year":2020}'
```

2) Upload de ficheiro multimédia
```bash
curl -X POST "https://localhost:5001/api/media/upload" \
  -H "Authorization: Bearer <TOKEN>" \
  -F "file=@/caminho/para/ficheiro.mp3" \
  -F "title=Minha Faixa"
```

3) Iniciar conversa com AI
```bash
curl -X POST "https://localhost:5001/api/ai/conversations" \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Procurar autor Maria Silva","messages":[{"role":"user","content":"Encontra perfil do autor Maria Silva"}],"language":"pt-PT"}'
```

---

## ⚙️ Configuração & Segredos (dicas)
- Ollama: `http://localhost:11434` (padrão) — defina `Ollama:BaseUrl` nos user-secrets do WebApi.
- OpenAI: `OpenAI:ApiKey` nos user-secrets do WebApi e, se necessário, no Blazor.
- DB: `ConnectionStrings:DefaultConnection`.

---

## 🛠️ Notas de Implementação para quem faz a Manutenção
- Componentes relevantes para autores:
  - `Web.Blazor/Pages/BaseComponents/AutoresBase.razor.cs` — lógica de grid, diálogos, export PDF, filtros e ações (editar, apagar, listar autores sem publicações).
  - `Web.Blazor/Components/Authors/AuthorBooks.razor` — componente que consulta Google Books e faz fallback para IA, inclui paginação, cache e badges de confiança de IA.
- O comportamento de fallback segue esta ordem: Google Books → (429 rate-limit tratado) → IA (Ollama/OpenAI).
- A UI exibe etiquetas de confiança AI quando aplicável e mostra mensagens para rate-limit do Google Books.
- Prompts estão localizados na pasta `MediaOrganizerApp.WebApi/Prompts` e podem ser editados via a página de administração "Edit Prompts".

---

## 🤝 Contribuição
- Fork, branch, desenvolver e abrir PR. Execute `dotnet test` e verifique a construção antes de submeter.

---

## 📝 Licença
Este projeto está licenciado sob a MIT License. Consulte o ficheiro LICENSE para detalhes.

---

