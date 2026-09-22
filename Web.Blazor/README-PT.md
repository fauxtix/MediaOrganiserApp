# 📚 Aplicação Media Organizer

**Sistema de Gestão de Coleções Multi-Utilizador**

[Versão em Inglês](README.md) | [Diagramas de Arquitetura](../docs/architecture/USER_ROLE_DATA_FLOW.md)

Já alguma vez sentiu frustração ao gerir os seus ficheiros multimédia, com centenas de ficheiros espalhados por diferentes discos ou dispositivos de armazenamento? Não seria ótimo tê-los organizados, facilitando a sua gestão?

A Aplicação Media Organizer é uma **aplicação multi-utilizador** para organizar os seus ficheiros multimédia (vídeos e ficheiros MP3) e documentos (PDFs e documentos Word).  
Também permite organizar coleções de livros e documentos escritos em papel (armários de arquivo, por exemplo).

Esta aplicação foi concebida para simplificar a organização, gestão e recuperação de dados multimédia e de publicações com **controlo de acesso baseado em funções**, **isolamento de dados por utilizador**, e um backend robusto.

**Perfeito para:** Coleções pessoais, famílias, pequenas bibliotecas, projetos de arquivo, equipas de colecionadores, ou qualquer cenário onde múltiplos utilizadores precisam de manter catálogos separados num sistema partilhado.

> 📊 **[Ver Diagramas Interativos de Arquitetura](../docs/architecture/USER_ROLE_DATA_FLOW.md)** - Veja como funciona o isolamento de dados e o controlo de acesso baseado em funções

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
- **Cloud-first com fallback local**: as funcionalidades de IA chamam primeiro o **OpenRouter (cloud)** e fazem fallback para um modelo **Ollama local** quando a cloud está indisponível (sem API key, offline, erros 429/HTTP). O perfil de autor pode também usar um cliente OpenAI-SDK apontado para o modelo local configurado.
- **Recuperação de metadados com IA**: Obtém perfis de autor, livros do autor, resumos e metadados quando APIs externas falham.
- **Cloud Chat & Conversas**: Chat com streaming, conversas agrupadas, copiar para o clipboard e avaliação 👍/👎.
- **Gestão de prompts**: Página para editar prompts usados pelo fallback.

### 📊 Analítica & Relatórios
- Estatísticas de publicações/media e export para PDF/Excel.

### 🔐 Sistema Multi-Utilizador & Isolamento de Dados
- **Arquitetura Multi-Utilizador**: Suporte para múltiplos utilizadores com contas individuais e coleções separadas.
- **Acesso a Dados Baseado em Funções**: 
  - **Função Admin**: Acesso total ao sistema — visualizar e gerir dados de TODOS os utilizadores, configurações do sistema, logs e backups.
  - **Função GeneralUser**: Acesso APENAS aos seus próprios dados — publicações, media e playlists filtrados automaticamente por UserID.
- **Gestão de Utilizadores**: Administradores podem criar, editar e eliminar contas na página "Gerir Utilizadores".
- **Isolamento de Dados**: Dados de cada utilizador armazenados com UserID; consultas filtram automaticamente para mostrar apenas itens do utilizador autenticado (exceto Admins).
- **Autenticação**: Login seguro usando ASP.NET Core Identity.
- **Casos de Uso**: Famílias, pequenas bibliotecas/arquivos, equipas de colecionadores, uso pessoal com supervisão admin.

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

- **Frontend**: Blazor Server
- **Backend**: ASP.NET Core Web API
- **Base de Dados**: SQL Server
- **Comunicação em Tempo Real**: SignalR
- **Integração com IA**: OpenRouter (cloud-first) com fallback local Ollama; Cloud Chat e registo de uso de IA
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
   git clone https://github.com/fauxtix/MediaOrganiserApp.git
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

### **Funções de Utilizador**
A aplicação utiliza **Controlo de Acesso Baseado em Funções (RBAC)** com **isolamento automático de dados**:

| Função         | Acesso a Dados                                                              | Acesso ao Sistema |
|-----------------|-----------------------------------------------------------------------------|-------------------|
| **Admin**       | **Vê TODOS os dados** de todos os utilizadores                                | Acesso total: gestão utilizadores, configurações, logs, backups |
| **GeneralUser** | **Vê APENAS os seus dados** (filtrado automaticamente por UserID)           | Funcionalidades padrão: CRUD nos seus próprios dados |

**Isolamento de Dados:**  
O repositório filtra automaticamente as consultas:
- **GeneralUser**: `WHERE UserID = utilizador_atual` (vê apenas os seus dados)
- **Admin**: Sem filtro UserID (vê todos os dados)

Os dados são isolados ao **nível do repositório**, garantindo segurança real (não apenas na UI).

---

## ⚙️ Admin Options

### **1. App Settings**
Administração das definições da aplicação incluindo API keys, storage e políticas de autenticação.

---

Se quiser que eu adicione links diretos para os componentes (`AuthorReadOnly`, `AuthorBooks`, `EditPublicacoes`) ou ajustar a tradução, diga-me.
