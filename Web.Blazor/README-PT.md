# 📚 Media Organizer App

[English version](README.md)

Já alguma vez sentiu frustração ao gerir os seus ficheiros multimédia, com centenas de ficheiros espalhados por diferentes discos ou dispositivos de armazenamento? Não seria ótimo tê-los organizados, facilitando a sua gestão?

A Aplicação Media Organizer App é uma aplicação para organizar os seus ficheiros multimédia (vídeos e ficheiros mp3) e documentos (PDFs e documentos Word).
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
- **Pesquisa & Filtro**: Pesquisa avançada por título, autor, categoria ou palavras-chave.
- **Deteção de Duplicados**: Sinalização automática de publicações duplicadas com base em metadados.
- **Relatórios**: Gerar relatórios detalhados sobre publicações, incluindo estatísticas por categoria, autor ou ano.

### 🖼️ Gestão de Média
- **Upload de Ficheiros**: Carregamento de ficheiros (arrastar e largar) para imagens, PDFs, vídeos e áudios.
- **Pré-visualização de Media**: Pré-visualizações integradas para tipos de ficheiros suportados.
- **Ligar Media a Publicações**: Associar ficheiros multimédia a publicações específicas.
- **Categorização e Etiquetagem**: Organize os ficheiros multimédia por categorias e etiquetas.
- **Opções de Armazenamento**: Suporte para armazenamento local de ficheiros.

### 📖 Visualizadores
#### 1. **Visualizador de PDF para Publicações**
- **Propósito**: Ver ficheiros de publicações diretamente no navegador sem ter de os descarregar.
- **Funcionalidades**:
  - Incorporar PDFs usando as tags `<iframe>` ou `<object>`.
  - Visualização interativa de PDFs com bibliotecas como [PDF.js](https://mozilla.github.io/pdf.js/).
  - Endpoint API seguro para servir ficheiros PDF (ex: `/api/publications/{id}/pdf`).

#### 2. **Visualizador de Média**
- **Visualizador MP4**:
  - Reproduzir vídeos MP4 usando a tag HTML `<video>`.
  - Controlos completos de reprodução (tocar, pausar, volume, ecrã inteiro).
  - Endpoint API seguro para servir ficheiros MP4 (ex: `/api/media/{id}/mp4`).
- **Visualizador MP3**:
  - Reproduzir ficheiros MP3.
  - Controlos completos de reprodução (tocar, pausar, volume).
  - Endpoint API seguro para servir ficheiros MP3 (ex: `/api/media/{id}/mp3`).

#### 3. **Opções de Media**
- **Novo Registo de Media**:
  - Criar novos registos multimédia do zero com metadados (ex: título, descrição, categoria).
  - Fazer upload de ficheiros multimédia diretamente na interface.
- **Integração com YouTube**:
  - Adicionar vídeos do YouTube usando a API do YouTube.
  - Obtenção automática de metadados (ex: título, descrição, miniatura).
- **Ficheiros MP3 Locais**:
  - Importar ficheiros MP3 do armazenamento local.
  - Categorização e etiquetagem automática dos ficheiros importados.

### 📊 Analítica & Relatórios
- **Estatísticas de Publicação**: Consultar estatísticas como o número de publicações por categoria, autor ou ano.
- **Insights do uso de Media**: Analisar a utilização de ficheiros multimédia e identificar ficheiros não utilizados ou desatualizados.
- **Relatórios Personalizáveis**: Gerar relatórios em PDF ou Excel adaptados às suas necessidades.

### 🔐 Gestão de Utilizadores & Autorização
- **Autenticação Segura**: Integração com sistemas de autenticação modernos, incluindo OAuth2, autenticação por dois fatores, etc.
- **Gestão de Perfis**: Gestão de perfis de utilizador com permissões e papéis personalizáveis (ex: Administrador, Editor, Utilizador).
- **Gestão de Equipas e Colaborações**: Gestão de múltiplos utilizadores com funções diferentes dentro da aplicação e possibilidade de colaboração na organização dos ficheiros.
---
### 🛠️ Tecnologias Utilizadas
- **Frontend**: [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), [MudBlazor](https://mudblazor.com/) (UI), [HTML5](https://developer.mozilla.org/docs/Web/HTML), [CSS3](https://developer.mozilla.org/docs/Web/CSS)
- **Backend**: [ASP.NET Core Web API](https://learn.microsoft.com/aspnet/core/web-api), [Entity Framework Core](https://docs.microsoft.com/ef/core/), [SQLite](https://www.sqlite.org/index.html) ou SQL Server
- **Armazenamento de Ficheiros**: Suporte integrado para armazenamento local e possibilidade de integração futura com armazenamento na cloud.
- **Segurança**: Autenticação JWT, proteção de endpoints API, cifragem de passwords.

### 🚀 Primeiro Passos

#### 1. Pré-requisitos
- [.NET 7 SDK ou superior](https://dotnet.microsoft.com/en-us/download)
- [Node.js](https://nodejs.org/) (se quiser modificar ficheiros JS/css)
- Editor como [Visual Studio Code](https://code.visualstudio.com/) ou [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)

#### 2. Clone o Repositório

```bash
git clone https://github.com/fauxtix/MediaOrganiserApp.Web.Blazor.git
cd MediaOrganiserApp.Web.Blazor
```

#### 3. Executar a Solução

```bash
dotnet build
dotnet run --project src/MediaOrganiserApp.Web.Blazor
```

#### 4. Aceder à Aplicação

Abra o navegador e aceda a `https://localhost:5001` ou ao endereço indicado no terminal.

### 📝 Plano de Desenvolvimento

- [ ] Adicionar suporte a armazenamento em nuvem (ex: Azure Blob Storage)
- [ ] Implementar notificações em tempo real (SignalR)
- [ ] Traduções para mais idiomas (contribuições são bem-vindas!)
- [ ] App móvel (Xamarin ou MAUI)

### 👥 Contribuir

Contribuições são bem-vindas! Por favor:
1. Fork este projeto
2. Crie uma branch (`git checkout -b feat/novo-recurso`)
3. Commit as suas alterações (`git commit -am 'Adiciona novo recurso'`)
4. Push a branch (`git push origin feat/novo-recurso`)
5. Abra um Pull Request

Leia o [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) e o [CONTRIBUTING.md](CONTRIBUTING.md) para mais detalhes.

### ⚖️ Licença

Distribuído sob licença MIT. Veja [LICENSE](LICENSE) para mais informações.

### 📧 Contacto

Se tiver questões ou sugestões, abra um issue ou contacte fauxtix@gmail.com.

---

Desenvolvido com ❤️ por [fauxtix](https://github.com/fauxtix)
