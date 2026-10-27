# PLAN_DEFINICOES_IA.md — Grupo único "IA" nas definições

**Objetivo:** unificar toda a configuração de IA num único local nas duas páginas de administração (`AppSettings.razor` e `AppSettings2.razor`, que têm de permanecer em sincronia), dividido em 3 secções:

- **Comum** (Ollama + OpenRouter): toggle `CloudChatEnableReasoning` + hinja nova.
- **OpenRouter**: chave API, modelo/predefendido, `AllowPaidModels`, `AuthorBooksUseCloud` + hinja, cap gratuito/cards de uso.
- **Ollama (local)**: `ApiKey`, `ModelBaseUrl`, `AiModelName`.

Nada muda no armazenamento (`CloudChat:*`, `settings.ApiKey/ModelBaseUrl/AiModelName`) — apenas markup/resx.

**Checklist de progresso:**

- [x] Resx (4 ficheiros): `TituloIAComum`, `TituloIAOpenRouter`, `TituloIAOllama`, `CloudChatEnableReasoningHint`
- [x] `AppSettings.razor` — tab "Inteligência Artificial" único (remove AI local do "OtherSettings 1" e o tab CloudChat)
- [x] `AppSettings2.razor` — grupo raiz "IA" com folhas `iacomum`/`iaopenrouter`/`iaollama` (remove "ialocal" e "cloud"/"cloudchat")
- [x] Build + testes (125)
- [x] `AGENTS.md` atualizado na mesma passagem
- [ ] Apagar este plano depois do commit