# Manual de Utilização — Media Organizer

> Guia completo da aplicação **Media Organizer**: gestão de publicações (livros, e-books e documentos), autores, média (áudio, vídeo e YouTube), galerias, funcionalidades de Inteligência Artificial, tabelas de apoio, administração, relatórios e ferramentas.
>
> Este manual destina-se a **utilizadores finais** e a **administradores**. Não são necessários conhecimentos de programação.
>
> 💡 O manual está disponível **dentro da aplicação**: clique no botão de ajuda (`?`) na **barra superior** para o abrir num diálogo, sem perder a página onde está.

---

## Índice

- [0. Quick Start — começar em 5 minutos](#0-quick-start--começar-em-5-minutos)
- [1. Visão Geral](#1-visão-geral)
- [2. Primeiros Passos](#2-primeiros-passos)
- [3. Dashboard (Home)](#3-dashboard-home)
- [4. Publicações (livros, e-books e documentos)](#4-publicações-livros-e-books-e-documentos)
- [5. Autores](#5-autores)
- [6. Média (Áudio, Vídeo e YouTube)](#6-média-áudio-vídeo-e-youtube)
- [7. Galerias](#7-galerias)
- [8. Inteligência Artificial](#8-inteligência-artificial)
- [9. Tabelas de Apoio](#9-tabelas-de-apoio)
- [10. Administração](#10-administração)
- [11. Relatórios](#11-relatórios)
- [12. Ferramentas](#12-ferramentas)
- [13. Dicas e Atalhos](#13-dicas-e-atalhos)
- [14. Resolução de Problemas](#14-resolução-de-problemas)
- [15. Glossário](#15-glossário)
- [16. Cenários completos (passo a passo)](#16-cenários-completos-passo-a-passo)
- [Apêndice A — Rotas principais](#apêndice-a--rotas-principais)
- [Apêndice B — Checklist de manutenção periódica](#apêndice-b--checklist-de-manutenção-periódica)
- [Apêndice C — Perguntas frequentes (FAQ)](#apêndice-c--perguntas-frequentes-faq)

---

## 0. Quick Start — começar em 5 minutos {#0-quick-start--começar-em-5-minutos}

Se é a primeira vez que usa a aplicação, comece por estes passos essenciais.

### 0.1 Entrar na aplicação

1. Abra o endereço da aplicação no browser (em ambiente local, normalmente `https://localhost:4110`).
2. Introduza o **email** e a **password** da sua conta.
3. Clique em **Entrar**.
4. Se ainda não tem conta, use a ligação de **registo** no ecrã de autenticação e aguarde a criação do utilizador (em algumas instalações o registo é feito pelo administrador).

### 0.2 Escolher o idioma

1. Abra **Definições** (rodapé do menu lateral) → **Idioma**.
2. Escolha entre **Português (predefinido)**, **Inglês**, **Francês** ou **Espanhol**.
3. A escolha é guardada e aplicada a toda a aplicação (a página recarrega).

> 💡 A barra superior **não** tem seletor de idioma: mostra apenas o idioma atual (ícone de globo + `PT`/`EN`/`FR`/`ES`), a par do nome de utilizador, do botão de ajuda (`?`), do botão de tema e de **Sair**.

### 0.3 Escolher o tema

1. Na barra superior, clique no ícone de **tema** (sol/lua).
2. A aplicação alterna entre **modo claro** e **modo escuro**.
3. A barra superior e o menu lateral são sempre escuros — é intencional, para manter a identidade visual.

### 0.4 Criar ou importar a primeira publicação

1. No menu lateral, abra **Publicações → Gestão** (`/publicacoes`).
2. Para criar à mão (ex.: um livro físico), clique em **Novo** (botão verde com `+`), preencha pelo menos **Título**, **Autor**, **Categoria**, **Editora**, **Formato** e **Local de arquivo** e clique em **Guardar**.
3. Para importar um documento do PC (PDF, DOCX ou mensagem Outlook `.msg`), use **Importar PDF/DOCX**, selecione o ficheiro e deixe a aplicação ler os **metadados**.
4. **Reveja** sempre os dados preenchidos automaticamente antes de gravar.

> 💡 A área **Publicações** não serve só para livros: gere também **e-books (PDF)**, **documentos de texto (DOCX)**, **mensagens Outlook** e outros documentos. O formulário é o mesmo; muda sobretudo a origem dos dados (manual ou importada).

### 0.5 Criar o primeiro autor

1. Abra **Publicações → Autores** (`/autores`).
2. Clique em **Novo**.
3. Preencha o **nome** e, se possível, a **biografia**.
4. **Guarde**.
5. Opcional: carregue uma **fotografia** e use a IA para gerar/ completar o **perfil do autor**.

### 0.6 Adicionar média (áudio/vídeo)

1. Abra **Media → Gestão** (`/media`).
2. Clique em **Novo** para um registo manual ou **Upload** para enviar ficheiros.
3. Escolha o tipo (áudio, vídeo ou YouTube), preencha os dados e **Guarde**.

### 0.7 Falar com a IA

1. Abra **AI → Chat** (`/ai/openrouter-chat`) para o chat na cloud, ou **AI → Chat (Ollama)** (`/ai/chat`) para o chat local.
2. Escreva a pergunta e pressione **Enter**.
3. A resposta aparece em streaming; pode **copiar**, avaliar com **👍/👎** e consultar o histórico.

### 0.8 Tabela “quero fazer X → vou a Y”

| Quero… | Vou a… | Rota |
|---|---|---|
| Ver o resumo geral | Home | `/` |
| Gerir publicações (livros, e-books, documentos) | Publicações → Gestão | `/publicacoes` |
| Ver capas em galeria | Publicações → Galeria | `/imagegallery` |
| Gerir autores | Publicações → Autores | `/autores` |
| Gerir média | Media → Gestão | `/media` |
| Ver vídeos | Media → Galeria | `/videogallery` |
| Criar playlists | Media → Playlists | `/media/playlists` |
| Editar prompts de IA | AI → Editor de Prompts | `/edit-prompts` |
| Chat cloud | AI → Chat | `/ai/openrouter-chat` |
| Chat local | AI → Chat (Ollama) | `/ai/chat` |
| Gerir utilizadores | Manutenção → Utilizadores | `/admin/manageusers` |
| Gerir tabelas auxiliares | Manutenção → Tabelas Auxiliares | `/admin/managelookuptables` |
| Ver logs | Logs | `/admin/viewapplogs` |
| Gerir ficheiros | Gestor de Ficheiros | `/admin/filemanager` |
| Backup da base de dados | Backup → Base de Dados | `/backupdatabase` |
| Definições | Rodapé do menu → Definições | `/admin/appsettings2` |

---

## 1. Visão Geral {#1-visão-geral}

### 1.1 O que é a Media Organizer

A Media Organizer é uma aplicação de **catálogo pessoal/organizacional** que reúne, num só lugar:

- **Publicações** — coleção mista de **livros impressos, e-books (PDF), documentos de texto (DOCX), mensagens Outlook (`.msg`)** e outros documentos, com capa, sinopse, ISBN, editora, categoria, formato, local de arquivo e classificação pessoal. Pode criar à mão ou **importar** o ficheiro e deixar a aplicação preencher os dados a partir dos metadados.
- **Autores** — fichas completas, com biografia, fotografia e lista de livros.
- **Média** — áudio, vídeo e ligações YouTube, organizados em playlists e reproduzidos na própria aplicação.
- **Galerias** — vistas visuais de imagens, vídeos e música.
- **Inteligência Artificial** — tradução, resumos, metadados, perfis de autor e dois chats (cloud e local).

### 1.2 Como a aplicação está montada (sem detalhes técnicos)

A aplicação tem duas partes que trabalham em conjunto:

1. **Serviço/API** — guarda e processa os dados (base de dados, ficheiros, IA).
2. **Interface** — aquilo que vê no browser.

Para o utilizador, comporta-se como um único site. Em ambiente local, os dois componentes podem ocupar portas diferentes; se uma página não carregar, ver [Resolução de Problemas](#14-resolução-de-problemas).

### 1.3 Papéis de utilizador

Existem dois papéis:

| Papel | O que pode ver | O que pode fazer |
|---|---|---|
| **Admin** | **Todos** os dados de todos os utilizadores | Tudo: gestão de utilizadores, tabelas auxiliares, definições, logs, backups, apagar histórico de todos |
| **GeneralUser** | **Apenas os seus próprios** dados | Criar/editar/apagar as suas publicações, autores, média e playlists; usar IA; gerir o seu histórico de chat |

> ⚠️ **Atenção:** o isolamento de dados é aplicado no servidor. Um GeneralUser **não consegue** ver registos de outro utilizador, mesmo alterando o endereço da página. Se um registo “desapareceu”, confirme que está autenticado com a conta correta.

### 1.4 Mapa mental da navegação

```
Home (/)
├── Publicações
│   ├── Gestão (/publicacoes)
│   ├── Galeria (/imagegallery)
│   └── Autores (/autores)
├── Media
│   ├── Gestão (/media)
│   ├── Galeria (/videogallery)
│   └── Playlists (/media/playlists)
├── AI
│   ├── Editor de Prompts (/edit-prompts)
│   ├── Chat (Ollama) (/ai/chat)
│   └── Chat (/ai/openrouter-chat)
├── [Admin] Backup
│   ├── Base de Dados (/backupdatabase)
│   ├── Uploads Publicações (/backupuploads/1)
│   ├── Uploads Media (/backupuploads/2)
│   └── Uploads Imagens (/backupuploads/3)
├── [Admin] Manutenção
│   ├── Utilizadores (/admin/manageusers)
│   └── Tabelas Auxiliares (/admin/managelookuptables)
├── [Admin] Gestor de Ficheiros (/admin/filemanager)
├── [Admin] Logs (/admin/viewapplogs)
└── Definições (/admin/appsettings2)   ← rodapé do menu
```

### 1.5 O que fica guardado onde

- **Registos** (publicações, autores, média, playlists, conversas, avaliações) — na base de dados.
- **Ficheiros** (PDFs, capas, imagens de autores, áudio, vídeo) — em pastas configuradas nas Definições.
- **Preferências locais** (tema) — no browser; o **idioma** é definido em **Definições → Idioma** e aplicado por cookie de cultura.
- **Configuração de IA** — na base de dados, editável na área **IA** das Definições.

---

## 2. Primeiros Passos {#2-primeiros-passos}

### 2.1 Autenticação

#### Iniciar sessão

1. Abra a aplicação.
2. Introduza **email** e **password**.
3. Clique em **Entrar**.
4. Se as credenciais estiverem corretas, entra diretamente no **Dashboard**.

#### Recuperar a password

1. No ecrã de autenticação, clique em **Esqueci-me da password**.
2. Introduza o email associado à conta.
3. Siga a ligação enviada por email.
4. Defina a nova password.

> 💡 Se o email de recuperação não chegar, confirme a pasta de spam ou contacte o administrador (o envio de email depende da configuração do servidor).

#### Terminar sessão

- Use a opção de **sair** na barra superior. A sessão é encerrada e volta ao ecrã de autenticação.

### 2.2 Idiomas

A aplicação está disponível em **quatro idiomas**:

| Idioma | Código | Observações |
|---|---|---|
| Português | `pt` | Idioma **predefinido** |
| Inglês | `en` | — |
| Francês | `fr` | — |
| Espanhol | `es` | — |

**Como trocar:**

1. Abra **Definições** (rodapé do menu lateral) → secção **Idioma**.
2. Escolha o idioma pretendido na lista.
3. A aplicação guarda a escolha e recarrega a interface já no novo idioma.

> 💡 A **barra superior não tem seletor de idioma**. Mostra apenas um indicador com o idioma atual (globo + `PT`/`EN`/`FR`/`ES`), a par do nome de utilizador, do botão de ajuda (`?`), do botão de tema e de **Sair**. A alteração faz-se sempre nas **Definições**.

**O que é traduzido:** menus, botões, etiquetas, mensagens de validação e textos da interface.

**O que não é traduzido:** o **conteúdo** que introduz (títulos, sinopses, biografias) e os nomes de modelos de IA. Pode ter a interface em inglês e as suas publicações em português — é normal.

> 💡 A escolha fica guardada nas Definições e é aplicada a toda a aplicação (não é uma preferência isolada por separador).

### 2.3 Tema claro/escuro

1. Clique no ícone de **tema** na barra superior.
2. A aplicação alterna instantaneamente, sem recarregar.
3. A preferência é memorizada no browser.

**Regras visuais importantes:**

- A **sidebar** e a **barra superior** são **sempre escuras**, em ambos os temas.
- Os cartões, grelhas, formulários e diálogos acompanham o tema.
- Os gráficos do Dashboard também se adaptam.

> 💡 Se notar um artefacto visual num gráfico após passar o rato, basta mover o rato para fora e voltar a entrar; a aplicação corrige automaticamente o repaint.

### 2.4 Anatomia da sidebar

A sidebar tem três zonas:

1. **Topo** — itens gerais: Home, Publicações, Media, AI.
2. **Meio** — itens administrativos (apenas para Admin): Backup, Manutenção, Gestor de Ficheiros, Logs.
3. **Rodapé** — **Definições**.

**Comportamento:**

- Os grupos com filhos expandem/recolhem ao clicar.
- O item ativo fica realçado.
- Em ecrãs pequenos, a sidebar pode ser recolhida.

### 2.5 Barra superior

Contém, tipicamente:

- O **nome/marca** da aplicação.
- O **idioma atual** (ícone de globo + `PT`/`EN`/`FR`/`ES`) — é apenas um indicador; a troca faz-se nas **Definições**.
- O **nome de utilizador**.
- O botão de **ajuda** (`?`), que abre este manual num diálogo, sem sair da página onde está.
- O **botão de tema**.
- A opção de **sessão** (sair).

### 2.6 Primeiro acesso — checklist

- [ ] Consegui entrar.
- [ ] Escolhi o idioma.
- [ ] Escolhi o tema.
- [ ] Criei ou importei pelo menos um livro/documento.
- [ ] Criei pelo menos um autor.
- [ ] Confirmei que as tabelas de apoio essenciais têm dados (categorias, formatos, editoras).
- [ ] Experimentei uma tradução ou um resumo com IA.
- [ ] Abri o chat e enviei uma mensagem.

---

## 3. Dashboard (Home) {#3-dashboard-home}

O Dashboard é a página inicial (`/`) e dá uma visão global do acervo e da atividade.

### 3.1 Cartões de totais

No topo aparecem cartões com contagens, por exemplo:

- **Publicações**.
- **Autores**.
- **Editoras**.
- **Formatos**.
- **Média**.

Cada cartão é clicável (quando aplicável) e leva à página correspondente.

### 3.2 Gráficos circulares

Existem gráficos de distribuição por:

- **Categorias de publicações**.
- **Formatos de publicações**.
- **Editoras**.
- **Categorias de média**.
- **Formatos de média**.

**Como ler:**

- Cada fatia representa uma categoria/formato/editora.
- Ao passar o rato, aparece o nome e o valor.
- Fatias muito pequenas podem agrupar-se visualmente; consulte as tabelas de apoio para o detalhe.

> 💡 Se um gráfico estiver vazio, não há registos suficientes (ou os registos não têm essa classificação preenchida).

### 3.3 Secção “Saúde” (qualidade dos dados)

Indicadores de registos incompletos ou problemas:

| Indicador | Significado | O que fazer |
|---|---|---|
| **Publicações sem capa** | Publicações sem imagem de capa | Abrir cada publicação e carregar capa |
| **Publicações sem páginas** | Publicações sem número de páginas | Completar o campo **Páginas** |
| **Autores sem publicações** | Autores sem publicações associadas | Associar publicações ou remover autores órfãos |
| **Editoras sem publicações** | Editoras sem uso | Reutilizar ou remover nas tabelas de apoio |
| **Categorias sem publicações** | Categorias sem uso | Reutilizar ou remover |
| **Formatos sem publicações** | Formatos sem uso | Reutilizar ou remover |
| **Locais de arquivo sem publicações** | Locais sem uso | Reutilizar ou remover |
| **Títulos duplicados** | Possíveis duplicados | Confirmar e consolidar registos |

> ⚠️ Estes indicadores são **sugestões**, não erros. Um registo pode estar incompleto de propósito.

### 3.4 Secção “Atividade”

Métricas do uso da IA/chat:

- **Conversas** — número de conversas.
- **Total de respostas**.
- **Últimos 7 dias**.
- **Latência média**.
- **Tokens/segundo** (com nota explicativa de que é uma medida aproximada).
- **Tamanho médio de tokens**.
- **Percentagem de uso cloud**.
- **Modelos mais usados** (tabela).

### 3.5 Secção “IA em uso interno”

Mostra o uso da IA **fora do chat** (traduções, resumos, metadados, perfis, etc.):

- Total de operações.
- Últimos 7 dias.
- Chamadas via **OpenRouter**.
- Chamadas via **Ollama** (local).
- **Fallbacks** (quantas vezes a cloud falhou e se recorreu ao local).
- Tabela **por operação** (traduzir, resumo, metadados…).
- Tabela dos **últimos registos** com operação, fornecedor, contexto, cenário, duração e data.

**Como interpretar:**

- Muitos **fallbacks** = a cloud está instável, sem chave ou sem saldo; a app está a usar o Ollama.
- Durações altas = modelos locais em máquinas modestas ou prompts extensos.

### 3.6 Secção “Auditoria”

- **Últimos logins**.
- **Erros recentes** (com nível e mensagem).
- Contagem total de erros.

> 💡 Para o detalhe completo, abra **Logs** (`/admin/viewapplogs`).

### 3.7 Atualização dos dados

O Dashboard carrega ao abrir a página. Se fez alterações noutro separador, recarregue a página (F5) para ver os números atualizados.

---

## 4. Publicações {#4-publicações-livros-e-books-e-documentos}

Página de gestão: **Gestão de Publicações** (`/publicacoes`).

### 4.1 A grelha de publicações

A grelha lista as publicações a que tem acesso, com colunas como título, autor, editora, categoria, formato, ano, classificação e estado de leitura.

**Operações disponíveis:**

| Ação | Como |
|---|---|
| Ordenar | Clicar no cabeçalho da coluna |
| Filtrar | Usar a linha de filtros do cabeçalho |
| Pesquisar | Caixa de pesquisa da grelha |
| Agrupar | Arrastar o cabeçalho para a área de agrupamento |
| Redimensionar colunas | Arrastar a borda do cabeçalho |
| Paginar | Controlos no fundo da grelha |

### 4.2 Criar uma publicação

1. Clique em **Novo** (botão verde `+`).
2. Preencha o formulário (ver [4.4 Campos](#44-campos-do-formulário)).
3. Clique em **Guardar**.
4. O registo aparece na grelha.

> 💡 **Título** e **Autor** são os campos mais importantes. Para **gravar** uma publicação, a aplicação exige também **Categoria**, **Editora**, **Formato** e **Local de arquivo** — crie-os primeiro nas Tabelas de Apoio se faltarem.

### 4.3 Editar e apagar

**Editar:**

1. Na linha da publicação, clique no ícone de **lápis** (Editar).
2. Altere os campos.
3. **Guarde**.

**Apagar:**

1. Na linha, clique no ícone de **lixo** (Apagar).
2. Confirme no diálogo.

> ⚠️ A eliminação é permanente. Se a publicação tiver ficheiros associados, confirme se também pretende removê-los do disco (a aplicação avisa quando aplicável).

### 4.4 Campos do formulário {#44-campos-do-formulário}

| Campo | Obrigatório | Descrição |
|---|---|---|
| **ID** | — | Gerado automaticamente; só leitura |
| **ISBN** | Não | Identificador da publicação; útil para pesquisa Google Books |
| **Data de movimento** | Não | Data de aquisição/registo |
| **Título** | **Sim** | Nome da publicação (2 a 256 caracteres) |
| **Ano de edição** | Não | Se preenchido, tem de ter **4 dígitos** e não pode ser superior ao ano atual; pode ser sugerido por IA |
| **Páginas** | Não | Número de páginas |
| **Preço** | Não | Valor monetário |
| **Autor** | **Sim** | Selecionado da lista de autores (pesquisável) |
| **Categoria** | **Sim** | Categoria da publicação |
| **Editora** | **Sim** | Editora |
| **Formato** | **Sim** | Formato físico/digital |
| **Local de arquivo** | **Sim** | Onde o exemplar/ficheiro está guardado |
| **Classificação (Rating)** | Não | Avaliação pessoal (estrelas) |
| **Lido** | Não | Marca se já leu |
| **Sinopse** | Não | Descrição/resumo; suporta tradução por IA |

**Notas sobre os seletores:**

- Os campos **Autor**, **Categoria**, **Editora**, **Formato** e **Local de arquivo** são pesquisáveis — escreva parte do nome para filtrar.
- Se faltar uma opção, crie-a primeiro nas **Tabelas de Apoio** (ou peça ao Admin).

### 4.5 Importar publicações a partir de ficheiros (PDF/DOCX)

1. Na página de gestão, use o botão de **Importar PDF/DOCX**.
2. Selecione um ou mais ficheiros.
3. A aplicação cria/atualiza registos a partir do conteúdo.
4. Reveja os registos criados e corrija o que for necessário.

> 💡 A importação tenta extrair título, autor e outros dados. **Reveja sempre** o resultado — a extração automática não é perfeita.

### 4.6 Ler uma publicação na aplicação

1. Na linha da publicação, clique no comando **Ler PDF**.
2. O visualizador abre o ficheiro associado.
3. Pode **editar a publicação** a partir do visualizador ou **fechar**.
4. Para documentos Word, aparece um visualizador próprio, com as mesmas ações.

> ⚠️ Se o ficheiro não existir no local indicado, a aplicação avisa. Use as páginas de diagnóstico (ver [4.9](#49-ficheiros-em-falta-e-caminhos-errados)).

### 4.7 Pesquisa Google Books

1. No formulário da publicação (ou no fluxo de pesquisa), abra **Pesquisar Online**.
2. Introduza o **título** e/ou **autor**.
3. A aplicação consulta o **Google Books**.
4. Escolha um resultado para preencher os campos.
5. Se o resultado não tiver descrição, a aplicação usa o **excerto** disponível.
6. Se não houver resultados, pode ser acionada a **IA** para gerar um resumo.
7. Confirme e **guarde**.

**Boas práticas:**

- Pesquise com o **título exato** (sem aspas) e o **apelido do autor**.
- Se a publicação for traduzida, experimente o título original.
- Verifique sempre o **ano** e a **editora** antes de guardar.

### 4.8 Funcionalidades de IA na publicação

No formulário da publicação pode usar:

| Função | O que faz | Onde aparece o resultado |
|---|---|---|
| **Resumo IA** | Gera uma sinopse a partir de título/autor/dados | Campo **Sinopse** |
| **Traduzir** | Traduz a sinopse/texto entre idiomas | Diálogo de tradução → **Confirmar** |
| **Ano de edição IA** | Sugere o ano de edição | Campo **Ano de edição** |
| **Livros do autor** | Lista/gera livros do autor selecionado | Painel **Livros do Autor** |

O funcionamento detalhado (incluindo o **chip do modelo** e o botão **Cancelar**) está em [8. Inteligência Artificial](#8-inteligência-artificial).

### 4.9 Ficheiros em falta e caminhos errados {#49-ficheiros-em-falta-e-caminhos-errados}

Duas páginas de diagnóstico ajudam a manter o acervo coerente:

#### Publicações não encontradas no disco (`/pubsnotfoundondisk`)

- Lista publicações cujo **ficheiro referenciado não existe**.
- Ações típicas: corrigir o caminho, voltar a carregar o ficheiro ou remover o registo.

#### Publicações com caminho errado (`/pubswithwrongpath`)

- Lista publicações cujo caminho aponta para um local inválido/antigo.
- Ações típicas: atualizar o **Local de arquivo**, mover o ficheiro ou corrigir a configuração de pastas.

> 💡 Execute estas verificações depois de mover pastas, mudar de computador ou restaurar backups.

### 4.10 Estatísticas e “livros do autor”

- O botão de **Estatísticas** abre um resumo com contagens e distribuições.
- O botão **Livros do Autor** abre um painel com os livros do autor selecionado, permitindo navegar rapidamente entre obras.

### 4.11 Boas práticas para publicações

- Preencha **Autor**, **Categoria**, **Editora** e **Formato** para melhorar os gráficos do Dashboard.
- Use **ISBN** para evitar duplicados.
- Nomeie os ficheiros de forma consistente (ex.: `ISBN - Título.pdf`).
- Mantenha as **capas** na pasta configurada.
- Reveja periodicamente a secção **Saúde** do Dashboard.

---

## 5. Autores {#5-autores}

Página: **Publicações → Autores** (`/autores`).

### 5.1 Lista de autores

A grelha mostra os autores a que tem acesso, com nome, número de livros e fotografia (quando existe).

**Operações:**

- Pesquisar por nome.
- Ordenar e filtrar.
- Abrir a ficha (**Editar**).
- Ver em modo **só leitura**.
- Apagar (com confirmação).

### 5.2 Criar um autor

1. Clique em **Novo**.
2. Preencha **Nome** (obrigatório, 2 a 128 caracteres).
3. Preencha **Sobre** (biografia; opcional, mas recomendado).
4. Indique a **data de nascimento** (opcional).
5. Carregue uma **fotografia** (opcional).
6. **Guarde**.

### 5.3 Ficha do autor

A ficha (edição) inclui:

| Campo | Obrigatório | Descrição |
|---|---|---|
| **Nome** | **Sim** | Nome do autor (2 a 128 caracteres) |
| **Data de nascimento** | Não | Data de nascimento do autor |
| **Sobre / Biografia** | Não | Texto livre (editor de texto com formatação); suporta tradução por IA |
| **Fotografia** | Não | Imagem do autor |
| **Livros** | — | Lista de obras associadas |
| **Perfil IA** | — | Perfil gerado/completado por IA |

### 5.4 Fotografia do autor

1. Na ficha, use a área de **upload**.
2. Selecione a imagem (formatos comuns: JPG, PNG).
3. Aguarde o progresso.
4. A imagem fica visível na ficha e na lista.

> 💡 Use imagens quadradas ou retrato, com boa resolução, para melhor apresentação.

### 5.5 Biografia e tradução

1. Escreva ou cole a biografia.
2. Use **Traduzir** para converter para outro idioma.
3. Reveja o texto no diálogo.
4. Clique em **Confirmar** para aplicar.

> ⚠️ A tradução substitui o texto do campo quando confirma. Se quiser manter o original, copie-o primeiro para um bloco de notas.

### 5.6 Livros do autor

- Na ficha ou na lista, abra **Livros do Autor**.
- Pode **associar** livros existentes e **criar** novos livros já ligados ao autor.
- A lista reflete o que está na base de dados para esse autor.

### 5.7 Gerar perfil com IA

1. Abra a ficha do autor.
2. Clique em **Gerar perfil IA** (ou equivalente).
3. A aplicação tenta primeiro a **cloud** (OpenRouter) e, se falhar, o **Ollama local**.
4. O resultado é apresentado para revisão.
5. Edite se necessário e **guarde**.

**O que a IA pode usar:** nome, biografia existente e obras conhecidas.

> 💡 Revise sempre o resultado: a IA pode inventar datas ou obras. O perfil é um **ponto de partida**, não uma fonte de verdade.

### 5.8 Vista só leitura

A vista **AuthorReadOnly** apresenta a ficha sem permitir edição — útil para consulta rápida ou apresentação.

### 5.9 Apagar autores

1. Clique em **Apagar** na linha do autor.
2. Confirme.

> ⚠️ Se o autor tiver livros associados, a aplicação pode impedir a eliminação ou avisar. Resolva primeiro as associações.

---

## 6. Média (Áudio, Vídeo e YouTube) {#6-média-áudio-vídeo-e-youtube}

Página de gestão: **Media → Gestão** (`/media`).

### 6.1 Tipos de média

A aplicação distingue:

- **Áudio** — MP3 e formatos semelhantes.
- **Vídeo** — ficheiros de vídeo locais.
- **YouTube** — ligações a vídeos alojados no YouTube (sem ficheiro local).

Cada tipo tem campos e ações próprias, mas partilha a mesma grelha e o mesmo padrão de edição.

### 6.2 A grelha de média

Colunas típicas: título, artista/autor, categoria, formato, classificação, tipo e ações.

**Ações por linha:**

| Ícone | Ação |
|---|---|
| ▶ | **Reproduzir** (player adequado ao tipo) |
| ℹ | **Informação/detalhe** |
| ✏ | **Editar** |
| 🗑 | **Apagar** |

### 6.3 Criar um registo de média

1. Clique em **Novo**.
2. Escolha o **tipo** (áudio, vídeo ou YouTube).
3. Preencha os campos (ver tabela abaixo).
4. Para YouTube, introduza a **ligação** do vídeo.
5. **Guarde**.

**Campos e validação:**

| Campo | Obrigatório | Notas |
|---|---|---|
| **Título** (nome do ficheiro/registo) | **Sim** | 2 a 256 caracteres |
| **Tipo** | **Sim** | Áudio ou vídeo (YouTube é tratado como caso próprio) |
| **Autor/Artista** | **Sim** | — |
| **Formato de média** | **Sim** | Ex.: MP3, MP4 (tabela de apoio) |
| **Género/Categoria** | **Sim** | Categoria de média (tabela de apoio) |
| **Ano de edição** | Não | Se preenchido, 4 dígitos e não superior ao ano atual |
| **Data de movimento** | Não | Não pode ser uma data futura |
| **Classificação** | Não | Estrelas |
| **Notas/descrição** | Não | Suporta IA |

### 6.4 Carregar ficheiros (Upload)

1. Clique em **Upload**.
2. Selecione um ou mais ficheiros.
3. Escolha o destino/tipo, se solicitado.
4. Aguarde a conclusão.
5. Reveja os registos criados.

**Recomendações:**

- Evite ficheiros enormes sem necessidade.
- Use nomes sem caracteres especiais problemáticos.
- Confirme o **formato de media** (categoria) para os gráficos ficarem corretos.

### 6.5 Metadados e notas com IA

No formulário/detalhe de média pode usar:

- **Notas IA** — gera uma descrição/notas sobre o item.
- **Metadados IA** — sugere dados como género, ano, artista.
- **Ano/edição** — quando aplicável.

O resultado é sempre **revisto por si** antes de guardar.

### 6.6 Playlists

Página: **Media → Playlists** (`/media/playlists`).

**Criar uma playlist:**

1. Clique em **Novo**.
2. Dê um **nome** e, se quiser, uma descrição.
3. **Guarde**.
4. Abra a playlist e **adicione itens** (média existente).

**Gerir itens:**

- Adicionar/remover.
- Reordenar (quando disponível).
- Reproduzir a playlist completa.

**Reproduzir:**

1. Abra a playlist.
2. Clique em **Reproduzir**.
3. O **Player de Playlist** (`/media/playlists/{id}/player`) abre com a fila.
4. Use os controlos para avançar/retroceder.

> 💡 Se um item não reproduzir, confirme que o ficheiro existe no disco ou que a ligação YouTube é válida.

### 6.7 Player MP3

Página: **`/mp3player`**.

- Reproduz faixas de áudio.
- Permite escolher faixa, pausar, avançar e ajustar volume.
- Mostra informação da faixa atual.

> 💡 O player funciona dentro da página; para continuar a ouvir enquanto navega, abra-o num separador próprio do browser.

### 6.8 Player YouTube

Página: **`/youtubeplayer`**.

- Reproduz vídeos através da ligação guardada no registo.
- Se a ligação for inválida ou o vídeo tiver sido removido, aparece um erro — corrija a ligação no registo.

### 6.9 Detalhe de média

- **Detalhe de áudio** (`Mp3Detail`) — informação e reprodução.
- **Detalhe de vídeo** (`VideoDetail`) — informação e reprodução.

### 6.10 YouTube e subscrições

A aplicação inclui uma área de **subscrições** (`/subscriptions`) para acompanhar canais/ligou YouTube, quando configurada.

### 6.11 Boas práticas para média

- Consistência nos nomes de **artista** e **categoria**.
- Uma playlist por contexto (ex.: “Leituras”, “Viagens”).
- Para YouTube, prefira ligações “não listadas” ou públicas estáveis.
- Verifique os **formatos de média** nas tabelas de apoio antes de importar em massa.

---

## 7. Galerias {#7-galerias}

As galerias oferecem vistas visuais alternativas às grelhas.

### 7.1 Galeria de imagens (`/imagegallery`)

**O que mostra:** capas de livros e imagens associadas, numa grelha de miniaturas.

**Como usar:**

1. Abra **Publicações → Galeria**.
2. Percorra as imagens.
3. Clique numa imagem para ampliar/ver detalhe.
4. Use a ligação para abrir o registo correspondente.

**Comportamento:**

- Imagens em falta mostram um placeholder.
- A galeria respeita o isolamento de dados (só vê o que é seu, salvo Admin).

### 7.2 Galeria de vídeos (`/videogallery`)

**O que mostra:** miniaturas dos vídeos, com reprodução integrada.

**Como usar:**

1. Abra **Media → Galeria**.
2. Clique num vídeo para reproduzir.
3. Use o detalhe para editar metadados.

### 7.3 Galeria de música (`/musicgallery`)

**O que mostra:** vista tipo **polaroid** das faixas/álbuns.

**Interações:**

- Passar o rato realça o item (cor de destaque).
- Clicar seleciona/reproduz.
- A seleção é visualmente distinta.

> 💡 A galeria de música é ideal para navegar por capas de álbuns; para edição de metadados, use a grelha de **Media**.

### 7.4 Quando uma galeria está vazia

Se não houver imagens/vídeos/faixas, aparece a mensagem **“Sem dados disponíveis”**. Verifique:

1. Se existem registos do tipo certo.
2. Se os ficheiros de imagem/vídeo estão associados.
3. Se está com a conta/utilizador corretos.

---

## 8. Inteligência Artificial {#8-inteligência-artificial}

A IA é uma das áreas mais ricas da aplicação. Esta secção explica **como funciona** e **como usar cada função**.

### 8.1 Arquitetura em duas camadas (o essencial)

A aplicação usa **dois fornecedores**:

1. **Cloud (OpenRouter)** — **primeira escolha**. Rápida e com modelos maiores, mas depende de internet, chave de API e limites.
2. **Local (Ollama)** — **reserva**. Corre na máquina, sem custos por chamada, mas mais lenta e dependente do hardware.

**Fluxo típico:**

```
Pedido do utilizador
        │
        ▼
  Cloud (OpenRouter)  ──falha/sem chave/limite──►  Local (Ollama)
        │                                              │
        └──────────── resposta ────────────────────────┘
```

**O que conta como “falha” da cloud:** sem ligação, sem chave, limite diário esgotado, erro HTTP, resposta inválida ou vazia.

> 💡 Este mecanismo é **automático**. O utilizador não precisa de escolher o fornecedor em cada operação — mas **vê qual foi usado** no chip do modelo (ver [8.3](#83-o-chip-do-modelo)).

### 8.2 Onde a IA é usada

| Operação | Onde | Resultado |
|---|---|---|
| **Traduzir** | Formulários de publicação/autor, ferramenta de tradução | Texto traduzido |
| **Resumo de publicação** | Edição de publicação / pesquisa Google Books | Sinopse |
| **Ano de edição** | Edição de publicação | Ano sugerido |
| **Notas de média** | Edição de média | Notas/descrição |
| **Metadados de média** | Edição de média | Campos sugeridos |
| **Livros do autor** | Ficha do autor | Lista de obras |
| **Perfil do autor** | Ficha do autor | Perfil/biografia |
| **Chat cloud** | `/ai/openrouter-chat` | Conversa |
| **Chat local** | `/ai/chat` | Conversa |

### 8.3 O chip do modelo {#83-o-chip-do-modelo}

Sempre que uma operação de IA termina, pode aparecer um **chip** (etiqueta pequena) com o modelo usado, por exemplo:

```
⊘ OpenRouter · llama-3.3-70b
```

**Significados:**

- **OpenRouter · nome-do-modelo** — a cloud respondeu com esse modelo.
- **Ollama · nome-do-modelo** — o modelo local respondeu.
- **Automático** — o router da cloud escolheu o modelo por si (não foi pedido um modelo específico).
- **Modelo em tentativa** (no `title`) — aparece quando a operação foi **cancelada**; indica qual estava a ser usado.

**Porque é útil:** como a aplicação tenta vários modelos e pode cair para o local, o modelo real não é previsível. O chip dá-lhe a **proveniência** da resposta.

> 💡 Na tradução, o chip aparece **por baixo do botão Traduzir**, para não competir com a mensagem de estado (cancelamento ou confiança).

### 8.4 Traduzir texto

**Onde encontrar:** formulários de livro (sinopse), ficha de autor (biografia) e ferramenta de tradução.

**Passo a passo:**

1. Abra o diálogo de tradução.
2. Escolha o idioma de **origem** e de **destino**.
3. Clique em **Traduzir**.
4. Durante o processamento, aparece uma mensagem de progresso que já **nomeia o modelo** em uso.
5. Se demorar demasiado, clique em **Cancelar**.
6. Com sucesso, reveja o texto e clique em **Confirmar**.

**O botão Cancelar:**

- A interface para **imediatamente**.
- O modelo local pode continuar a processar em segundo plano, mas o resultado é **descartado**.
- Aparece a mensagem **“Cancelado pelo utilizador”** no lado direito.
- **Confirmar** fica desativado (não há texto para aplicar).

**Fechar o diálogo:**

- Cancela silenciosamente qualquer tradução em curso.
- Uma resposta tardia **não** sobrescreve o campo.

**Dicas:**

- Traduza textos já revistos, para evitar traduzir erros.
- Se a tradução falhar, tente de novo — a aplicação pode escolher outro modelo.
- Em textos longos, o cancelamento é mais rápido do que esperar pelo timeout.

### 8.5 Resumo de publicação (IA)

**Como usar:**

1. Abra/edite a publicação.
2. Clique em **Resumo IA**.
3. A aplicação tenta a cloud e, se necessário, o local.
4. O resumo é colocado no campo **Sinopse**.
5. **Reveja e edite** antes de guardar.

**Boas práticas:**

- Confirme que o **título** e o **autor** estão corretos — são a base do resumo.
- Se já existir sinopse, a IA pode complementar ou substituir; confirme o comportamento no diálogo.
- Resumos gerados podem conter imprecisões: valide factos.

### 8.6 Ano de edição por IA

1. Abra a publicação.
2. Use a ação **Ano de edição IA**.
3. A aplicação sugere um ano.
4. Aceite ou corrija manualmente.

> ⚠️ A IA pode errar o ano. Confirme com uma fonte (ISBN, editora, Google Books).

### 8.7 Notas e metadados de média

**Notas:**

1. Abra o registo de média.
2. Clique em **Notas IA**.
3. Reveja o texto gerado.
4. Guarde.

**Metadados:**

1. Abra o registo.
2. Clique em **Metadados IA**.
3. A IA sugere campos (género, ano, artista…).
4. Aceite os que fizerem sentido.

> 💡 Para YouTube, inclua o título/artista correto para melhorar as sugestões.

### 8.8 Livros do autor (IA)

1. Abra a ficha do autor.
2. Use **Livros do Autor** / geração por IA.
3. A aplicação tenta obter ou gerar a lista de obras.
4. Reveja e associe/crie registos.

**Atenção:** listas geradas por IA podem incluir obras inexistentes ou faltar obras reais. Valide antes de criar registos em massa.

### 8.9 Perfil do autor (IA)

1. Abra a ficha do autor.
2. Clique em **Gerar perfil**.
3. A aplicação usa primeiro a cloud e depois o local.
4. O perfil é apresentado para revisão.
5. Edite e guarde.

> ⚠️ Verifique datas, prémios e obras — são os pontos onde a IA mais erra.

### 8.10 Chat cloud (OpenRouter)

Página: **AI → Chat** (`/ai/openrouter-chat`).

**Características:**

- Streaming em tempo real.
- Sem fallback local: se a cloud falhar, mostra erro amigável.
- Histórico de conversas.
- Avaliação 👍/👎 e cópia.
- Bloco de **raciocínio** colapsável (quando o modelo o devolve).

**Enviar uma mensagem:**

1. Escreva na caixa de texto.
2. Pressione **Enter** (ou o botão de enviar).
3. A resposta aparece progressivamente.
4. Pode **parar** a geração, se disponível.

**Raciocínio (“💭 Raciocínio”):**

- Aparece quando o modelo devolve raciocínio e a opção está ligada.
- Está **colapsado por exemplo**; expanda por mensagem.
- A escolha (aberto/fechado) é respeitada e guardada com a conversa.

**Histórico:**

- Abra o painel **Histórico**.
- Selecione uma conversa para retomar.
- As conversas ficam associadas ao utilizador.

### 8.11 Chat local (Ollama)

Página: **AI → Chat (Ollama)** (`/ai/chat`).

**Diferenças face ao chat cloud:**

- Corre **inteiramente na máquina** (Ollama).
- Pode funcionar sem internet.
- Velocidade depende do hardware/modelo.
- Também suporta histórico, 👍/👎, cópia e raciocínio.

**Modelos com raciocínio:**

- Modelos como Qwen3, DeepSeek-R1 e semelhantes podem devolver um bloco de “pensamento”.
- Com a opção de raciocínio **ligada**, o bloco aparece separado da resposta.
- Com a opção **desligada**, o bloco é descartado e **nunca** aparece na resposta.
- Alguns modelos enviam o pensamento “inline” (dentro do texto, entre marcadores); a aplicação separa-o corretamente e não deixa marcadores visíveis.

**Resposta vazia:**

- Se o modelo não produzir texto, a aplicação mostra uma mensagem de aviso (não uma resposta em branco).
- Tente reformular a pergunta ou escolher outro modelo nas Definições.

### 8.12 Histórico e apagar conversas

O painel **Histórico** é comum aos dois chats.

**Ações:**

| Ação | Efeito |
|---|---|
| **Nova conversa** | Começa uma conversa limpa |
| **Selecionar conversa** | Retoma o contexto |
| **Apagar histórico** | Elimina as conversas (com confirmação) |

**Apagar histórico:**

1. Abra **Histórico**.
2. Clique em **Apagar histórico** (botão de perigo, à direita).
3. Confirme no diálogo, que indica **quantas conversas** serão apagadas e avisa que é **irreversível**.
4. A lista fica vazia e a conversa atual é reiniciada.

**Notas importantes:**

- A eliminação é feita **conversa a conversa**.
- Para um **Admin**, o histórico apagado inclui o de **todos os utilizadores**.
- A tabela de conversas não distingue a interface: apaga o histórico **dos dois chats**.
- As avaliações 👍/👎 associadas ficam órfãs e são ignoradas (não há erro).
- Os botões ficam **desativados** quando a lista está vazia.

> ⚠️ Não há “recuperar” depois de apagar. Exporte o que precisar antes.

### 8.13 Avaliar respostas (👍/👎) e copiar

**Avaliar:**

1. Em cada resposta do assistente, clique em **👍** (gosto) ou **👎** (não gosto).
2. Clicar novamente no mesmo botão **remove** a avaliação.
3. A etiqueta de estado mostra **Aprovo / Não aprovo / Não votado**.
4. Ao reabrir a conversa, a avaliação é restaurada.

**Copiar:**

1. Clique no ícone de **copiar**.
2. O texto da resposta é copiado para a área de transferência.
3. Cole onde precisar (Ctrl+V).

> 💡 As avaliações servem para medir a qualidade das respostas e são guardadas por conversa/mensagem.

### 8.14 Limites de modelos gratuitos

- A aplicação usa, por predefinição, **modelos gratuitos**.
- Existe um **limite diário** de pedidos gratuitos (configurável pelo Admin).
- Quando o limite se esgota:
  - As operações **cloud** devolvem “sem resposta” e a app **cai para o Ollama** (quando aplicável).
  - O **chat cloud** mostra uma mensagem amigável (não tem fallback local).

**Como saber:** consulte a secção **IA em uso interno** no Dashboard e os cartões de uso nas Definições.

> 💡 Se precisar de mais capacidade, o Admin pode ativar **modelos pagos** ou aumentar o limite.

### 8.15 Artefactos de segurança dos modelos

Alguns modelos gratuitos devolvem, por vezes, apenas uma linha de classificação de segurança (ex.: “User Safety: safe;”) em vez de uma resposta.

**O que a aplicação faz:**

- **Remove** essas linhas de respostas válidas.
- Se a resposta for **só** o artefacto, substitui por uma mensagem que pede para responder **“continua”**.

**O que deve fazer:** responder **“continua”** ou reformular a pergunta.

### 8.16 Editor de Prompts

Página: **AI → Editor de Prompts** (`/edit-prompts`).

**O que permite:**

- Consultar e editar os **templates de prompt** usados pela IA.
- Ver a descrição de cada template.
- Guardar alterações (com validação).

**Recomendações:**

- Altere um prompt de cada vez.
- Mantenha o foco e o formato pedido (ex.: “responde apenas com o texto traduzido”).
- Teste numa operação simples antes de usar em massa.
- Em caso de erro, reverta para a versão anterior.

> ⚠️ Prompts mal editados degradam **todas** as operações que os usam. Se não souber o efeito, não altere.

### 8.17 Configurar a IA (visão de Admin)

A configuração vive na área **IA** das Definições (ver [10.3](#103-área-ia)). Inclui:

- **Definições comuns** — ligar/desligar raciocínio.
- **OpenRouter** — chave de API, modelo predefinido, permitir modelos pagos, usar cloud para publicações de autor, limite diário, cartões de uso.
- **Ollama (local)** — chave (se aplicável), endereço do modelo, nome do modelo.

> 💡 Sem chave OpenRouter, a aplicação funciona apenas com o Ollama local.

---

## 9. Tabelas de Apoio {#9-tabelas-de-apoio}

As tabelas de apoio (ou “auxiliares”) alimentam os seletores e os gráficos. Sem elas, os formulários ficam pobres e as estatísticas incompletas.

Página: **Manutenção → Tabelas Auxiliares** (`/admin/managelookuptables`). Existe também uma versão nova em `/admin/managelookuptables-new`, com o mesmo conteúdo e layout diferente.

### 9.1 Categorias de publicações

- **Para que serve:** classificar publicações (ex.: Romance, Técnico, História).
- **Campos:** nome/descrição.
- **Impacto:** usado no formulário de publicação e no gráfico de categorias.

### 9.2 Categorias de média

- **Para que serve:** classificar áudio/vídeo (ex.: Música, Documentário, Podcast).
- **Campos:** nome/descrição (com detalhe próprio).
- **Impacto:** formulário de média e gráfico de categorias de média.

### 9.3 Formatos de publicações

- **Para que serve:** formato físico/digital da publicação (ex.: Capa dura, Brochado, eBook, PDF).
- **Impacto:** formulário de publicação e gráfico de formatos.

### 9.4 Formatos de média

- **Para que serve:** formato do ficheiro de média (ex.: MP3, MP4, FLAC).
- **Impacto:** formulário de média e gráfico de formatos de média.

### 9.5 Editoras

- **Para que serve:** editora da publicação.
- **Campos:** nome e dados de contacto (quando aplicável).
- **Impacto:** formulário de publicação, pesquisa Google Books e gráfico de editoras.

### 9.6 Locais de arquivo

- **Para que serve:** indicar onde o exemplar/ficheiro está guardado (ex.: Estante A, Pasta PDFs, Nuvem).
- **Impacto:** formulário de publicação e diagnóstico de caminhos.

### 9.7 Modelos de IA

- **Para que serve:** catálogo de modelos disponíveis para o Ollama local (e referência de configuração).
- **Campos:** nome do modelo, endereço/base, parâmetros.
- **Impacto:** escolha do modelo local nas Definições e no chat local.

### 9.8 Regras de manutenção das tabelas

| Regra | Porquê |
|---|---|
| Não apagar valores **em uso** | Pode quebrar associações e deixar registos “órfãos” |
| Preferir **editar** a apagar+recriar | Mantém as associações existentes |
| Evitar duplicados | Confunde gráficos e pesquisas |
| Nomes curtos e consistentes | Melhora os filtros e a leitura |
| Rever após importações em massa | Dados importados podem criar duplicados |

> 💡 Se uma categoria/editora estiver bloqueada para eliminação, é porque existem registos a usá-la. Reassocie primeiro.

### 9.9 Versão clássica vs. versão nova

Ambas fazem o mesmo:

- **Clássica** (`/admin/managelookuptables`) — separadores (tabs).
- **Nova** (`/admin/managelookuptables-new`) — barra lateral com grupos (Publicações, Média, IA) e cartões.

Use a que preferir; os dados são os mesmos.

---

## 10. Administração {#10-administração}

> As secções seguintes aplicam-se a utilizadores com papel **Admin**. Um GeneralUser não vê estes itens no menu.

### 10.1 Utilizadores

Página: **Manutenção → Utilizadores** (`/admin/manageusers`).

**Lista de utilizadores:** mostra utilizador, papel e ações.

**Criar um utilizador:**

1. Clique em **Novo**.
2. Preencha nome, email e password.
3. Escolha o **papel**: **Admin** ou **GeneralUser**.
4. **Guarde**.

**Editar:**

1. Clique em **Editar** na linha.
2. Altere os dados/papel.
3. **Guarde**.

**Apagar:**

1. Clique em **Apagar**.
2. Confirme.

> ⚠️ Apagar um utilizador pode remover/ocultar os dados associados. Confirme o impacto antes. Evite apagar o último Admin.

**Boas práticas:**

- Um Admin por pessoa, não uma conta partilhada.
- GeneralUser para uso diário.
- Desativar/remover contas inativas.
- Documentar quem tem privilégios.

### 10.2 Definições — visão geral

Página: **Definições** (`/admin/appsettings2`, no rodapé do menu). Existe também a versão clássica em `/admin/appsettings`.

A versão nova organiza tudo numa barra lateral com ícones e cartões à direita. As secções são:

| Secção | Conteúdo |
|---|---|
| **Downloads** | Pasta de downloads de média |
| **Cópias de segurança** | Pasta de backups de uploads |
| **Armazenamento** | Pastas de listagens, imagens da app, capas de publicações, imagens de autores |
| **Imagens** | Configurações de imagens |
| **Contactos** | Email de suporte |
| **Outras definições** | Tempo de notificação, organizar ficheiros, remover imagem após edição |
| **Idioma** | Idioma predefinido |
| **IA** | Definições comuns, OpenRouter, Ollama (ver 10.3) |
| **WebApi** | Endereço da API |
| **Serviços** | Configuração de serviços |
| **Base de Dados** | Ligação/estado |

> 💡 Alterações nas pastas afetam **onde os ficheiros são lidos/escritos**. Mude com cuidado e confirme que as pastas existem.

### 10.3 Área IA {#103-área-ia}

A área **IA** tem três subsecções:

#### Definições comuns

- **Raciocínio** (ligar/desligar) — comum aos dois chats (cloud e local). Quando ligado, o raciocínio dos modelos é mostrado no bloco “💭 Raciocínio”.

#### OpenRouter

| Campo | Para que serve |
|---|---|
| **Chave de API** | Autentica na cloud |
| **Modelo predefinido** | Modelo a usar por omissão |
| **Permitir modelos pagos** | Se desligado, só usa modelos gratuitos |
| **Usar cloud para publicações de autor** | Liga a geração de publicações de autor na cloud |
| **Limite diário gratuito** | Máximo de pedidos gratuitos por dia |
| **Cartões de uso** | Consumo atual e histórico |

**Notas:**

- Sem chave, a cloud não funciona e tudo cai para o Ollama.
- Com **modelos pagos desligados**, a aplicação **não** usa modelos pagos: escolhe equivalentes gratuitos ou o router gratuito.
- O limite é **aplicado no servidor**; quando esgota, as operações cloud devolvem “sem resposta” e a app usa o local.

#### Ollama (local)

| Campo | Para que serve |
|---|---|
| **Chave de API** | Se o seu Ollama a exigir |
| **Endereço base** | Onde o Ollama está a correr |
| **Nome do modelo** | Modelo local a usar |

**Notas:**

- O Ollama tem de estar instalado e a correr na máquina (ou acessível no endereço indicado).
- Modelos maiores dão melhores resultados, mas exigem mais memória e são mais lentos.

### 10.4 Logs

Página: **Logs** (`/admin/viewapplogs`).

**O que mostra:** registos da aplicação (data, nível, mensagem), incluindo erros.

**Como usar:**

1. Abra **Logs**.
2. Ordene por data (mais recente primeiro).
3. Filtre por nível (erro, aviso, informação).
4. Use a mensagem para diagnosticar.

> 💡 O Dashboard mostra apenas os erros recentes; os Logs têm o histórico.

### 10.5 Gestor de ficheiros

Página: **Gestor de Ficheiros** (`/admin/filemanager`).

**O que permite:**

- Navegar pelas pastas de uploads/downloads.
- Ver ficheiros e tamanhos.
- **Carregar** ficheiros.
- **Descarregar** ficheiros.
- **Criar pastas**.
- **Apagar** ficheiros/pastas (com confirmação).

**Cuidados:**

- Apagar ficheiros referenciados por registos deixa-os sem conteúdo.
- Não mova ficheiros “à mão” sem atualizar os caminhos na aplicação.
- Prefira esta ferramenta a mexer diretamente no sistema de ficheiros.

### 10.6 Backups

#### Base de dados

Página: **Backup → Base de Dados** (`/backupdatabase`).

1. Abra a página.
2. Clique no botão de **backup**.
3. Confirme a operação.
4. Aguarde a mensagem de conclusão.

**O que copia:** a base de dados (registos, utilizadores, configurações, conversas, avaliações).

**O que não copia:** os ficheiros (PDFs, capas, média).

#### Uploads

Três páginas dedicadas:

| Página | Conteúdo | Ficheiro gerado |
|---|---|---|
| `/backupuploads/1` | Uploads de **publicações** (PDFs e capas) | `PdfsAndCovers` (ZIP) |
| `/backupuploads/2` | Uploads de **média** | `MediaFiles` (ZIP) |
| `/backupuploads/3` | Uploads de **imagens** | `AllAppImages` (ZIP) |

**Como usar:**

1. Abra a página do tipo pretendido.
2. Inicie o backup.
3. Aguarde a conclusão.
4. Guarde o ficheiro resultante num local seguro.

> ⚠️ Um backup completo = **base de dados + uploads**. Fazer só um dos dois deixa o sistema inconsistente em caso de restauro.

### 10.7 Configuração de tabelas da aplicação

A aplicação inclui páginas auxiliares de configuração (`/admin/configapptables`, `/admin/adminpanel`) para cenários avançados. **Não são necessárias para o uso normal.** Altere apenas se souber o efeito.

### 10.8 Boas práticas de administração

- Faça backups **regulares** (BD + uploads).
- Teste o restauro numa cópia antes de precisar dele.
- Mantenha a chave OpenRouter privada.
- Reveja os logs semanalmente.
- Limpe tabelas de apoio não usadas com cuidado.
- Documente alterações de configuração.
- Mantenha o Ollama e os modelos atualizados.

---

## 11. Relatórios {#11-relatórios}

A aplicação gera relatórios a partir de modelos (RDLC) e apresenta-os na própria interface.

### 11.1 Relatório de autores

Página: **`/AuthorsPreview_Report`**.

**Como usar:**

1. Abra a página de relatório.
2. Escolha/escreva os parâmetros (por exemplo, autor ou intervalo).
3. Clique em **Pré-visualizar**.
4. O relatório é gerado e mostrado.
5. Use as opções de **exportação** disponíveis (PDF/Excel, conforme o modelo).

**O que esperar:**

- O relatório usa os dados a que tem acesso (isolamento aplicado).
- Relatórios grandes podem demorar.
- Se não houver dados, o relatório pode aparecer vazio.

### 11.2 Resolução de problemas de relatórios

| Sintoma | Causa provável | Solução |
|---|---|---|
| Relatório vazio | Sem dados para os parâmetros | Alargar o intervalo/limpar filtros |
| Erro ao gerar | Modelo em falta ou dados inválidos | Verificar Logs; contactar Admin |
| Exportação falha | Permissões ou formato não suportado | Tentar outro formato; verificar pasta |
| Demora excessiva | Muitos registos | Restringir parâmetros |

> 💡 Os modelos de relatório são mantidos pelo Admin. Se um relatório estiver sistematicamente errado, é aí que se corrige.

---

## 12. Ferramentas {#12-ferramentas}

### 12.1 Tradutor

O tradutor aparece como **diálogo** em vários pontos da aplicação e como ferramenta autónoma.

**Funcionalidades:**

- Escolha de idioma de origem/destino.
- Progresso com **nome do modelo**.
- **Cancelar** (aborta a UI imediatamente).
- **Confirmar** (aplica o texto).
- **Chip** com o modelo que respondeu.

**Boas práticas:**

- Traduza frases completas, não fragmentos.
- Reveja nomes próprios e termos técnicos.
- Se cancelar, o texto original mantém-se intacto.

### 12.2 Benchmark de IA

A aplicação recolhe métricas de desempenho dos modelos (latência, tokens/segundo, uso) e apresenta-as no Dashboard (secção **Atividade**) e nas Definições (cartões de uso).

**Para que serve:**

- Comparar modelos.
- Detetar degradação de desempenho.
- Apoiar a escolha do modelo predefinido.

> 💡 O benchmark é uma ferramenta de **observação**; não altera o comportamento da app.

### 12.3 Chat como ferramenta de trabalho

Além de conversa, os chats podem ajudar em tarefas do dia-a-dia:

- Redigir sinopses.
- Sugerir categorias.
- Traduzir excertos.
- Resumir textos colados.
- Corrigir ortografia.

> 💡 Para tarefas estruturadas, use as funções dedicadas (Resumo, Traduzir, Metadados) — os resultados são aplicados diretamente nos campos.

### 12.4 Receitas e conteúdos auxiliares

Existem páginas auxiliares de receitas (`/recipes`, `/recipe_cards`) usadas como conteúdo de demonstração/exemplo. Não fazem parte do fluxo principal de catalogação.

---

## 13. Dicas e Atalhos {#13-dicas-e-atalhos}

### 13.1 Produtividade

- **Enter** no chat envia a mensagem.
- **Ctrl+V** cola texto/imagens nos campos que o suportam.
- Use a **pesquisa dos seletores** (autor, editora, categoria) em vez de percorrer listas longas.
- Ordene/filtre a grelha antes de exportar ou reportar.
- Mantenha uma **conversa por tema** no chat, para o histórico ser útil.

### 13.2 Qualidade de dados

- Preencha sempre **Título**, **Autor** e **Categoria**.
- Use **ISBN** para desambiguar.
- Evite duplicados: pesquise antes de criar.
- Consistência de nomes (ex.: “J. R. R. Tolkien”, não três variantes).
- Reveja os indicadores de **Saúde** do Dashboard.

### 13.3 Ficheiros e pastas

- Nomes sem caracteres problemáticos (`/ \ : * ? " < > |`).
- Uma pasta por tipo (PDFs, capas, áudio, vídeo).
- Backups antes de grandes importações.
- Não mova ficheiros fora da aplicação sem atualizar caminhos.

### 13.4 IA

- Reveja **sempre** o que a IA gera.
- Prefira operações dedicadas para resultados estruturados.
- Se a cloud estiver lenta/indisponível, a app usa o local automaticamente.
- Em modelos gratuitos, o artefacto “User Safety” pode aparecer — responda “continua”.
- Se a resposta vier vazia, reformule ou mude de modelo.

### 13.5 Segurança

- Não partilhe a chave OpenRouter.
- Termine sessão em computadores partilhados.
- Use passwords fortes e únicas.
- Não use a conta Admin para tarefas do dia-a-dia.
- Desconfie de prompts/ficheiros que peçam dados sensíveis.

### 13.6 Tema e acessibilidade

- O tema claro é melhor para ambientes iluminados; o escuro para ambientes com pouca luz.
- A sidebar/barra superior escuras são intencionais e não mudam com o tema.
- Use o zoom do browser (Ctrl + / Ctrl -) se o texto for pequeno.
- Os contrastes principais cumprem normas de acessibilidade (WCAG AA).

### 13.7 Tabela de ações rápidas

| Quero… | Ação |
|---|---|
| Abrir uma publicação rapidamente | Pesquisar na grelha de publicações |
| Abrir este manual | Botão de ajuda (`?`) na barra superior |
| Trocar idioma | Definições → Idioma (a barra superior só mostra o idioma atual) |
| Trocar tema | Ícone sol/lua |
| Cancelar uma tradução | Botão **Cancelar** no diálogo |
| Copiar resposta da IA | Ícone de copiar na mensagem |
| Avaliar resposta | 👍 / 👎 na mensagem |
| Apagar histórico | Histórico → Apagar histórico |
| Ver erros recentes | Dashboard → Auditoria / Logs |
| Fazer backup | Backup → Base de Dados + Uploads |

---

## 14. Resolução de Problemas {#14-resolução-de-problemas}

Esta secção cobre os problemas mais comuns. Os sintomas estão descritos como o utilizador os vê.

### 14.1 Autenticação e sessão

| Sintoma | Causa provável | Solução |
|---|---|---|
| “Credenciais inválidas” | Email/password errados | Confirmar maiúsculas e teclado; recuperar password |
| Sessão expira rapidamente | Política de segurança | Voltar a entrar; contactar Admin |
| Redireciona para login em todas as páginas | Cookie/sessão inválida | Limpar cookies do site; voltar a entrar |
| Não recebo email de recuperação | SMTP não configurado/spam | Verificar spam; contactar Admin |

### 14.2 Dados não aparecem

| Sintoma | Causa provável | Solução |
|---|---|---|
| Publicações/autores “desapareceram” | Conta errada (isolamento por utilizador) | Confirmar utilizador; pedir a um Admin para verificar |
| Grelha vazia mas existem registos | Filtro/pesquisa ativa | Limpar filtros e pesquisa |
| Gráficos vazios | Campos de classificação não preenchidos | Preencher categoria/formato/editora |
| Registos de outro utilizador invisíveis | Comportamento esperado para GeneralUser | Pedir acesso Admin, se necessário |

### 14.3 IA sem resposta

| Sintoma | Causa provável | Solução |
|---|---|---|
| Tradução/resumo não devolve nada | Cloud indisponível e Ollama parado | Verificar Ollama; tentar de novo |
| “Limite diário atingido” | Cap de modelos gratuitos | Esperar pelo dia seguinte ou pedir aumento/ modelos pagos ao Admin |
| Chat cloud com erro | Sem chave/limite/instabilidade | Verificar chave nas Definições; tentar mais tarde; usar chat local |
| Resposta vazia | Modelo sem conteúdo | Reformular; escolher outro modelo |
| “User Safety: safe;” | Artefacto do modelo | Responder “continua” |
| Tradução muito lenta | Modelo local grande | Cancelar; usar cloud ou modelo menor |
| Raciocínio não aparece | Opção desligada | Ligar “Raciocínio” nas Definições comuns |
| Modelo inesperado | Sweep/fallback automático | Ver chip do modelo; fixar modelo predefinido nas Definições |

### 14.4 Ficheiros

| Sintoma | Causa provável | Solução |
|---|---|---|
| “Ficheiro não encontrado” | Ficheiro movido/apagado | Página `/pubsnotfoundondisk`; corrigir caminho ou recarregar |
| Caminho errado | Pasta mudou | Página `/pubswithwrongpath`; atualizar local |
| Capa não aparece | Imagem em falta/formato inválido | Recarregar imagem |
| Upload falha | Tamanho/formato/permissões | Reduzir ficheiro; verificar formato; ver Logs |
| Player não reproduz | Codec/ficheiro inválido | Converter para formato suportado |

### 14.5 Relatórios

| Sintoma | Causa provável | Solução |
|---|---|---|
| Relatório vazio | Sem dados/filtros | Alargar critérios |
| Erro de geração | Modelo/dados | Ver Logs; contactar Admin |
| Exportação falha | Permissões/pasta | Tentar outro formato/local |

### 14.6 Ambiente local (técnico, para Admin)

| Sintoma | Causa provável | Solução |
|---|---|---|
| Página não carrega / ligação recusada | API ou interface paradas | Arrancar ambos os processos |
| Conflito na porta 5001 | API e interface usam o mesmo porto por omissão | Usar perfis/portos diferentes (ex.: API 4100, interface 4110) |
| Erro de certificado no browser | Certificado auto-assinado | Aceitar o certificado; não ligar HSTS até haver certificado de confiança |
| `dotnet build` falha com MSB3027/MSB3021 | Aplicação em execução bloqueia a DLL | Parar a aplicação e voltar a compilar |
| Restore falha | Fonte NuGet/Syncfusion | Verificar configuração da fonte e chave |
| Login SQL falha com `Trusted_Connection` | App pool com nome diferente do login | Alinhar nome do app pool com o login SQL |

> 💡 Os detalhes de deploy, portos e SQL estão documentados nas notas técnicas do projeto (`AGENTS.md`), não neste manual de utilização.

### 14.7 Quando pedir ajuda

Recolha e envie:

1. **O que estava a fazer** (página e ação).
2. **O que esperava** e **o que aconteceu**.
3. **Mensagem de erro** (texto ou captura).
4. **Data/hora** aproximada.
5. **Utilizador** (sem enviar passwords).
6. Se possível, os **Logs** correspondentes.

> ⚠️ Nunca envie passwords nem a chave OpenRouter em capturas de ecrã.

---

## 15. Glossário {#15-glossário}

| Termo | Significado |
|---|---|
| **Admin** | Utilizador com acesso total, incluindo administração |
| **GeneralUser** | Utilizador normal; vê apenas os seus dados |
| **Publicação** | Registo de uma publicação/documento |
| **Média** | Registo de áudio, vídeo ou YouTube |
| **Playlist** | Coleção ordenada de itens de média |
| **Tabela de apoio / auxiliar** | Lista de valores usada por seletores (categorias, formatos, editoras…) |
| **Lookup** | Sinónimo de tabela de apoio |
| **OpenRouter** | Serviço cloud que dá acesso a vários modelos de IA |
| **Ollama** | Serviço local que corre modelos de IA na máquina |
| **Fallback** | Mudança automática para o fornecedor alternativo quando o principal falha |
| **Cloud** | Fornecedor remoto (OpenRouter) |
| **Local** | Fornecedor na máquina (Ollama) |
| **Modelo** | Motor de IA (ex.: llama-3.3-70b) |
| **Chip do modelo** | Etiqueta que mostra qual modelo respondeu |
| **Prompt** | Instrução/texto que orienta a IA |
| **Template de prompt** | Prompt reutilizável, editável no Editor de Prompts |
| **Streaming** | Resposta que aparece progressivamente |
| **Raciocínio** | “Pensamento” interno de alguns modelos, mostrado separado da resposta |
| **Histórico** | Lista de conversas guardadas |
| **Rating** | Classificação pessoal (estrelas) |
| **Sinopse** | Resumo/descrição de uma publicação |
| **ISBN** | Identificador internacional de publicação |
| **RDLC** | Formato de modelo de relatório |
| **Backup** | Cópia de segurança |
| **Log** | Registo de eventos/erros |
| **Isolamento de dados** | Regra que impede um utilizador de ver dados de outro |
| **Tema** | Aparência clara ou escura |
| **Cultura** | Idioma/região da interface (pt, en, fr, es) |
| **Upload** | Carregamento de ficheiros para a aplicação |
| **Download** | Transferência de ficheiros da aplicação |
| **Cap** | Limite máximo (ex.: pedidos gratuitos por dia) |
| **Token** | Unidade de texto usada pelos modelos de IA |
| **Latência** | Tempo de resposta |
| **TPS / Tokens por segundo** | Velocidade de geração do modelo |

---

## 16. Cenários completos (passo a passo) {#16-cenários-completos-passo-a-passo}

Os cenários seguintes juntam várias funcionalidades e servem de “receita” para tarefas reais.

### 16.1 Catalogar uma publicação a partir do ISBN

**Objetivo:** criar um registo completo, com dados oficiais e capa, em poucos minutos.

1. Abra **Publicações → Gestão** (`/publicacoes`).
2. Clique em **Novo**.
3. No campo **ISBN**, introduza o número (com ou sem hífenes).
4. Abra **Pesquisar Online** e pesquise pelo ISBN ou pelo título.
5. Escolha o resultado correto na lista.
6. Confirme os campos preenchidos: **Título**, **Autor**, **Editora**, **Ano**, **Páginas**, **Sinopse**.
7. Se faltar o **Autor**, crie-o primeiro em **Autores** e volte atrás (os dados do formulário perdem-se se sair sem guardar — copie o que já tem).
8. Se faltar **Categoria**, **Formato** ou **Local de arquivo**, escolha uma opção existente (ou peça ao Admin para criar).
9. Se a sinopse vier noutro idioma, use **Traduzir** e confirme.
10. Carregue a **capa** (se a pesquisa não a tiver trazido).
11. Clique em **Guardar**.
12. Volte ao Dashboard e confirme que a publicação aparece nas contagens e que a secção **Saúde** já não a lista como “sem capa/páginas”.

**Erros comuns:**

| Erro | Causa | Correção |
|---|---|---|
| “Autor requerido” | Não selecionou autor | Escolher/criar autor |
| “Categoria requerida” | Não selecionou categoria | Escolher categoria |
| “Ano de Edição inválido” | Ano com menos/menos de 4 dígitos ou futuro | Corrigir para 4 dígitos ≤ ano atual |
| Registo duplicado | ISBN já existente | Pesquisar antes de criar; consolidar |

### 16.2 Importar uma pasta de PDFs em lote

**Objetivo:** criar registos a partir de vários ficheiros e depois corrigir o que faltar.

1. Antes de importar, faça **backup** da base de dados (`/backupdatabase`).
2. Prepare os ficheiros numa pasta, com nomes consistentes (`ISBN - Título.pdf`).
3. Em **Publicações → Gestão**, use **Importar PDF/DOCX** e selecione os ficheiros.
4. Aguarde o processamento (pode demorar).
5. Abra a grelha e ordene por **data** para ver os registos novos.
6. Para cada registo:
   - Confirme **Título** e **Autor**.
   - Complete **Categoria**, **Editora**, **Formato** e **Local de arquivo**.
   - Adicione **capa** e **sinopse** (IA, se necessário).
7. No fim, abra `/pubsnotfoundondisk` e `/pubswithwrongpath` e corrija os caminhos.
8. Reveja a secção **Saúde**: duplicados, sem capa, sem páginas.
9. Faça um novo backup.

**Dicas:**

- Importe em lotes pequenos (10–20) para não perder trabalho se algo falhar.
- Se um ficheiro não for reconhecido, abra-o primeiro e confirme que não está protegido/corrompido.
- Não apague os ficheiros originais até validar os registos.

### 16.3 Preparar uma ficha de autor completa

**Objetivo:** autor com fotografia, biografia revista e perfil gerado por IA.

1. Abra **Publicações → Autores** (`/autores`).
2. **Novo** → preencha **Nome** → **Guardar**.
3. Abra a ficha do autor.
4. Carregue a **fotografia** (retrato, boa resolução).
5. Escreva uma **biografia base** curta (2–4 frases) com factos que conhece.
6. Use **Gerar perfil IA** para completar.
7. **Reveja** o resultado: nomes, datas, obras e prémios.
8. Corrija o que estiver errado e **Guarde**.
9. Abra **Publicações do Autor** e associe/crie as obras.
10. Se a biografia estiver noutro idioma, use **Traduzir** e confirme.

**Atenção:** a IA pode inventar datas e obras. O perfil deve ser validado antes de ser partilhado.

### 16.4 Construir uma musicoteca e uma playlist

**Objetivo:** carregar álbuns, preencher metadados e ouvir numa playlist.

1. Abra **Media → Gestão** (`/media`).
2. Clique em **Upload** e selecione os ficheiros de áudio.
3. Para cada registo criado, abra **Editar** e confirme:
   - **Título**, **Autor/Artista**, **Formato de média**, **Género/Categoria**, **Ano**.
4. Use **Metadados IA** / **Notas IA** para completar o que faltar (reveja sempre).
5. Abra **Media → Playlists** (`/media/playlists`).
6. **Novo** → dê um nome (ex.: “Jazz — noite”) → **Guardar**.
7. Abra a playlist → **Adicionar itens** → selecione as faixas.
8. Reordene, se necessário.
9. Clique em **Reproduzir** para abrir o **Player de Playlist**.
10. Confirme que todas as faixas tocam; corrija as que falharem.

**Dicas:**

- Uma playlist por contexto é mais útil do que uma lista gigante.
- Se uma faixa não tocar, confirme o ficheiro no **Gestor de Ficheiros**.
- Verifique o **Formato de média** correto para os gráficos do Dashboard.

### 16.5 Adicionar e reproduzir um vídeo do YouTube

1. Abra **Media → Gestão**.
2. **Novo** → escolha o tipo **YouTube**.
3. Preencha **Título**, **Autor/Artista**, **Género/Categoria**, **Formato de média**.
4. Cole a **ligação** do vídeo (URL completo).
5. **Guarde**.
6. Na grelha, clique em **▶** para reproduzir.
7. Se o player mostrar erro, confirme:
   - A ligação está correta e o vídeo não é privado.
   - O vídeo não foi removido.
8. Para organizar, adicione o item a uma **playlist** (as playlists podem misturar áudio, vídeo e YouTube, conforme a configuração).

### 16.6 Traduzir e resumir sem perder o original

**Objetivo:** usar a IA mantendo o texto original intacto.

1. Abra a publicação/autor e localize o texto (sinopse/biografia).
2. **Copie o texto original** para um bloco de notas ou para o chat (como referência).
3. Abra **Traduzir**, escolha os idiomas e clique **Traduzir**.
4. Se demorar, clique **Cancelar** — o campo fica intacto.
5. Com sucesso, compare o resultado com o original.
6. Clique **Confirmar** apenas quando estiver satisfeito.
7. Para o resumo, use **Resumo IA** e reveja antes de guardar.
8. Guarde o registo.

> ⚠️ **Confirmar** aplica o texto ao campo. Se quiser manter as duas versões, guarde a original noutro local antes.

### 16.7 Rotina de backup e verificação

**Objetivo:** garantir que consegue recuperar o sistema.

1. Abra `/backupdatabase` e gere o backup da base de dados.
2. Abra `/backupuploads/1`, `/backupuploads/2` e `/backupuploads/3` e gere os backups de uploads.
3. Copie os ficheiros resultantes para **fora da máquina** (disco externo/nuvem).
4. Registe a data e o que foi incluído.
5. Periodicamente, **teste o restauro** numa cópia — um backup não testado não é um backup.
6. Confirme que os caminhos das pastas nas **Definições** continuam corretos após qualquer mudança.

### 16.8 Recuperar de uma mudança de pastas

**Objetivo:** depois de mover ficheiros de sítio, voltar a ter tudo a funcionar.

1. Atualize as pastas nas **Definições** (Armazenamento, Imagens, Downloads, Backups).
2. Abra `/pubsnotfoundondisk` e veja o que ficou sem ficheiro.
3. Abra `/pubswithwrongpath` e corrija os caminhos.
4. No **Gestor de Ficheiros**, confirme que os uploads estão visíveis.
5. Abra algumas publicações e confirme que o **Ler PDF** funciona.
6. Abra as galerias e confirme que as imagens aparecem.
7. Faça um backup novo.

---

## Apêndice A — Rotas principais {#apêndice-a--rotas-principais}

| Rota | Página |
|---|---|
| `/` | Dashboard |
| `/publicacoes` | Gestão de publicações |
| `/imagegallery` | Galeria de imagens |
| `/autores` | Autores |
| `/media` | Gestão de média |
| `/videogallery` | Galeria de vídeos |
| `/media/playlists` | Playlists |
| `/media/playlists/{id}/player` | Player de playlist |
| `/mp3player` | Player MP3 |
| `/youtubeplayer` | Player YouTube |
| `/musicgallery` | Galeria de música |
| `/edit-prompts` | Editor de prompts |
| `/ai/openrouter-chat` | Chat cloud |
| `/ai/chat` | Chat local (Ollama) |
| `/pubsnotfoundondisk` | Publicações sem ficheiro |
| `/pubswithwrongpath` | Publicações com caminho errado |
| `/AuthorsPreview_Report` | Relatório de autores |
| `/admin/manageusers` | Utilizadores |
| `/admin/managelookuptables` | Tabelas auxiliares (clássica) |
| `/admin/managelookuptables-new` | Tabelas auxiliares (nova) |
| `/admin/appsettings` | Definições (clássica) |
| `/admin/appsettings2` | Definições (nova) |
| `/admin/filemanager` | Gestor de ficheiros |
| `/admin/viewapplogs` | Logs |
| `/backupdatabase` | Backup da base de dados |
| `/backupuploads/1` | Backup de uploads de publicações |
| `/backupuploads/2` | Backup de uploads de média |
| `/backupuploads/3` | Backup de uploads de imagens |

## Apêndice B — Checklist de manutenção periódica {#apêndice-b--checklist-de-manutenção-periódica}

**Diária**

- [ ] Verificar erros recentes (Dashboard/Logs).
- [ ] Confirmar que a IA responde (cloud ou local).

**Semanal**

- [ ] Rever indicadores de **Saúde**.
- [ ] Verificar backups recentes.
- [ ] Limpar conversas antigas, se necessário.

**Mensal**

- [ ] Fazer backup completo (BD + uploads) e guardar fora da máquina.
- [ ] Rever utilizadores e papéis.
- [ ] Rever tabelas de apoio (duplicados/órfãos).
- [ ] Atualizar modelos locais e verificar espaço em disco.

**Após importações/migrações**

- [ ] Correr `/pubsnotfoundondisk` e `/pubswithwrongpath`.
- [ ] Confirmar capas e ficheiros.
- [ ] Rever duplicados de títulos/autores.

---

## Apêndice C — Perguntas frequentes (FAQ) {#apêndice-c--perguntas-frequentes-faq}

**P: Porque é que uma publicação que criei não aparece a outro utilizador?**
R: Porque o isolamento de dados é aplicado por utilizador. Só um **Admin** vê os dados de todos. Um GeneralUser vê apenas os seus.

**P: Mudei o idioma, mas as minhas publicações continuam em português. É um erro?**
R: Não. O seletor de idioma traduz a **interface**. O **conteúdo** que introduziu (títulos, sinopses, biografias) não é traduzido automaticamente. Use **Traduzir** nesses campos.

**P: A IA demora muito. O que faço?**
R: Pode **Cancelar** a operação (a interface para imediatamente). Se acontecer frequentemente, provavelmente está a usar o modelo **local** (Ollama); um modelo menor ou a cloud será mais rápido.

**P: A resposta da IA veio vazia.**
R: A aplicação mostra um aviso em vez de uma bolha em branco. Reformule a pergunta, tente novamente ou escolha outro modelo nas Definições.

**P: Apareceu “User Safety: safe;” numa resposta.**
R: É um artefacto de alguns modelos gratuitos. Responda **“continua”** ou reformule a pergunta. A aplicação já remove essas linhas de respostas válidas.

**P: Esgotei o limite diário de IA. E agora?**
R: As operações cloud passam a falhar e a aplicação usa o **Ollama local** (quando disponível). O chat cloud mostra uma mensagem amigável. Aguarde o dia seguinte ou peça ao Admin para rever o limite/modelos pagos.

**P: O chip diz “Automático”. Que modelo respondeu?**
R: Significa que o router da cloud escolheu o modelo. O nome exato pode não ser apresentado nesse caso; o chip indica o fornecedor (OpenRouter) e que a escolha foi automática.

**P: Posso recuperar uma conversa apagada?**
R: Não. O apagar histórico é **irreversível**. As avaliações associadas ficam órfãs e são ignoradas.

**P: Porque é que a sidebar continua escura no tema claro?**
R: É intencional. A sidebar e a barra superior têm uma identidade própria e não mudam com o tema.

**P: Apaguei uma categoria e as publicações ficaram estranhos.**
R: Provavelmente a categoria estava em uso. Prefira **editar** em vez de apagar. Se necessário, recrie a categoria e reatribua as publicações.

**P: O ficheiro PDF da publicação não abre.**
R: Confirme o caminho em `/pubsnotfoundondisk` e `/pubswithwrongpath`; verifique o **Gestor de Ficheiros**; recarregue o ficheiro se necessário.

**P: O relatório vem vazio.**
R: Verifique os parâmetros/filtros e se existem dados no período. Relatórios respeitam o isolamento por utilizador.

**P: Posso usar a aplicação sem internet?**
R: Parcialmente. A aplicação e os dados locais funcionam; a IA **cloud** não. O **Ollama local** permite IA sem internet, desde que esteja a correr.

**P: Onde está a chave OpenRouter?**
R: Nas **Definições → IA → OpenRouter** (apenas Admin). Nunca a partilhe nem a inclua em capturas de ecrã.

**P: Como sei qual fornecedor de IA está a ser usado?**
R: Pelo **chip do modelo** (OpenRouter/Ollama/Automático) e pela secção **IA em uso interno** do Dashboard, que mostra também os fallbacks.

**P: O que é um “fallback”?**
R: É a mudança automática da cloud para o local quando a cloud falha. Muitos fallbacks indicam instabilidade, falta de chave ou limite esgotado.

---

*Fim do manual. Para detalhes técnicos de instalação, deploy e configuração do servidor, consulte a documentação técnica do projeto.*
