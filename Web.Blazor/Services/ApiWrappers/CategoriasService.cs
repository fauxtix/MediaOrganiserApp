using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using System.Text;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.ApiWrappers
{
    public class CategoriasService : ICategoriasService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoriasService> _logger;
        private readonly string _Uri;
        private readonly IConfiguration _env;

        public CategoriasService(HttpClient httpClient, 
            ILogger<CategoriasService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;

            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/categorias";
        }

        public async Task<IEnumerable<CategoryVM>> GetAllAsync(CategoryType categoryType)
        {
            var uri = $"{_Uri}/AllAsync/{(int)categoryType}";

            try
            {
                var _CategoriesDTO = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Autores - Erro ao pesquisar API Categorias(GetAllAsync)");
                return null;
            }
        }
        public async Task<IEnumerable<CategoryVM>> AllAsync(CategoryType categoryType)
        {
            var uri = $"{_Uri}/AllAsync/{(int)categoryType}";

            try
            {
                var _CategoriesDTO = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Autores - Erro ao pesquisar API Categorias(GetAllAsync)");
                return null;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications()
        {
            var uri = $"{_Uri}/GetCategoriesWithPublications";

            try
            {
                var _CategoriesDTO = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Erro ao pesquisar API de Categorias (GetCategoriesWithPublications) {exc.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries()
        {
            var uri = $"{_Uri}/GetCategoriesWithMediaEntries";

            try
            {
                var _categories = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _categories;
            }
            catch (Exception exc)
            {
                if(exc.Message.Contains("404"))
                {
                    _logger.LogWarning($"Tabela 'Media' sem registos... opção ignorada (GetCategoriesWithMediaEntries)");
                }
                else
                {
                    _logger.LogError($"Erro ao pesquisar API de Categorias (GetCategoriesWithMediaEntries) ({exc.Message})");
                }
                return null;
            }
        }


        public async Task<CategoryVM> GetCategoriesById(int id)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                var category = await _httpClient.GetFromJsonAsync<CategoryVM>(uri);
                return category;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Categorias");
                return null;
            }
        }

        public async Task<bool> CreateCategory(CategoryVM categoria)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, categoria))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Categoria {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateCategory(int id, CategoryVM categoria)
        {
            var uri = $"{_Uri}/{id}";

            try
            {

                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, categoria))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Categoria ({id})");
                throw;
            }
        }
        public async Task<bool> DeleteCategory(int id)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                using (HttpResponseMessage result = await _httpClient.DeleteAsync(uri))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao apagar Categoria ({id})");
                throw;
            }
        }

        public async Task<List<CategoryVM>> Listar(int tipo)
        {
            var uri = $"{_Uri}/Listar?tipo={tipo}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<CategoryVM>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao Listar");
                throw;
            }
        }

        public async Task<int> GetIdCategoria(string description)
        {
            var uri = $"{_Uri}/GetIdCategoria/{description}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetIdCategoria");
                throw;
            }
        }

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int id)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id}";

            try
            {
                var pubs =  _httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Categorias (PublicacoesCatalogadas)");
                throw;
            }
        }
        public Task<List<CategoryVM>> ListByType(AppDefinitions.Query_TipoMedia tipoMedia)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CategoriaComRegistos(int id)
        {
            var uri = $"{_Uri}/CategoriaComRegistos/{id}";

            try
            {
                var autorComLivros = await _httpClient.GetFromJsonAsync<bool>(uri);
                return autorComLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (CategoriasComRegistos)");
                throw;
            }
        }

        public List<MediaVM> MediaCatalogados(int id)
        {
            var uri = $"{_Uri}/MediaCatalogados/{id}";

            try
            {
                var result = _httpClient.GetFromJsonAsync<List<MediaVM>>(uri);
                return result.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Sem registos catalogados (MediaCatalogados)");
                return new List<MediaVM>();
            }
        }

        public string RegistoComErros(CategoryVM categoria)
        {
            CategoriaValidator validator = new();
            ValidationResult results = validator.Validate(categoria);

            if (!results.IsValid)
            {
                StringBuilder sb = new();
                foreach (var failure in results.Errors)
                {
                    sb.AppendLine(failure.ErrorMessage);
                }
                return sb.ToString();
            }

            return "";
        }

        public async Task<List<CategoryVM>> Listar_Categoria_List(string descricao = "''", bool calledFromPubs = true)
        {
            var uri = $"{_Uri}/Listar_Categoria/{descricao}/{calledFromPubs}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<CategoryVM>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao Listar");
                throw;
            }
        }

        public async Task<List<CategoryVM>> Listar_Categoria(bool calledFromPubs = true)
        {
            int tipo = calledFromPubs ? 1 : 2;
            var uri = $"{_Uri}/Listar/{tipo}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<CategoryVM>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao Listar");
                throw;
            }
        }

    }
}
