# ?? Checklist Completo: Onde Procurar Google Authentication

## ? **Já Verificado e Removido:**
- [x] `appsettings.json` - Seção `Authentication` removida
- [x] `Program.cs` - Código `.AddGoogle()` comentado/removido
- [x] `Web.Blazor.csproj` - Pacotes Google removidos:
  - `Google.Apis.Auth.AspNetCore3`
  - `Microsoft.AspNetCore.Authentication.Google`
- [x] Cache de build limpo (`bin`, `obj`)

---

## ?? **Locais Adicionais para Verificar MANUALMENTE:**

### 1?? **appsettings.Development.json**
**Caminho:** `Web.Blazor\appsettings.Development.json`

Procure por:
```json
"Authentication": {
  "Google": { ... }
}
```

**Ação:** Se existir, DELETAR completamente a seção.

---

### 2?? **User Secrets (Segredos do Usuário)**
**Localização:** `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`

**UserSecretsId:** `aspnet-Web.Blazor-82599089-8959-419C-85C2-F04A9D8841F2` (do .csproj)

**Caminho completo provável:**
```
C:\Users\<SeuUsuario>\AppData\Roaming\Microsoft\UserSecrets\aspnet-Web.Blazor-82599089-8959-419C-85C2-F04A9D8841F2\secrets.json
```

**Ação:** 
1. Abra este arquivo
2. Procure por `"Authentication"` ou `"Google"`
3. DELETAR a seção completamente

**Comando PowerShell para encontrar:**
```powershell
Get-Content "$env:APPDATA\Microsoft\UserSecrets\aspnet-Web.Blazor-82599089-8959-419C-85C2-F04A9D8841F2\secrets.json" -ErrorAction SilentlyContinue
```

---

### 3?? **Areas\Identity\IdentityHostingStartup.cs**
**Caminho:** `Web.Blazor\Areas\Identity\IdentityHostingStartup.cs`

Procure por:
```csharp
.AddGoogle(...)
```

**Ação:** Se existir, comentar ou deletar.

---

### 4?? **Arquivos de Configuração de Identity Personalizados**
Procure em:
- `Areas\Identity\**\*.cs`
- `Identity\**\*.cs`

**Comando para buscar:**
```powershell
Get-ChildItem -Path "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor\Areas\Identity" -Recurse -Filter "*.cs" | Select-String -Pattern "AddGoogle"
```

---

### 5?? **Configuração em Projetos Referenciados**

#### **MediaOrganizerApp.Application**
Verificar se há configuração de autenticação:
```powershell
Get-ChildItem -Path "C:\NewProjects\MediaOrganizer\MediaOrganizerApp\Application" -Recurse -Filter "*.cs" | Select-String -Pattern "AddGoogle"
```

#### **MediaOrganizerApp.Persistence**
```powershell
Get-ChildItem -Path "C:\NewProjects\MediaOrganizer\MediaOrganizerApp\MediaOrganizerApp.Persistence" -Recurse -Filter "*.cs" | Select-String -Pattern "AddGoogle"
```

---

### 6?? **launchSettings.json**
**Caminho:** `Web.Blazor\Properties\launchSettings.json`

Procure por variáveis de ambiente com Google:
```json
"environmentVariables": {
  "Authentication__Google__ClientId": "..."
}
```

**Ação:** Se existir, DELETAR.

---

### 7?? **web.config (se existir)**
**Caminho:** `Web.Blazor\web.config`

Procure por:
```xml
<add key="Authentication:Google:ClientId" value="..." />
```

---

### 8?? **Verificar Todos os appsettings*.json**
Execute:
```powershell
Get-ChildItem -Path "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor" -Filter "appsettings*.json" | ForEach-Object { 
    Write-Host "Arquivo: $($_.Name)"
    Get-Content $_.FullName | Select-String -Pattern "Google|Authentication"
}
```

---

## ?? **SOLUÇÃO NUCLEAR: Desabilitar TODOS os Esquemas Externos**

Se nada funcionar, adicione isto no `Program.cs` **ANTES** de `.AddDefaultUI()`:

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Desabilitar esquemas externos completamente
    options.SignIn.RequireConfirmedAccount = false;
})
.AddDefaultUI()
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<PublicationsContext>();

// REMOVER TODOS OS ESQUEMAS EXTERNOS
builder.Services.Configure<AuthenticationOptions>(options =>
{
    var schemesToRemove = options.Schemes
        .Where(s => s.Name.Contains("Google", StringComparison.OrdinalIgnoreCase))
        .ToList();
    
    foreach (var scheme in schemesToRemove)
    {
        options.RemoveScheme(scheme.Name);
    }
});
```

---

## ?? **Ordem de Verificação Recomendada:**

1. ? User Secrets (mais provável!)
2. ? appsettings.Development.json
3. ? launchSettings.json
4. ? IdentityHostingStartup.cs
5. ? Projetos referenciados

---

## ??? **Comandos Úteis:**

### **Encontrar TODOS os arquivos com "Google":**
```powershell
Get-ChildItem -Path "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor" -Recurse -Include "*.json","*.cs" | Select-String -Pattern "Google" | Select-Object Path, LineNumber, Line
```

### **Limpar User Secrets:**
```powershell
dotnet user-secrets clear --project "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor\Web.Blazor.csproj"
```

### **Listar User Secrets:**
```powershell
dotnet user-secrets list --project "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor\Web.Blazor.csproj"
```

---

## ?? **AVISO:**

Se após verificar TODOS estes locais o erro persistir, o problema pode ser:
1. **Pacote transitivo** (dependência de outro pacote)
2. **Configuração no IIS Express** (se estiver usando)
3. **Configuração global do .NET** no sistema

---

**Data:** $(Get-Date -Format "yyyy-MM-dd HH:mm")
**Status:** Investigação em andamento
