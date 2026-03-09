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
    public class FormatosService : IFormatosService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<FormatosService> _logger;
        private string _Uri = Endpoints.PublicationsFormatsEndpoint;


        public FormatosService(HttpClient httpClient, IMapper mapper,
            ILogger<FormatosService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<FormatoPublicacaoVM>> GetAllAsync()
        {

            _logger.LogInformation("Lendo todos os Formato (Publicação) do API");
            try
            {
                var _formatospublicacoesDTO = (await httpClient.GetFromJsonAsync<IEnumerable<FormatoPublicacaoVM>>(_Uri)).ToList();
                return _formatospublicacoesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                return null;
            }
        }

        public async Task<FormatoPublicacaoVM> GetFormatosPublicacoesById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo todas as Formato (Publicação)s do API");
            try
            {
                var publicationFormat = await httpClient.GetFromJsonAsync<FormatoPublicacaoVM>(uri);
                return publicationFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreatePublicationFormat(FormatoPublicacaoVM formatoPublicacaoVM)
        {
            _logger.LogInformation("Criando Formato");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, formatoPublicacaoVM);
                var success = result.IsSuccessStatusCode;
                return success;
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
        public async Task<bool> DeletePublicationFormat(int id)
        {
            var uri = $"{_Uri}/{id}";
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

        public async Task<List<FormatoPublicacaoVM>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            _logger.LogInformation("Lendo todos os Formatos do API");
            try
            {
                var formatos = await httpClient.GetFromJsonAsync<List<FormatoPublicacaoVM>>(uri);
                return formatos;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
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
        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int id)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id}";
            try
            {
                var pubs = httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(FormatoPublicacaoVM formato)
        {
            FormatoValidator validator = new FormatoValidator();
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
