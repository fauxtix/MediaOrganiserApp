# 📦 Media Organizer App — Arquitetura e Visão Geral

![.NET](https://img.shields.io/badge/.NET-net10.0-blue)
![Blazor](https://img.shields.io/badge/Blazor-Server-purple)
![OpenRouter](https://img.shields.io/badge/LLM-OpenRouter--%2B--Ollama-orange)
![Architecture-Data--First](https://img.shields.io/badge/Architecture-Data--First-brightgreen)

---

## 📌 Descrição

Este projeto é uma plataforma de gestão de metadados (livros, música e autores), baseada numa arquitetura **data-first com enriquecimento progressivo**, onde APIs externas, cache, base de dados e LLM trabalham em conjunto de forma hierárquica e controlada.

---

## 🧠 Visão geral da arquitetura

```
Cache (TTL) → APIs externas → Base de Dados → LLM (fallback)
```

---

## 🏗️ Camadas do sistema

<details>
<summary>🥇 Base de Dados (Fonte de verdade)</summary>

- Contém todos os registos finais
- Dados manuais e enriquecidos
- Traduções e metadados consolidados

✔ Fonte oficial do sistema

</details>

<details>
<summary>🥈 APIs externas</summary>

Exemplos:
- Google Books API
- Last.fm API

Função:
- Dados estruturados e verificáveis
- Primeira escolha para enriquecimento

</details>

<details>
<summary>🥉 Cache (TTL)</summary>

- Cache por tipo de entidade
- Reduz chamadas externas
- Suporta modo:
  - cached
  - refresh

```csharp
// Exemplo de TTL
Books: 60 dias
Music: 30 dias
```

</details>

<details>
<summary>🟦 LLM (OpenRouter primário + Ollama local fallback)</summary>

Duas camadas (cloud-first, com fallback local):

- **OpenRouter (cloud)** — provider primário para tradução, notas/metadados/ano de edição de media e author-books
- **Ollama (local)** — fallback quando o cloud está indisponível (offline, sem API key, 429, erro HTTP/parse)

Usado como enriquecimento/fallback (nunca fonte primária de verdade):

- Preenchimento de metadados
- Geração de descrições
- Modo offline (com consentimento)

⚠ Nunca é fonte primária de verdade

</details>

<details>
<summary>🧑 Entrada manual</summary>

- Maior prioridade do sistema
- Override total de APIs e LLM
- Correções diretas no DB

</details>

---

## ⚖️ Hierarquia de confiança

```text
Manual > APIs externas > LLM
```

---

## 🌐 Modo offline

```text
Deteção de falha de rede
        ↓
Notificação ao utilizador
        ↓
Pedido de consentimento
        ↓
┌───────────────┬───────────────┐
│ Aceitar LLM   │ Recusar       │
│ fallback AI   │ sem geração   │
└───────────────┴───────────────┘
```

---

## ⏱️ Estratégia de cache

| Tipo | TTL |
|------|-----|
| Livros | ~60 dias |
| Música | ~30 dias |
| Outros | configurável |

```csharp
// exemplo conceptual
CacheMode = Cached | Refresh
```

---

## 🔁 Fluxo típico de dados

```text
Cache → API → DB → LLM (fallback)
```

---

## 🧠 Papel do LLM

O LLM é usado como:

- enriquecimento de dados
- geração de texto (resumos, descrições)
- fallback offline

### Regras:
- não substitui APIs
- não é fonte de verdade
- pode ser corrigido manualmente

---

## 🧩 Princípios da arquitetura

- Data-first design
- Enriquecimento progressivo
- Separação entre verdade e geração
- TTL para controlo de frescura
- Modo offline controlado pelo utilizador

---

## 💻 Exemplo — Prompt rendering (C#)

```csharp
var prompt = PromptTemplateRenderer.Render(template, new Dictionary<string, string?>
{
    ["sourceLanguage"] = "pt-PT",
    ["targetLanguage"] = "en",
    ["text"] = request.Text
});
```

---

## 🔌 Exemplo — chamada ao LLM

```csharp
// OpenRouter (cloud-first) — os serviços cloud devolvem null em falha
var model = CloudChatSettingsKeys.ApplyPaidModelGate(resolvedModel, allowPaid);
var response = await _chatService.CompleteAsync(model, prompt);

// se null → fallback Ollama local
var result = await _ollamaClient.GenerateAsync(prompt);
```

---

## 🧠 Conclusão

Este sistema é uma arquitetura híbrida onde:

- APIs fornecem dados estruturados
- Cache otimiza performance
- Base de dados consolida o estado final
- LLM é apenas mecanismo auxiliar controlado
- O utilizador mantém controlo total em cenários de incerteza

---

📌 *Media Organizer App — Architecture Documentation*

