# 📦 Media Organizer App — Arquitetura e Visão Geral

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Blazor](https://img.shields.io/badge/Blazor-Server-purple)
![Ollama](https://img.shields.io/badge/LLM-Ollama-orange)
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
<summary>🟦 LLM (Ollama / phi4-mini)</summary>

Usado apenas como fallback ou enriquecimento:

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

## 🔌 Exemplo — chamada ao Ollama

```csharp
var payload = new
{
    model = "phi4-mini",
    prompt,
    stream = false
};

var response = await _httpClient.PostAsJsonAsync("/api/generate", payload);
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

