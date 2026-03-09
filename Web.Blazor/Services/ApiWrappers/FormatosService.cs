using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using MediaOrganizerApp.Domain.Entities;
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
    public class FormatosService : IFormatosService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;

        private readonly ILogger<FormatosService> _logger;
        private readonly string _Uri;

        public FormatosService(HttpClient httpClient, 
            ILogger<FormatosService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/publicationsformats";
        }

        public async Task<IEnumerable<FormatoPublicacaoVM>> GetAllAsync()
        {

            try
            {
                var _formatospublicacoesDTO = (await _httpClient.GetFromJsonAsync<IEnumerable<FormatoPublicacaoVM>>(_Uri)).ToList();
                return _formatospublicacoesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos");
                return Enumerable.Empty<FormatoPublicacaoVM>();
            }
        }

        public async Task<FormatoPublicacaoVM> GetFormatosPublicacoesById(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {
                var publicationFormat = await _httpClient.GetFromJsonAsync<FormatoPublicacaoVM>(uri);
                return publicationFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos");
                throw;
            }
        }

        public async Task<bool> CreatePublicationFormat(FormatoPublicacaoVM formatoPublicacaoVM)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, formatoPublicacaoVM))
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

        public async Task<bool> UpdatePublicationFormat(int id, FormatoPublicacaoVM publicationFormatDTO)
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
        public async Task<bool> DeletePublicationFormat(int id)
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

        public async Task<List<FormatoPublicacaoVM>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            try
            {
                var formatos = await _httpClient.GetFromJsonAsync<List<FormatoPublicacaoVM>>(uri);
                return formatos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Formatos");
                throw;
            }
        }

        public async Task<List<FormatoPublicacaoVM>> Listar_Formato(string descricao)
        {
            var uri = $"{_Uri}/Listar_Formato/{descricao}";
            try
            {
                var formatos = await _httpClient.GetFromJsonAsync<List<FormatoPublicacaoVM>>(uri);
                return formatos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao listar formatos por descrição");
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
                _logger.LogError(exc, "Erro ao pesquisar API Formatos (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(FormatoPublicacaoVM formato)
        {
            FormatoValidator validator = new();
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
