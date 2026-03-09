using AutoMapper;
using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;

using MediaOrganizerApp.Shared.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{
    public class LocaisArquivoService : ILocaisArquivoService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<LocaisArquivoService> _logger;
        private string _Uri = Endpoints.StoragePlacesEndpoint;

        public LocaisArquivoService(HttpClient httpClient, IMapper mapper,
            ILogger<LocaisArquivoService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<LocalArquivoVM>> GetAllAsync()
        {
            _logger.LogInformation("Lendo todos Locais de Arquivo do API");
            try
            {
                var _storagePlacesDTO = await httpClient.GetFromJsonAsync<IEnumerable<LocalArquivoVM>>(_Uri);
                return _storagePlacesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<LocalArquivoVM> GetStoragePlaceById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo todas os Locais de Arquivo do API");
            try
            {
                var local = await httpClient.GetFromJsonAsync<LocalArquivoVM>(uri);
                return local;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreateStoragePlace(LocalArquivoVM storagePlaceVM)
        {
            _logger.LogInformation("Criando Local de Arquivo");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, storagePlaceVM);
                var success = result.IsSuccessStatusCode;
                return success;
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

                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, storagePlaceVM);
                var success = result.IsSuccessStatusCode;
                return success;
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
                HttpResponseMessage result = await httpClient.DeleteAsync(uri);
                var success = result.IsSuccessStatusCode;
                return success;
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
            _logger.LogInformation("Lendo todos os Locais de Arquivo do API");
            try
            {
                var local = await httpClient.GetFromJsonAsync<List<LocalArquivoVM>>(uri);
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
                var result = await httpClient.GetFromJsonAsync<int>(uri);
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
                var pubs = httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(LocalArquivoVM localArquivo)
        {
            LocalArquivoValidador validator = new LocalArquivoValidador();
            ValidationResult results = validator.Validate(localArquivo);

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
