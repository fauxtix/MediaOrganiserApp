using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers
{

    public class Formatos_MediaService : IFormatos_MediaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;
        private readonly ILogger<Formatos_MediaService> _logger;
        private readonly string _Uri;

        public Formatos_MediaService(HttpClient httpClient, 
            ILogger<Formatos_MediaService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/mediaformats";
        }

        public async Task<IEnumerable<FormatoMediaVM>> GetAllAsync()
        {

            try
            {
                var _formatosMediaDTO = (await _httpClient.GetFromJsonAsync<IEnumerable<FormatoMediaVM>>(_Uri)).ToList();
                return _formatosMediaDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos Media");
                return Enumerable.Empty<FormatoMediaVM>();
            }
        }

        public async Task<FormatoMediaVM> GetFormatosMediaById(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {
                var publicationFormat = await _httpClient.GetFromJsonAsync<FormatoMediaVM>(uri);
                return publicationFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos Media");
                throw;
            }
        }

        public async Task<bool> CreateMediaFormat(FormatoMediaVM FormatoMediaVM)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, FormatoMediaVM))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Formato (Publicação) {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateMediaFormat(int id, FormatoMediaVM publicationFormatDTO)
        {
            var uri = $"{_Uri}/{id}";
            try
            {

                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, publicationFormatDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Formato (Publicação) ({id})");
                throw;
            }
        }
        public async Task<bool> DeleteMediaFormat(int id)
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
                _logger.LogError(exc, $"Erro ao apagar Formato (Publicação) ({id})");
                throw;
            }
        }

        public async Task<List<FormatoMediaVM>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            try
            {
                var formatos = await _httpClient.GetFromJsonAsync<List<FormatoMediaVM>>(uri);
                return formatos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos Media");
                throw;
            }
        }

        public List<MediaVM> Media_CatalogadosByFormato(int id)
        {
            var uri = $"{_Uri}/Media_CatalogadosByFormato/{id}";
            try
            {
                var result = _httpClient.GetFromJsonAsync<List<MediaVM>>(uri);
                return result.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método Media_CatalogadosByFormato");
                throw;
            }
        }
        public async Task<int> GetIdFormato(string description)
        {
            var uri = $"{_Uri}/GetIdFormato/{description}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetIdFormato");
                throw;
            }
        }
        public async Task<IEnumerable<PublicacoesVM>> PublicacoesCatalogadas(int id)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id}";
            try
            {
                var pubs = await _httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos Media(PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(FormatoMediaVM formato)
        {
            FormatoMediaValidator validator = new();
            ValidationResult results = validator.Validate(formato);

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
