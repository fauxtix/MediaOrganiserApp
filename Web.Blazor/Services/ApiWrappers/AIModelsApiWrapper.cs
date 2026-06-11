using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using System.Text;

namespace Web.Blazor.Services.ApiWrappers
{
    public class AIModelsApiWrapper : IAiModelsService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;

        private readonly ILogger<AIModelsApiWrapper> _logger;
        private readonly string _Uri;

        public AIModelsApiWrapper(HttpClient httpClient,
            ILogger<AIModelsApiWrapper> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/AIModels";
        }

        public async Task<bool> AIModelExist(int Id)
        {
            var uri = $"{_Uri}/modelexist/{Id}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<bool>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método AIModelExist");
                throw;
            }
        }

        public async Task<IEnumerable<AIModelVM>> GetAllAsync()
        {
            var uri = $"{_Uri}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<AIModelVM>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetAllAsync");
                throw;
            }
        }

        public async Task<int> GetModelId(string modelNameDescription)
        {
            var uri = $"{_Uri}/GetIdAIModel/{modelNameDescription}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetModelId");
                throw;
            }
        }

        public async Task<string> GetModelName(int Id)
        {
            var uri = $"{_Uri}/GetIdAIModel/modelname/{Id}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<string>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetModelName");
                throw;
            }
        }

        public async Task<AIModelVM> GetModelById(int id)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                var model = await _httpClient.GetFromJsonAsync<AIModelVM>(uri);
                return model;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API AIModels");
                return null;
            }
        }

        public async Task<bool> CreateAIModel(AIModelVM aiModel)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, aiModel))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar AIModel {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateAIModel(int id, AIModelVM aiModel)
        {
            var uri = $"{_Uri}/{id}";

            try
            {

                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, aiModel))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar AIModel ({id})");
                throw;
            }
        }

        public async Task<bool> DeleteModel(int id)
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
                _logger.LogError(exc, $"Erro ao apagar AIModel ({id})");
                throw;
            }
        }

        public async Task<int> GetIdCategoria(string description)
        {
            var uri = $"{_Uri}/GetIdAIModel/{description}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetIdAIModel");
                throw;
            }
        }


        public string RegistoComErros(AIModelVM aiModel)
        {
            AIModelValidator validator = new();
            ValidationResult results = validator.Validate(aiModel);

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
