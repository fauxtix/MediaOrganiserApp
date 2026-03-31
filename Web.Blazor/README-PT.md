# 📚 Aplicação Media Organizer

[Versão em Inglês](README.md)

Já alguma vez sentiu frustração ao gerir os seus ficheiros multimédia, com centenas de ficheiros espalhados por diferentes discos ou dispositivos de armazenamento? Não seria ótimo tê-los organizados, facilitando a sua gestão?

A Aplicação Media Organizer é uma aplicação para organizar os seus ficheiros multimédia (vídeos e ficheiros MP3) e documentos (PDFs e documentos Word).  
Também permite organizar coleções de livros e documentos escritos em papel (armários de arquivo, por exemplo).

Esta aplicação foi concebida para simplificar a organização, gestão e recuperação de dados multimédia e de publicações com uma interface amigável e um backend robusto.

---

## ✨ Funcionalidades Principais

### 📚 Gestão de Publicações
- **Operações CRUD**: Criar, consultar, atualizar e eliminar publicações com metadados detalhados.
- **Importação/Upload em Massa**:
  - Carregue publicações em massa utilizando Pdf, Excel ou Word.
  - Mapeamento automático e manual para integração.
  - Validação, deteção de duplicados e relatórios de erros.
- **Pesquisa & Filtro**: Pesquisa avançada por título, autor, categoria, palavras-chave.
- **Deteção de Duplicados**: Sinalização automática de publicações duplicadas.
- **Relatórios**: Gerar relatórios detalhados por categoria, autor ou ano.
- **Google Books API**: Pesquisa integrada para metadados de livros.
- **Visualização de livros do autor**: Ver os livros de um autor diretamente do editor de publicações e da página de autor (Google Books + fallback IA).

### 🖼️ Gestão de Média
- **Uploads**: Drag-and-drop para imagens, PDFs, vídeos e áudio com progressbar.
- **Pré-visualização**: Inline previews para ficheiros suportados.
- **Ligar Media a Publicações**: Associar ficheiros a publicações.
- **Categorização & Tags**: Organize ficheiros por categorias e etiquetas.
- **Playlists**: Criar e gerir playlists (audio/video).
- **YouTube Integration**: Adicionar registos do YouTube.
- **Recuperação de Metadados com IA**: Botão "Obter Informações" com fallback de IA e atualizações de estado.

### 📄 Gestão de Documentos
- **Editor de .docx**: Edição em browser usando `SfDocumentEditor`.
- **Impressão**: Imprimir diretamente do editor.
- **Visualizador de Documentos**: Componente separado para visualização e impressão.

### 🔄 Atualizações em Tempo Real
- **SignalR** para progresso em operações longas.

### 🤖 Fallback de IA
- **Recuperação de metadados com IA**: Usa Ollama/local LLM para gerar perfis de autor e metadados quando APIs externas falham.
- **Gestão de prompts**: Página para editar prompts usados pelo fallback.

### 📊 Analítica & Relatórios
- Estatísticas de publicações/media e export para PDF/Excel.

### 🔐 Gestão de Utilizadores
- RBAC (Admin, Editor, Viewer), Perfis e Logs.

---

## 📂 Visão Geral das Páginas

| Nome da Página                  | Descrição                                                                 |
|---------------------------------|-----------------------------------------------------------------------------|
| **Home**                        | Dashboard com métricas principais e links rápidos.                        |
| **Publications**                | Manage publications, view details, and perform CRUD operations.           |
| **GoogleBooksApiSearch**        | Search and retrieve book metadata using Google Books API.                 |
| **Media**                       | Upload, preview, and manage media files.                                   |
| **Video Gallery**               | Browse and manage video files.                                             |
| **Playlists**                   | Create and manage playlists for media files.                               |
| **Backup Database**             | Backup the database.                                                       |
| **Backup Publications**         | Backup publication uploads.                                                |
| **Backup Media**                | Backup media uploads.                                                      |
| **Backup Images**               | Backup image uploads.                                                      |
| **Manage Authors**              | Manage author information.                                                 |
| **Manage Users**                | Manage user accounts and roles.                                            |
| **Lookup Tables**               | Manage auxiliary data like authors, categories, and publishers.            |
| **Application Settings**        | Configure application-wide settings.                                       |
| **File Manager**                | Manage files and directories.                                              |
| **View Logs**                   | View application logs for debugging and monitoring.                        |
| **Edit Prompts**                | Edit AI prompts for metadata generation.                                   |
| **Reports**                     | Generate and download reports for publications and media.                  |
| **Data Integrity**              | Tools to find publications with missing files and store PDFs/PNGs flagged as not deleted. |

---

## ⚙️ Tecnologias Utilizadas

- **Frontend**: Blazor WebAssembly
- **Backend**: ASP.NET Core Web API
- **Base de Dados**: SQL Server
- **Comunicação em Tempo Real**: SignalR
- **Integração com IA**: Ollama API para recuperação de metadados (local LLM)
- **Autenticação**: ASP.NET Core Identity
- **Registos**: Serilog
- **Outras Ferramentas**: FluentValidation, AutoMapper

---

## 🛠️ Instalação

### Pré-requisitos
1. **.NET SDK 10.0** instalado.
2. **SQL Server** com a base de dados `Publicacoes_Prod`.
3. **IIS** para implementação (opcional).
4. **Node.js** (se utilizar bibliotecas JavaScript).

### Configuração
1. Clone o repositório:
```
   git clone https://github.com/fauxtix/MediaOrganiserApp.Web.Blazor.git
```

2. Navegue até ao diretório da solução:
```
   cd MediaOrganizerApp
```

3. Restaure as dependências:
```
   dotnet restore
```

4. Atualize o `appsettings.json` com a sua string de conexão.

5. Execute as migrações:
```
   dotnet ef database update
```

6. Inicie a aplicação:
```
   dotnet run
```

---

## 📖 API & Integrações

A solução fornece uma API RESTful para gerir publicações e media. Endpoints principais incluem:

- **Publicações**: `/api/publications`
- **Media**: `/api/media`
- **Utilizadores**: `/api/users`

### Integrações
- **Armazenamento Externo**: Os ficheiros multimédia podem ser armazenados no Azure Blob Storage ou num sistema de ficheiros local.
- **Recuperação de Metadados com IA**: Obter automaticamente metadados usando IA quando as APIs externas falham.

---

## 🔐 User Roles & Authorization

### **User Roles**
- **Admin**: Full access to all features, including user management, app settings, logs, and backups.
- **Editor**: Manage publications and media files.
- **Viewer**: Read-only access to publications and media.

---

## ⚙️ Admin Options

### **1. App Settings**
Administração das definições da aplicação incluindo API keys, storage e políticas de autenticação.

---

Se quiser que eu adicione links diretos para os componentes (`AuthorReadOnly`, `AuthorBooks`, `EditPublicacoes`) ou ajustar a tradução, diga-me.
