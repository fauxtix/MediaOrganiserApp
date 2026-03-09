# ?? Correção do Erro: Google Authentication ClientId

## ? **Erro Original:**
```
ArgumentNullException: Value cannot be null. (Parameter 'ClientId')
```

## ? **Solução Aplicada:**

### **1. Removi a seção Authentication do appsettings.json**

**Antes:**
```json
"Authentication": {
  "Google": {
    "ClientId": "client_id",
    "ClientSecret": "client_secret"
  }
}
```

**Depois:**
```json
// Seção removida temporariamente
```

---

## ?? **Onde Procurar Código Google Auth (Para Comentar Manualmente)**

Se o erro persistir, procure nos seguintes locais e **comente** o código que configura Google:

### **1. Program.cs**
Procure por:
```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        // ...
    });
```

**Comente assim:**
```csharp
// Temporariamente desabilitado para testes
/*
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });
*/
```

### **2. IdentityHostingStartup.cs**
Já verificado - está comentado ?

### **3. Arquivos ServiceExtensions**
Procure por arquivos com nomes como:
- `AuthenticationExtensions.cs`
- `ServiceExtensions.cs`
- `IdentityExtensions.cs`

Verifique se há configuração de Google neles.

---

## ?? **Próximos Passos:**

1. **Salve todos os arquivos** (Ctrl+Shift+S)
2. **Pare a aplicação** (se estiver rodando)
3. **Execute novamente:**
   ```powershell
   cd "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor"
   dotnet run
   ```

4. **Acesse:** `https://localhost:XXXX`

---

## ?? **Se Quiser Habilitar Google Auth Depois:**

### **Passo 1: Obter Credenciais Google**
1. Acesse: https://console.cloud.google.com/
2. Crie um projeto (ou use existente)
3. Vá em "APIs & Services" ? "Credentials"
4. Crie "OAuth 2.0 Client ID"
5. Configure "Authorized redirect URIs":
   - `https://localhost:5001/signin-google`
6. Copie o **Client ID** e **Client Secret**

### **Passo 2: Adicionar no User Secrets**
```powershell
cd "C:\NewProjects\BlazorOrganizer\MyOrganizer\Web.Blazor"
dotnet user-secrets set "Authentication:Google:ClientId" "SEU_CLIENT_ID_AQUI"
dotnet user-secrets set "Authentication:Google:ClientSecret" "SEU_CLIENT_SECRET_AQUI"
```

### **Passo 3: Habilitar no Código**
Descomente o código do Google Authentication no `Program.cs`.

---

## ? **Status Atual:**
- ? Seção Google removida do appsettings.json
- ? Aguardando teste: Execute e veja se carrega agora!

---

**Data:** $(Get-Date -Format "yyyy-MM-dd HH:mm")
