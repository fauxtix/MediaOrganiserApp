# 📚 Media Organizer App

[Versão em Português](README-PT.md)

Já sentiu frustração ao gerir media espalhados por vários discos e locais? O Media Organizer App facilita a organização, gestão e pesquisa de media e publicações com uma interface amigável e um backend robusto.

O aplicativo organiza ficheiros multimédia (vídeos, áudio) e documentos (PDF, Word) e também permite gerir coleções de livros e documentos físicos.

---

## ✨ Funcionalidades principais

### 📚 Gestão de Publicações
- **Operações CRUD**: Criar, ler, atualizar e apagar publicações com metadados detalhados.
- **Importação em massa**: Upload de publicações via PDF, Excel ou Word com mapeamento automático/manual, validação e deteção de duplicados.
- **Pesquisa e filtro avançados**: Pesquise por título, autor, categoria, ano ou palavras-chave.
- **Deteção de duplicados**: Identificação automática de publicações duplicadas.
- **Relatórios**: Gerar relatórios e estatísticas por autor, categoria ou ano.
- **Pesquisa Google Books**: Integração com a API Google Books para obter metadados de livros.
- **Visualização de livros do autor**: Ver os livros de um autor diretamente do editor de publicações e da página de autor (Google Books + fallback IA).

### 🖼️ Gestão de Media
- **Uploads com drag & drop**: Suporte a imagens, PDFs, vídeos e áudio com indicador de progresso.
- **Pré-visualizações**: Visualização inline para tipos suportados.
- **Vincular media a publicações**: Associe ficheiros a publicações.
- **Categorização e tags**: Organize por categorias e etiquetas.
- **Playlists**: Crie e edite playlists (áudio e vídeo).
- **Integração YouTube**: Adicione registos do YouTube diretamente.

### 📄 Gestão de Documentos
- **Editor DOCX no browser**: Edição de `.docx` com `SfDocumentEditor`.
- **Impressão**: Imprimir diretamente do editor.

### 🔄 Atualizações em tempo real
- **SignalR**: Progresso em tempo real para operações longas (importação, recuperação de metadados).

### 🤖 Fallback de IA
- **Recuperação de metadados com IA**: Usa Ollama/local LLM para gerar perfis de autor e metadados quando APIs externas falham.
- **Gestão de prompts**: Interface para editar prompts que controlam o comportamento do fallback de IA.

### 📊 Analítica & Relatórios
- Estatísticas de publicações e utilização de media; export para PDF/Excel.

### 🔐 Autenticação e permissões
- **Role-based access control**: Admin, Editor, Viewer.
- **Perfis de utilizador** e logs de atividade.

---

## 📂 Visão geral das páginas

| Página | Descrição |
|---|---|
| Home | Dashboard com métricas e atalhos |
| Publications | Gestor de publicações (CRUD, detalhes) |
| GoogleBooksApiSearch | Pesquisar metadados via Google Books |
| Media | Upload, pré-visualização e gestão de media |
| Video Gallery | Navegar e gerir vídeos |
| Playlists | Criar e gerir playlists |
| Backup Database | Backup da base de dados |
| Backup Publications | Backup de uploads de publicações |
| Backup Media | Backup de uploads de media |
| Backup Images | Backup de imagens |
| Manage Authors | Gestão de autores |
| Manage Users | Gestão de utilizadores e roles |
| Lookup Tables | Gestão de tabelas auxiliares (autores, categorias, editoras) |
| Application Settings | Configurações da aplicação |
| File Manager | Gestão de ficheiros e diretórios |
| View Logs | Visualização de logs |
| Edit Prompts | Editar prompts de IA |
| Reports | Gerar relatórios |
| Data Integrity | Ferramentas para encontrar publicações com ficheiros em falta e armazenar PDFs/PNGs marcados como não eliminados |

---

## ⚙️ Tecnologias usadas

- **Frontend**: Blazor Server
- **Backend**: ASP.NET Core Web API
- **Database**: SQL Server
- **ORM**: Dapper
- **Realtime**: SignalR
- **AI**: Ollama (local LLM) para fallback de metadados
- **Auth**: ASP.NET Core Identity
- **Logging**: Serilog
- **Outras**: FluentValidation, AutoMapper

---

## 🛠️ Instalação

### Pré-requisitos
1. .NET SDK 10.0
2. SQL Server com a base `Publicacoes_Prod`
3. IIS (opcional)
4. Node.js (se usar libs JS)

### Setup
1. Clone o repositório:
```
   git clone https://github.com/fauxtix/MediaOrganiserApp.git
```
2. Entre na pasta da solução:
```
   cd MediaOrganizerApp
```
3. Restaurar dependências:
```
   dotnet restore
```
4. Atualizar `appsettings.json` com a connection string.
5. Executar migrations:
```
   dotnet ef database update
```
6. Executar a aplicação:
```
   dotnet run
```

---

## 📖 API & integrações

Principais endpoints:

- `/api/publications` (Publicações)
- `/api/media` (Media)
- `/api/users` (Utilizadores)

Integrações:

- Armazenamento: Azure Blob (opcional) ou sistema de ficheiros local
- AI metadata fallback: Ollama/local LLM

---

## 🔐 Roles & autorização

- Admin: Acesso total (configurações, backups, logs)
- Editor: Gerir publicações e media
- Viewer: Acesso apenas leitura

---

## 🧩 Notas operacionais

- Ao eliminar uma publicação, a API pode limpar o autor correspondente se não houver outras publicações desse autor.
- Existem endpoints e ferramentas para verificar integridade de ficheiros e para armazenar PDFs/PNGs marcados como "não eliminados".

---

## 📜 Licença

MIT

---

Se precisar que eu sincronize a versão Web.Blazor do README ou adicione links para componentes específicos, diga-me.

