# AI Fallback — Troubleshooting (API / PowerShell)

This note documents common issues when testing the AI fallback endpoint.

Endpoint:
- `POST https://localhost:<port>/api/Ai/author-profile` (local fallback)
- `POST https://localhost:<port>/api/Ai/author-profile-cloud` (OpenRouter; returns 503 when the cloud path is unavailable, triggering the fallback)

Headers:
- `ApiKey: <APP_API_KEY>` (this is the key consumed by `ApiKeyMiddleware`)

Body:
- JSON: `{ "name": "José Saramago" }`

---

## 1) 401: "Api Key was not provided."
Cause:
- The request did not include the `ApiKey` header.

Fix:
- Ensure PowerShell uses:

```powershell
$headers = @{ "ApiKey" = "<APP_API_KEY>" }
```

---

## 2) 401: "Unauthorized client."
Cause:
- The `ApiKey` header was sent but does not match the configured value.

Fix:
- Use the app API key value from Web API configuration/user-secrets under key `ApiKey`.
- Do NOT send the OpenAI key in this header.

---

## 3) 500: Unable to resolve `IAiAuthorProfileService`
Symptoms:
- HTTP 500 and Developer Exception Page shows:
  - `Unable to resolve service for type 'MediaOrganizerApp.Application.Interfaces.IAiAuthorProfileService' ...`

Cause:
- Missing DI registration.

Fix:
- Ensure `MediaOrganizerApp.WebApi\Program.cs` includes:

```csharp
builder.Services.AddScoped<IAiAuthorProfileService, AiAuthorProfileService>();
```

Then restart the API.

---

## 4) 400: Bad Request ("Pedido incorreto")
Most common causes:
- Wrong URL path (must be `/api/Ai/author-profile`).
- Body is not valid JSON.
- Request body property is not `name`.

Fix:
- Use exactly:

```powershell
$body = @{ name = "José Saramago" } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$baseUrl/api/Ai/author-profile" -Headers $headers -ContentType "application/json" -Body $body
```

---

## 5) 500 after DI works (OpenAI call failing)
Possible causes:
- Missing/invalid `OpenAI:ApiKey` in Web API configuration.
- Model name invalid.
- Network restrictions.

Fix:
- Confirm Web API user-secrets contains:
  - `OpenAI:ApiKey`
  - (optional) `OpenAI:Model`
- Check the debug output / Developer Exception Page for the actual exception.
