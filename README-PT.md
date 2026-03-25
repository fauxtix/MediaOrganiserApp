# 📚 Aplicação Media Organiser

[Versão em Inglês](README.md)

Já alguma vez sentiu frustração ao gerir os seus ficheiros multimédia, com centenas de ficheiros espalhados por diferentes discos ou dispositivos de armazenamento? Não seria ótimo tê-los organizados, facilitando a sua gestão?

A Aplicação Media Organiser é uma aplicação para organizar os seus ficheiros multimédia (vídeos e ficheiros MP3) e documentos (PDFs e documentos Word).  
Também permite organizar coleções de livros, discos de vinil, CDs, DVDs e documentos escritos em papel (armários de arquivo, por exemplo).

Esta aplicação foi concebida para simplificar a organização, gestão e pesquisa de dados multimédia e de publicações com uma interface intuitiva e um backend robusto.

---

## ✨ Funcionalidades Principais

### 📚 Gestão de Publicações
- **Operações CRUD**: Criar, consultar, atualizar e eliminar publicações com metadados detalhados.
- **Importação/Upload em Massa**:
  - Carregue publicações em massa utilizando ficheiros CSV, Excel ou JSON.
  - Mapeamento de dados automático e manual para integração sem falhas.
  - Validação de campos obrigatórios, deteção de duplicados e relatórios de erros.
- **Pesquisa e Filtro**: Pesquisa avançada por título, autor, categoria ou palavras-chave.
- **Deteção de Duplicados**: Sinalização automática de publicações duplicadas com base em metadados.
- **Relatórios**: Gerar relatórios detalhados sobre publicações, incluindo estatísticas por categoria, autor ou ano.
- **Pesquisa na API Google Books**: Pesquisa integrada para metadados de livros usando a API Google Books.

### 🖼️ Gestão de Média
- **Upload de Ficheiros**: Carregamento de ficheiros (arrastar e largar) para imagens, PDFs, vídeos e áudios.
  - **Acompanhamento de Progresso**: Barra de progresso em tempo real para uploads de ficheiros.
- **Pré-visualização de Média**: Pré-visualizações integradas para tipos de ficheiros suportados.
- **Ligar Média a Publicações**: Associar ficheiros multimédia a publicações específicas.
- **Categorização e Etiquetagem**: Organize os ficheiros multimédia por categorias e etiquetas.
- **Listas de Reprodução**: Criar, gerir e editar listas de reprodução para organizar ficheiros multimédia.
  - **Tipos de Média**: Suporte para listas de reprodução de áudio e vídeo.
  - **Detalhes da Lista de Reprodução**: Exibir nomes, descrições e ficheiros associados às listas de reprodução.
- **Títulos de Página Dinâmicos**: As páginas de média agora incluem títulos dinâmicos com badges e subtítulos.
- **Integração com YouTube**: Adicionar registos do YouTube diretamente na interface de gestão de média.
- **Recuperação de Metadados com IA**: Obter metadados para ficheiros multimédia diretamente na interface.
  - **Botão Obter Informações**: Permite aos utilizadores recuperar metadados para um ficheiro selecionado usando prompts de IA.
  - **Atualizações de Estado**: Exibe progresso e alertas durante o processo de recuperação de metadados.

### 📄 Gestão de Documentos
- **Edição de Documentos**: Editar documentos Word (`.docx`) diretamente no navegador usando `SfDocumentEditor`.
- **Impressão**: Imprimir documentos diretamente do editor.
- **Estilos Personalizados**: Estilização melhorada para opções de documentos.

### 🔄 Atualizações em Tempo Real
- **Integração com SignalR**: Atualizações em tempo real para operações demoradas, como importações em massa ou recuperação de metadados.

### 🤖 IA de Apoio
- **Recuperação de Metadados**: Obter automaticamente metadados (ex.: perfis de autores) usando IA quando as chamadas à API externa falham.
- **Integração com Ollama**: Utilizado para gerar perfis de autores e metadados.
  - **Perfis de Autores**: Obter informações detalhadas sobre autores usando prompts de IA.
  - **Prompts Personalizáveis**: Suporte para consultas dinâmicas para resultados personalizados.

### 📊 Analítica e Relatórios
- **Estatísticas de Publicação**: Consultar estatísticas como o número de publicações por categoria, autor ou ano.
- **Insights do Uso de Média**: Analisar a utilização de ficheiros multimédia e identificar ficheiros não utilizados ou desatualizados.
- **Relatórios Personalizáveis**: Gerar relatórios em PDF ou Excel adaptados às suas necessidades.

### 🔐 Gestão de Utilizadores e Autorização
- **Controlo de Acesso Baseado em Funções**: Atribuir funções (Administrador, Editor, Visualizador) aos utilizadores.
- **Perfis de Utilizador**: Gerir perfis de utilizador, incluindo detalhes pessoais e preferências.
- **Registos de Atividade**: Acompanhar atividades dos utilizadores para responsabilidade.
- **Autenticação**: Login seguro usando ASP.NET Core Identity com MFA opcional.

---

## 📂 Visão Geral das Páginas

| Nome da Página             | Descrição                                                                 |
|----------------------------|-------------------------------------------------------------------------|
| **Início**                 | Painel com métricas principais e links rápidos.                        |
| **Publicações**            | Gerir publicações, ver detalhes e realizar operações CRUD.             |
| **Pesquisa Google Books**  | Pesquisar e recuperar metadados de livros usando a API Google Books.   |
| **Média**                  | Carregar, pré-visualizar e gerir ficheiros multimédia.                 |
| **Galeria de Vídeos**      | Navegar e gerir ficheiros de vídeo.                                    |
| **Listas de Reprodução**   | Criar e gerir listas de reprodução para ficheiros multimédia.          |
| **Backup da Base de Dados**| Fazer backup da base de dados.                                         |
| **Backup de Publicações**  | Fazer backup de uploads de publicações.                               |
| **Backup de Média**        | Fazer backup de uploads de média.                                     |
| **Backup de Imagens**      | Fazer backup de uploads de imagens.                                   |
| **Gerir Autores**          | Gerir informações de autores.                                         |
| **Gerir Utilizadores**     | Gerir contas e funções de utilizadores.                               |
| **Tabelas Auxiliares**     | Gerir dados auxiliares como autores, categorias e editores.           |
| **Definições da Aplicação**| Configurar definições gerais da aplicação.                            |
| **Gestor de Ficheiros**    | Gerir ficheiros e diretórios.                                         |
| **Ver Registos**           | Ver registos da aplicação para depuração e monitorização.             |
| **Editar Prompts**         | Editar prompts de IA para geração de metadados.                      |
| **Relatórios**             | Gerar e descarregar relatórios para publicações e média.              |
| **Gestão de Utilizadores** | Gerir funções, permissões e perfis de utilizadores.                   |

---

## ⚙️ Tecnologias Utilizadas

- **Frontend**: Blazor WebAssembly
- **Backend**: ASP.NET Core Web API
- **Base de Dados**: SQL Server
- **Comunicação em Tempo Real**: SignalR
- **Integração com IA**: OpenAI API para recuperação de metadados
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

4. Atualize o ficheiro `appsettings.json` com a sua string de conexão à base de dados.

5. Execute as migrações:
```
   dotnet ef database update
```

6. Inicie a aplicação:
```
   dotnet run
```

---

## 📖 API e Integrações

A solução fornece uma API RESTful para gerir publicações e média. Endpoints principais incluem:

- **Publicações**: `/api/publications`
- **Média**: `/api/media`
- **Utilizadores**: `/api/users`

### Integrações
- **Armazenamento**: Os ficheiros multimédia são armazenados num sistema de ficheiros local.
- **Recuperação de Metadados com IA**: Obter automaticamente metadados usando IA quando as APIs externas falham.

