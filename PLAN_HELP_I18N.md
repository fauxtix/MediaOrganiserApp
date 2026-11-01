# PLAN — Manual de Ajuda em 4 idiomas

Objetivo: o botão de ajuda (`?`) da barra superior passa a abrir o manual
correspondente ao idioma ativo da aplicação (pt / en / fr / es), com fallback
para o manual português (`Help.md`) quando a tradução não existir.

## Decisões

- `Web.Blazor/docs/Help.md` continua a ser o manual **português** (predefinido) e o
  fallback universal.
- Novos ficheiros: `Web.Blazor/docs/Help.en.md`, `Help.fr.md`, `Help.es.md`.
- O idioma é resolvido por `CultureInfo.CurrentUICulture.TwoLetterISOLanguageName`
  (mesmo padrão já usado em `GoogleBooksApiSearch`, `EditMedia`, `AuthorBooks`, etc.).
- Se o ficheiro do idioma não existir (ou o idioma não for suportado), usa-se `Help.md`.
- Nenhuma chave de resx nova necessária (nome do ficheiro é interno).

## Progresso

- [ ] Criar `PLAN_HELP_I18N.md` (este ficheiro)
- [ ] Traduzir o manual para inglês → `Web.Blazor/docs/Help.en.md`
- [ ] Traduzir o manual para francês → `Web.Blazor/docs/Help.fr.md`
- [ ] Traduzir o manual para espanhol → `Web.Blazor/docs/Help.es.md`
      (estrutura, `{#anchors}` do índice e rotas mantidas idênticas ao original)
- [ ] `Web.Blazor.csproj`: copiar `docs\Help*.md` para output/publish
      (atualmente só `docs\Help.md` está incluído)
- [ ] `HelpDialog.razor.cs`: resolver `Help.<lang>.md` com fallback para `Help.md`;
      log da variante escolhida; manter leitura do disco a cada abertura
- [ ] Verificar consistência das traduções (nº de secções, anchors e links do índice)
- [ ] `dotnet build Web.Blazor` (ignorar MSB3021/MSB3027 se a app estiver a correr)
- [ ] Atualizar a nota do `HelpDialog` no `AGENTS.md` (regra de documentação viva)
- [ ] Commit e remoção deste ficheiro de plano
