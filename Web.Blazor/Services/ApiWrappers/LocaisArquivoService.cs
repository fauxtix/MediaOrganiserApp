using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using System.Text;

namespace Web.Blazor.Services.ApiWrappers
{
    public class LocaisArquivoService : ILocaisArquivoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;

        private readonly ILogger<LocaisArquivoService> _logger;
        private readonly string _Uri;

        public LocaisArquivoService(HttpClient httpClient,
            ILogger<LocaisArquivoService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/storageplaces";
        }

        public async Task<IEnumerable<LocalArquivoVM>> GetAllAsync()
        {
            try
            {
                var _storagePlacesDTO = await _httpClient.GetFromJsonAsync<IEnumerable<LocalArquivoVM>>(_Uri);
                return _storagePlacesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Locais de Arquivo");
                return Enumerable.Empty<LocalArquivoVM>();
            }
        }

        public async Task<LocalArquivoVM> GetStoragePlaceById(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {
                var local = await _httpClient.GetFromJsonAsync<LocalArquivoVM>(uri);
                return local;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Locais de Arquivo");
                throw;
            }
        }

        public async Task<bool> CreateStoragePlace(LocalArquivoVM storagePlaceVM)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, storagePlaceVM))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Local de Arquivo {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateStoragePlace(int id, LocalArquivoVM storagePlaceVM)
        {
            var uri = $"{_Uri}/{id}";
            try
            {

                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, storagePlaceVM))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Local de Arquivo ({id})");
                throw;
            }
        }
        public async Task<bool> DeleteStoragePlace(int id)
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
                _logger.LogError(exc, $"Erro ao apagar Local de Arquivo ({id})");
                throw;
            }
        }

        public async Task<List<LocalArquivoVM>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            try
            {
                var local = await _httpClient.GetFromJsonAsync<List<LocalArquivoVM>>(uri);
                return local;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao Listar (Locais)");
                throw;
            }
        }

        public async Task<List<LocalArquivoVM>> Listar_LocalArquivo(string descricao)
        {
            var uri = $"{_Uri}/Listar_LocalArquivo/{descricao}";
            try
            {
                var local = await _httpClient.GetFromJsonAsync<List<LocalArquivoVM>>(uri);
                return local;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao Listar (Locais)");
                throw;
            }
        }


        public async Task<int> GetIdLocalArquivo(string description)
        {
            var uri = $"{_Uri}/GetIdLocalArquivo/{description}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetIdLocalArquivo");
                throw;
            }
        }

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int id)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id}";
            try
            {
                var pubs = _httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Locais de Arquivo (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(LocalArquivoVM localArquivo)
        {
            LocalArquivoValidador validator = new();
            ValidationResult results = validator.Validate(localArquivo);

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
    }
}
