# PLAN — Corrigir warnings AutoMapper (NU1608 + NU1903) sem licença comercial

## Restrições do utilizador

- **Não usar AutoMapper ≥ 15** (RPL-1.5 / licença comercial). Última versão MIT: **14.0.0**.
- **Pacotes usados em mais de um projeto da solução têm de ter a mesma versão.**

## Diagnóstico

1. **NU1608** — `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1` exige
   `AutoMapper (= 12.0.1)`, mas os hosts resolviam 16.3.0/16.1.1. Esse pacote de DI está
   descontinuado: desde o AutoMapper 13 as extensões `AddAutoMapper` estão no pacote principal.
2. **NU1903** (`GHSA-rvv3-g6hj-g44x` / `CVE-2026-32933`) — DoS por recursão sem limite.
   Vulneráveis: `< 15.1.1` e `>= 16.0.0, < 16.1.1`. Como não queremos a linha comercial,
   não existe versão MIT corrigida (13.x/14.x estão dentro do intervalo vulnerável).

## Alterações

- [x] `Application`: `AutoMapper` `12.0.1` → `14.0.0` (MIT).
- [x] `WebApi`: remover `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1`;
      `AutoMapper` `16.3.0` → `14.0.0`.
- [x] `Web.Blazor`: remover `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1`;
      `AutoMapper` `16.1.1` → `14.0.0`.
- [x] `Program.cs` de WebApi/Web.Blazor: **sem alteração** — a 14.0.0 expõe
      `AddAutoMapper(IServiceCollection, params Type[])` (verificado por reflection).
- [x] `Web.Blazor.Reporting`: `Newtonsoft.Json` `13.0.1` → `13.0.4` (alinhar com EmailService/Web.Blazor).
- [x] `WebApi`: `OpenAI` `2.9.1` → `2.11.0` (alinhar com Web.Blazor).
- [x] Build da solução: 0 erros; NU1608 de AutoMapper desapareceu; 194 testes passam.
- [x] NU1903: **decisão do utilizador — manter o aviso visível** (sem `NuGetAuditSuppress`).
      A 14.0.0 é a última MIT; a correção oficial exige ≥ 15.1.1 (RPL/comercial).
- [x] `Web.Blazor`: `HtmlSanitizer` `9.0.892` → `11.0.16` (resolve NU1608 com AngleSharp 1.10.0;
      traz AngleSharp.Css 1.3.2; API `Ganss.Xss.HtmlSanitizer.Sanitize(...)` inalterada).
- [x] `WebApi`: removida a referência não usada a `Microsoft.CodeAnalysis.Common 5.0.0`
      (resolve os NU1608 com o `Microsoft.CodeAnalysis.CSharp` 4.14.0 do EF Tools).
- [ ] Apagar este ficheiro após commit (nunca antes).

## Fora de âmbito (avisos NU1608 pré-existentes, não AutoMapper)

- `Microsoft.CodeAnalysis.Common 5.0.0` no WebApi vs `CSharp 4.14.0` (transitivo do EF Tools).
  O `Common` parece não ser usado no WebApi — candidato a remoção.
- `AngleSharp 1.10.0` vs `HtmlSanitizer 9.0.892` / `AngleSharp.Css 0.17.0`. Risco real em
  runtime; candidato a subir o `HtmlSanitizer` para uma versão compatível com AngleSharp 1.x.
