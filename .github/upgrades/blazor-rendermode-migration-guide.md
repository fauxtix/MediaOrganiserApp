# Guia de Migração: Blazor Server (.NET 7) ? Novos Modos de Renderização (.NET 10)

## ?? Visão Geral

Este guia documenta a migração do modelo **Blazor Server puro (.NET 7)** para os **novos modos de renderização do .NET 10**.

---

## ?? Mudanças Necessárias

### 1. **Deletar _Host.cshtml**

? **Remover:** `Pages/_Host.cshtml` e `Pages/_Host.cshtml.cs`

O .NET 10 não usa mais `_Host.cshtml`. A renderização agora é feita diretamente no `App.razor`.

---

### 2. **Atualizar Program.cs**

**Antes (.NET 7 - Blazor Server):**
```csharp
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
```

**Depois (.NET 10 - Novos Modos):**
```csharp
// Adicionar componentes Razor com suporte a modos interativos
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();  // Para Blazor Server (InteractiveServer)
    // .AddInteractiveWebAssemblyComponents(); // Se quiser WASM
    // .AddCircuitOptions(options => { ... }); // Configurações adicionais

// No pipeline
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();  // Habilita InteractiveServer
    // .AddInteractiveWebAssemblyRenderMode(); // Se quiser WASM
    // .AddAdditionalAssemblies(...); // Assemblies adicionais
```

---

### 3. **Atualizar App.razor**

**Antes (.NET 7):**
```razor
<Router AppAssembly="@typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
            <!-- ... -->
        </AuthorizeRouteView>
    </Found>
</Router>
```

**Depois (.NET 10) - Com HeadOutlet:**
```razor
<!DOCTYPE html>
<html lang="pt">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
    <link href="Web.Blazor.styles.css" rel="stylesheet" />
    <link href="css/site.css" rel="stylesheet" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

---

### 4. **Criar Routes.razor (Novo arquivo)**

Criar `Components/Routes.razor`:

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" 
                                DefaultLayout="@typeof(Components.Layout.MainLayout)">
                <NotAuthorized>
                    <div class="col-lg-4">
                        <SfCard>
                            <CardHeader Title="Não autorizado"></CardHeader>
                            <CardContent Content="Verifique as credenciais."></CardContent>
                        </SfCard>
                    </div>
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <PageTitle>Não encontrado</PageTitle>
            <LayoutView Layout="@typeof(Components.Layout.MainLayout)">
                <div class="col-lg-4">
                    <SfCard>
                        <CardHeader Title="Página não encontrada"></CardHeader>
                        <CardContent Content="Verifique o endereço."></CardContent>
                    </SfCard>
                </div>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
}
```

---

### 5. **Atualizar Componentes com Modos de Renderização**

Agora você pode especificar modos de renderização **por componente**:

**Static SSR (sem interatividade):**
```razor
@* Nenhum atributo = Static SSR *@
<MyComponent />
```

**InteractiveServer (Blazor Server com SignalR):**
```razor
@rendermode InteractiveServer
```

**InteractiveAuto (Servidor primeiro, depois WASM):**
```razor
@rendermode InteractiveAuto
```

**Exemplo em MainLayout.razor:**
```razor
@inherits LayoutComponentBase
@rendermode InteractiveServer

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>
    <main>
        @Body
    </main>
</div>
```

---

### 6. **Atualizar _Imports.razor**

Adicionar:
```razor
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
```

---

### 7. **Configurar SignalR (se usar InteractiveServer)**

No `Program.cs`:

```csharp
builder.Services.AddSignalR(e => 
{ 
    e.MaximumReceiveMessageSize = 102400000; 
});

// Ou usar AddCircuitOptions
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });
```

---

## ?? Benefícios da Migração

? **Renderização Streaming** - Conteúdo carrega progressivamente  
? **Melhor SEO** - Static SSR renderiza no servidor  
? **Performance** - Menos dados trafegados  
? **Flexibilidade** - Escolher modo por componente  
? **Híbrido** - Combinar Server + WASM no mesmo app  

---

## ?? Breaking Changes

### **1. HeadOutlet mudou**
```razor
<!-- Antes -->
<component type="typeof(HeadOutlet)" render-mode="ServerPrerendered" />

<!-- Depois -->
<HeadOutlet />
```

### **2. JavaScript mudou**
```html
<!-- Antes -->
<script src="_framework/blazor.server.js"></script>

<!-- Depois -->
<script src="_framework/blazor.web.js"></script>
```

### **3. Não há mais _Host.cshtml**
Todo HTML agora está em `App.razor`.

---

## ?? Comandos de Migração

### Passo 1: Backup
```bash
git checkout -b backup-before-rendermode-migration
git commit -am "Backup before render mode migration"
```

### Passo 2: Deletar arquivos antigos
```bash
rm Pages/_Host.cshtml
rm Pages/_Host.cshtml.cs
```

### Passo 3: Criar novos arquivos
- `App.razor` (atualizar estrutura HTML)
- `Components/Routes.razor` (criar)

### Passo 4: Atualizar Program.cs

### Passo 5: Testar
```bash
dotnet run
```

---

## ?? Recursos

- [Documentação oficial - ASP.NET Core Blazor render modes](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes)
- [Migrating from Blazor Server to .NET 8](https://learn.microsoft.com/aspnet/core/migration/70-to-80)
- [What's new in ASP.NET Core 10.0](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0)

---

## ? Checklist de Migração

- [ ] Deletar `_Host.cshtml` e `_Host.cshtml.cs`
- [ ] Atualizar `Program.cs` com `AddRazorComponents()`
- [ ] Atualizar `App.razor` com estrutura HTML completa
- [ ] Criar `Routes.razor`
- [ ] Atualizar `_Imports.razor`
- [ ] Adicionar `@rendermode` nos componentes interativos
- [ ] Testar autenticação
- [ ] Testar SignalR
- [ ] Verificar JavaScript customizado
- [ ] Atualizar CSS/layouts
- [ ] Rodar testes

---

**Data:** $(Get-Date -Format "yyyy-MM-dd")  
**Versão:** .NET 10.0  
**Status:** ?? Pendente
