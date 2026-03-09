using AutoMapper;
using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;
using MediaOrganizerApp.Shared.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{

    public class Formatos_MediaService : IFormatos_MediaService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<Formatos_MediaService> _logger;
        private string _Uri = Endpoints.MediaFormatsEndpoint;

        public Formatos_MediaService(HttpClient httpClient, IMapper mapper, ILogger<Formatos_MediaService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<IEnumerable<FormatoMediaVM>> GetAllAsync()
        {

            _logger.LogInformation("Lendo todos os Formato (Publicação) do API");
            try
            {
                var _formatosMediaDTO = (await httpClient.GetFromJsonAsync<IEnumerable<FormatoMediaVM>>(_Uri)).ToList();
                return _formatosMediaDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                return null;
            }
        }

        public async Task<FormatoMediaVM> GetFormatosMediaById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo todas as Formato (Publicação)s do API");
            try
            {
                var publicationFormat = await httpClient.GetFromJsonAsync<FormatoMediaVM>(uri);
                return publicationFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreateMediaFormat(FormatoMediaVM FormatoMediaVM)
        {
            _logger.LogInformation("Criando Formato");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, FormatoMediaVM);
                var success = result.IsSuccessStatusCode;
                return success;
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

                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, publicationFormatDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Formato (Publicação) ({id})");
                throw;
            }
        }
        public async Task<bool> DeleteMediaFormat(int id)
        {
            var uri = $"{_Uri}/{id:int}";

            try
            {
                HttpResponseMessage result = await httpClient.DeleteAsync(uri);
                var success = result.IsSuccessStatusCode;
                return success;
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
            _logger.LogInformation("Lendo todos os Formatos do API");
            try
            {
                var formatos = await httpClient.GetFromJsonAsync<List<FormatoMediaVM>>(uri);
                return formatos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public List<MediaVM> Media_CatalogadosByFormato(int id)
        {
            var uri = $"{_Uri}/Media_CatalogadosByFormato/{id}";
            try
            {
                var result = httpClient.GetFromJsonAsync<List<MediaVM>>(uri);
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
                var result = await httpClient.GetFromJsonAsync<int>(uri);
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
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id:int}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(FormatoMediaVM formato)
        {
            FormatoMediaValidator validator = new FormatoMediaValidator();
            ValidationResult results = validator.Validate(formato);

            if (!results.IsValid)
            {
                StringBuilder sb = new StringBuilder();
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
