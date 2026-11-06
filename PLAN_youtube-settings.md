# PLAN_youtube-settings (transiente — não commitar)

Objetivo: remover as credenciais do YouTube hardcoded em `MediaOrganizerApp.Persistence/Services/Youtube/Settings/Keys.cs` e passá-las a gerir via `dbo.ApplicationSettings`, expostas na página `Web.Blazor/Pages/Admin/AppSettings2.razor` (secção "Serviços"), seguindo o padrão já usado por LastFm/GoogleBooks/IMDb.

## Checklist de implementação
- [x] 1. Criar script SQL `scripts/AddYouTubeSettings.sql` (colunas `YouTubeApiKey`, `YouTubeClientId`, `YouTubeClientSecret`, `YouTubeApplicationName` + recriar `usp_AppSettings_Update`), idempotente (`COL_LENGTH`).
- [x] 2. Adicionar propriedades à entidade `Domain\Entities\ApplicationSettings.cs` e DTO `Application\DTOs\ApplicationSettingsDto.cs`.
- [x] 3. Adicionar parâmetros a `AppSettingsRepository.UpdateSettingsAsync` (SP) + `GetSettingsAsync` já traz as colunas (SELECT *).
- [x] 4. Adicionar chaves resx (`YouTubeApiKey`, `YouTubeClientId`, `YouTubeClientSecret`, `YouTubeApplicationName`) nos 4 ficheiros `App.*.resx`.
- [x] 5. Adaptar `YouTubeSubscriptionService.cs` a receber as chaves via `IAppSettingsRepository` (sem `Keys`).
- [x] 6. Adaptar `OAuthService.cs` para receber credenciais como parâmetros (sem `Keys`).
- [x] 7. Adaptar `GetYoutubeVideoMetadata.cs` a ler `YouTubeApiKey`/`YouTubeApplicationName` das settings da BD (via `GetKeys()`), remover uso de `Web.Blazor.Static.Keys`.
- [x] 8. Adaptar `ServiceRegistration.cs` (Web.Blazor) a obter a apiKey do `YoutubeService` a partir de `IAppSettingsService.GetSettingsAsync()`.
- [x] 9. Adaptar `AppSettings2.razor` (secção "servicos"): campos YouTube API Key / Client Id / Client Secret / Application Name.
- [x] 10. `AppSettings2.razor.cs`: sem alterações necessárias (bind automático via DTO + `UpdateSettingsAsync` já envia tudo).
- [x] 11. Build: Persistence / WebApi / Web.Blazor compilam com 0 erros.
- [x] 12. Executar `scripts/AddYouTubeSettings.sql` na BD `Publicacoes_Prod` (verificado: 4 colunas + 4 parâmetros no `usp_AppSettings_Update`).
- [x] 13. Confirmar métodos de consulta/gravação: `GetSettingsAsync` usa `SELECT TOP 1 *` + Dapper (mapeia automático); `UpdateSettingsAsync` já tem os 4 parâmetros; AutoMapper `CreateMap<ApplicationSettings, ApplicationSettingsDto>().ReverseMap()` mapeia por convenção.
- [x] 14. Sincronizar `AppSettings.razor` original com os campos YouTube (além do `AppSettings2.razor`).
- [x] 15. Apagar os dois `Keys.cs` (`Persistence/Services/Youtube/Settings/Keys.cs` e `Web.Blazor/Static/Keys.cs`).
- [x] 16. Corrigir `GetYoutubeVideoMetadata.GetKeys()`: deixou de usar o `HttpClient` genérico (sem `BaseAddress`, origem do `HttpClient.PrepareRequestMessage`) e passou a usar `IAppSettingsService` (URL absoluta `BaseUrl + /api/appsettings`).
- [x] 17. `GetYoutubeVideoMetadata`: acrescentado `EnsureYouTubeKeysLoaded()` a `GetAlbumArtistMetadata`, `GetSingleVideoMetadata` e `SearchByArtistAndSong` (antes liam as `Keys` estáticas, agora a key vem da BD).
- [x] 18. `Web.Blazor/ServiceRegistration.cs`: `YoutubeService` passou a typed client (`AddHttpClient<YoutubeService>()`), sem `.GetAwaiter().GetResult()` no registo; a API key é lida em runtime via `IAppSettingsService` com cache.
- [x] 19. `YoutubeService`: `GetPlaylistsAsync` e `FillVideoDurations` passaram a usar `await GetApiKeyAsync()` (já não `_apiKey` síncrono).
- [x] 20. Semear as credenciais na BD (valores do antigo `Keys.cs`): API key, ClientId, ClientSecret e `YouTubeApplicationName = 'MediaOrganizer'`. Corrigido o `ClientSecret` que estava gravado com um `"` à frente.
- [x] 21. Build final Web.Blazor + WebApi: 0 erros.

## Critério de conclusão
- [x] Nenhuma referência a `Keys.YouTubeApiKey` / `Keys.ClientId` / `Keys.ClientSecret` nem `Web.Blazor.Static.Keys` nos consumidores do YouTube.
- [x] As chaves do YouTube são lidas/gravadas em `dbo.ApplicationSettings` e editáveis em `AppSettings2.razor` > Serviços.
- [x] Solução compila sem erros.

## Notas
- `Keys.cs` (Persistence) e `Web.Blazor\Static\Keys.cs` (gitignored) deixaram de ser usados nos caminhos ativos.
- `OAuthService` não tem chamadores no código; assinatura alterada para receber `(clientId, clientSecret)`.
