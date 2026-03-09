using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Validators;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Web.Blazor.Static;


namespace Web.Blazor.Services.ApiWrappers
{
    public class PublicacaoService : IPublicacaoService
    {
        private string _Uri;

        private readonly HttpClient httpClient;
        private readonly IConfiguration _env;
        private readonly ILogger<PublicacaoService> _logger;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IValidationService _validationService;

        public PublicacaoService(HttpClient Client,
            ILogger<PublicacaoService> logger,
            IConfiguration Env,
            UserManager<IdentityUser> manager,
            AuthenticationStateProvider authenticationStateProvider,
            IValidationService validationService)
        {
            httpClient = Client;
            _logger = logger;
            _env = Env;

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/publications";
            _authenticationStateProvider = authenticationStateProvider;
            _validationService = validationService;
        }

        public async Task<IEnumerable<PubsWithDescriptions>> GetAllAsync(string userId, string userRole)
        {
            try
            {
                _logger.LogInformation($"Endpoint do api: {_Uri} com o user: {userId} com a chave: {_env["ApiKey"]}");
                var publications = await httpClient.GetFromJsonAsync<IEnumerable<PubsWithDescriptions>>($"{_Uri}/{userId}/{userRole}");
                return publications is not null ? publications : Enumerable.Empty<PubsWithDescriptions>();
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações ({exc.Message})");
                return Enumerable.Empty<PubsWithDescriptions>();
            }
        }

        public async Task<PubsWithDescriptions> GetByIdAsync_Extended(int id)
        {

            var uri = $"{_Uri}/{id}";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações por Id ({id} ({exc.Message}))");

                throw;
            }
        }

        public async Task<bool> InsertPubAsync(PubsWithDescriptions publicationDTO)
        {
            try
            {
                publicationDTO.CreatedBy = await GetCurrentUserId();
                using (HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, publicationDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao criar publicação ({exc.Message})");

                throw;
            }
        }
        public async Task UpdatePubAsync(int id, PubsWithDescriptions publicationDTO)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                var whoCreated = publicationDTO.CreatedBy;
                var whoWillUpdate = await GetCurrentUserId();

                publicationDTO.LastModifiedBy = whoWillUpdate;
                await httpClient.PutAsJsonAsync(uri, publicationDTO);

                if (whoCreated != whoWillUpdate)
                {
                    var userName = await GetCurrentUserEmail();
                    _logger.LogWarning(AppLoggingEvents.Update, $"Publicação '{publicationDTO.Titulo}' foi alterado por {userName}");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(AppLoggingEvents.Error, exc, $"Erro ao atualizar registo {id} ({exc.Message})");

                throw;
            }
        }
        public async Task<bool> DeletePubAsync(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {

                using (HttpResponseMessage result = await httpClient.DeleteAsync(uri))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao apagar publicação {id} ({exc.Message})");
                throw;
            }
        }

        public async Task AtualizaTituloOriginal(int id, string sTituloOriginal)
        {
            var uri = $"{_Uri}/AtualizaTituloOriginal/{id}/{sTituloOriginal}";
            try
            {
                await httpClient.PutAsJsonAsync(uri, new PubsWithDescriptions());

            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações por Id ({id}) ({exc.Message})");

                throw;
            }
        }

        public async Task Atualiza_CapaPublicacao(int id, string sImageCoverPath)
        {
            var uri = $"{_Uri}/AtualizaCapaPublicacao/{id}/{sImageCoverPath}";
            try
            {
                await httpClient.PutAsJsonAsync(uri, new PubsWithDescriptions());

            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao atualizar registo ({id} : {exc.Message})");

                throw;
            }
        }

        public async Task Atualiza_Publicacao(int id, PubsWithDescriptions pub)
        {
            var uri = $"{_Uri}/atualiza_publicacao/{id}";
            try
            {

                await httpClient.PutAsJsonAsync(uri, pub);

            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao atualizar registo ({id}) ({exc.Message})");

                throw;
            }
        }

        public async Task<List<AutorDTO>> AutoresSemLivros()
        {
            var uri = $"{_Uri}/AutoresSemLivros";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<List<AutorDTO>>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (AutoresSemLivros) ({exc.Message})");
                throw;
            }
        }

        public async Task<bool> AutorComLivros(int id)
        {
            var uri = $"{_Uri}/AutoresComLivros/{id}";
            try
            {
                var publisherWithPubs = await httpClient.GetFromJsonAsync<bool>(uri);
                return publisherWithPubs;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (AutorComLivros - {id}) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            var uri = $"{_Uri}/EditorasSemLivros";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<List<PublisherVM>>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (EditorasSemLivros) ({exc.Message})");
                throw;
            }
        }

        public async Task<int> GetFirstIdLivro()
        {
            var uri = $"{_Uri}/GetFirstIdLivro";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<int>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetFirstIdLivro) ({exc.Message})");
                throw;
            }
        }

        public async Task<PubsWithDescriptions> Query_ById_Extended(int id)
        {
            var uri = $"{_Uri}/Query_ById_Extended/{id}";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Query_ById_Extended) ({exc.Message})");
                throw;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where)
        {
            var uri = $"{_Uri}/Query_ById_Extended/{where}";
            try
            {
                var publications = await httpClient.GetFromJsonAsync<IEnumerable<PubsWithDescriptions>>(uri);
                return publications;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Query_ById_Extended) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetID_ByCapa(string capa)
        {
            var uri = $"{_Uri}/GetID_ByCapa/{capa}";
            try
            {
                var publications = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return publications;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetID_ByCapa) ({exc.Message})");
                throw;
            }
        }

        public async Task<int> GetID_ByPath(string sTitulo)
        {
            var uri = $"{_Uri}/GetID_ByPath/{sTitulo}";
            try
            {
                var publicationId = await httpClient.GetFromJsonAsync<int>(uri);
                return publicationId;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetID_ByPath) ({exc.Message})");
                throw;
            }
        }

        public async Task<int> GetLastIdLivro()
        {
            var uri = $"{_Uri}/GetLastIdLivro";
            try
            {
                var publicationId = await httpClient.GetFromJsonAsync<int>(uri);
                return publicationId;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetLastIdLivro) ({exc.Message})");
                throw;
            }
        }

        public async Task<string> GetLastTitle()
        {
            var uri = $"{_Uri}/GetLastTitle";
            try
            {
                var lastTitle = await httpClient.GetFromJsonAsync<string>(uri);
                return lastTitle;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetLastTitle) ({exc.Message})");
                throw;
            }
        }

        public async Task<string> GetPubUserName(int iUserID)
        {
            var uri = $"{_Uri}/GetPubUserName/{iUserID}";
            try
            {
                var pubUserName = await httpClient.GetFromJsonAsync<string>(uri);
                return pubUserName;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubUserName) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitle)
        {
            var uri = $"{_Uri}/GetPubsByTitle/{sTitle}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs ?? new();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new();
                }

                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubsByTitle) ({ex.Message})");
                return null;
            }

            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubsByTitle) ({exc.Message})");
                throw;
            }
        }

        public async Task<bool> IDLivroExiste_BD(int id)
        {
            var uri = $"{_Uri}/IDLivroExiste_BD/{id}";
            try
            {
                var pubExists = await httpClient.GetFromJsonAsync<bool>(uri);
                return pubExists;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (IDLivroExiste_BD) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar(Agrupamento grupo)
        {
            var uri = $"{_Uri}/Listar/{grupo}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return output;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubsByTitle) ({exc.Message})");
                throw;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Listar_Duplicados()
        {
            var uri = $"{_Uri}/Listar_Duplicados";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                if (output.Count > 0 || output is not null)
                    return output;
                else
                    return null;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_Duplicados) ({exc.Message})");
                throw;
            }
        }

        public async Task<PubsWithDescriptions> Listar_ID_Extended(int id)
        {
            var uri = $"{_Uri}/Listar_ID_Extended/{id}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_ID_Extended) ({exc.Message})");
                throw;
            }
        }


        public async Task<List<PubsWithDescriptions>> Listar_Lidos(bool Lido)
        {
            var uri = $"{_Uri}/Listar_Lidos/{Lido}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_Lidos) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago)
        {
            var uri = $"{_Uri}/Listar_Pagos/{Pago}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_Pagos) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemCapa()
        {
            var uri = $"{_Uri}/Listar_SemCapa";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_SemCapa) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemPaginas()
        {
            var uri = $"{_Uri}/Listar_SemPaginas";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                if (pubs.Count > 0)
                    return pubs;
                else
                    return new List<PubsWithDescriptions>();
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Listar_SemPaginas) ({exc.Message})");
                throw;
            }
        }

        public async Task<List<FileCheckResult>> PubsNotFoundOnDisk(string webRoot)
        {
            try
            {
                var encodedWebRoot = Uri.EscapeDataString(webRoot);
                var uri = $"{_Uri}/PubsNotFoundOnDisk/{encodedWebRoot}";

                var pubsNotFoundInDisk = await httpClient.GetFromJsonAsync<List<FileCheckResult>>(uri);

                return pubsNotFoundInDisk ?? new List<FileCheckResult>();
            }
            catch (HttpRequestException httpExc)
            {
                _logger.LogError(httpExc, $"Erro de requisição HTTP ao pesquisar API (PubsNotFoundInDisk)");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (PubsNotFoundInDisk) ({exc.Message})");
                throw;
            }
        }
        public async Task<bool> TituloExiste_BD(string sTituloOriginal)
        {
            var uri = $"{_Uri}/TituloExiste_BD/{sTituloOriginal}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<bool>(uri);
                return output;
            }
            catch (NotSupportedException exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Verificar se título existe na BD) ({exc.Message})");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (Verificar se título existe na BD) ({exc.Message})");
                throw;
            }
        }

        public Task<IEnumerable<BooksViewModel>> GetAllPubs_Grid_Async()
        {
            throw new NotImplementedException();
        }

        public List<string> RegistoComErros(PubsWithDescriptions publicacao)
        {
            ValidationResult results = _validationService.Validate(publicacao);

            if (!results.IsValid)
            {
                List<string> sb = new();
                foreach (var failure in results.Errors)
                {
                    sb.Add(failure.ErrorMessage);
                }
                return sb;
            }

            return null;
        }

        public async Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria)
        {
            var uri = $"{_Uri}/GetImageLocations/{idCategoria}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<ImageCategoryLocations>>(uri);
                return output;
            }
            catch (NotSupportedException exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetImageLocations) ({exc.Message})");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetImageLocations) ({exc.Message})");
                throw;
            }
        }

        public async Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery)
        {
            var uri = $"{_Uri}/updatepubsfromcontextmenu/{sUpdateQuery}";
            try
            {
                var UpdateOk = await httpClient.GetFromJsonAsync<bool>(uri);
                return UpdateOk;
            }
            catch (NotSupportedException exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetImageLocations) ({exc.Message})");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetImageLocations) ({exc.Message})");
                throw;
            }
        }

        public async Task<int> GetPubFormat(int pubId)
        {
            var uri = $"{_Uri}/getpubformat/{pubId}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<int>(uri);
                return output;
            }
            catch (NotSupportedException exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubFormat) ({exc.Message})");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API Publicações (GetPubFormat) ({exc.Message})");
                throw;
            }
        }


        public Task<List<PubsWithDescriptions>> Listar_IDAutor(int IdAutor)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_IDCategoria(int IdCategoria)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_IDEditora(int IdEditora)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_IDFormato(int IdFormato)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_IDLocalArquivo(int IdLocalArquivo)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_PalavraTitulo(string sWhereParam)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_AnoEdicao(int parInt)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_Titulo(string Titulo)
        {
            throw new NotImplementedException();
        }

        public Task<List<PubsWithDescriptions>> Listar_UserID(int iUserID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> StorePdfNotDeleted(PdfsAndPngNotDeleted fileName)
        {
            var uri = $"{_Uri}/storepdfnotdeleted/{fileName}";

            try
            {
                using (HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, fileName))
                {
                    var isSuccess = result.IsSuccessStatusCode;
                    return isSuccess;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao criar publicação ({exc.Message})");

                throw;
            }

        }

        private async Task<string> GetCurrentUserId()
        {
            var authstate = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userId = authstate.User.Claims.FirstOrDefault().Value;
            return userId;
        }

        private async Task<string> GetCurrentUserEmail()
        {
            var authstate = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userName = authstate.User.Identity.Name;
            return userName;

        }

    }
}
