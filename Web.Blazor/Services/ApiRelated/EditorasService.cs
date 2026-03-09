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
    public class EditorasService : IEditorasService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<EditorasService> _logger;
        private string _Uri = Endpoints.PublishersEndpoint;

        public EditorasService(HttpClient httpClient, IMapper mapper,
            ILogger<EditorasService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PublisherVM>> GetAllAsync()
        {
            _logger.LogInformation("Lendo todas as Editoras via API");
            try
            {
                var _CategoriesDTO = await httpClient.GetFromJsonAsync<IEnumerable<PublisherVM>>(_Uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<PublisherVM> GetAuthorsById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo Editoras por Id");
            try
            {
                var publisher = await httpClient.GetFromJsonAsync<PublisherVM>(uri);
                return publisher;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreateAuthor(PublisherVM publisherVM)
        {
            _logger.LogInformation("Criando Editora");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, publisherVM);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Editora {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdatePublisher(int id, PublisherVM publisherDTO)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, publisherDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Editora ({id})");
                throw;
            }
        }
        public async Task<bool> DeleteAuthor(int id)
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
                _logger.LogError(exc, $"Erro ao apagar Editora ({id})");
                throw;
            }
        }

        public async Task<List<PublisherVM>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            _logger.LogInformation("Lendo todos as Editoras via método 'Listar'");
            try
            {
                var editoras = await httpClient.GetFromJsonAsync<List<PublisherVM>>(uri);
                return editoras;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (listar editoras)");
                throw;
            }
        }

        public async Task<int> GetIdEditora(string description)
        {
            var uri = $"{_Uri}/GetIdEditora/{description}";
            _logger.LogInformation("Lendo Id da Editora a partir de Descrição");
            try
            {
                var result = await httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no método GetIdEditora");
                throw;
            }
        }

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int id)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{id}";
            _logger.LogInformation("Lendo Publicações Catalogas através de Id");
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

        public async Task<IEnumerable<PublisherVM>> EditorasSemLivros()
        {
            var uri = $"{_Uri}/EditorasSemLivros";
            _logger.LogInformation("Lendo Editoras sem publicações");

            try
            {
                var editorasSemLivros = await httpClient.GetFromJsonAsync<IEnumerable<PublisherVM>>(uri);
                return editorasSemLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (EditorasSemLivros)");
                throw;
            }
        }
        public async Task<bool> EditoraComLivros(int id)
        {
            var uri = $"{_Uri}/EditoraComLivros/{id}";
            _logger.LogInformation("Lendo Editoras com publicações");
            try
            {
                var autorComLivros = await httpClient.GetFromJsonAsync<bool>(uri);
                return autorComLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (AutorComLivros)");
                throw;
            }
        }

        public string RegistoComErros(PublisherVM editora)
        {
            _logger.LogInformation($"Validando Editora {editora.Descricao}");

            EditoraValidator validator = new EditoraValidator();
            ValidationResult results = validator.Validate(editora);

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
