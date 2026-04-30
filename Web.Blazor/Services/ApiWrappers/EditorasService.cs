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
    public class EditorasService : IEditorasService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;

        private readonly ILogger<EditorasService> _logger;

        private readonly string _Uri;

        public EditorasService(HttpClient httpClient, 
            ILogger<EditorasService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/publishers";
        }

        public async Task<IEnumerable<PublisherVM>> GetAllAsync()
        {
            try
            {
                var _CategoriesDTO = await _httpClient.GetFromJsonAsync<IEnumerable<PublisherVM>>(_Uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras");
                return Enumerable.Empty<PublisherVM>();
            }
        }

        public async Task<PublisherVM> GetAuthorsById(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {
                var publisher = await _httpClient.GetFromJsonAsync<PublisherVM>(uri);
                return publisher;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras");
                throw;
            }
        }

        public async Task<bool> CreateAuthor(PublisherVM publisherVM)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, publisherVM))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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
                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, publisherDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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
                using (HttpResponseMessage result = await _httpClient.DeleteAsync(uri))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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
            try
            {
                var editoras = await _httpClient.GetFromJsonAsync<List<PublisherVM>>(uri);
                return editoras;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras (listar editoras)");
                throw;
            }
        }

        public async Task<int> GetIdEditora(string description)
        {
            var uri = $"{_Uri}/GetIdEditora/{description}";
            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
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
            try
            {
                var pubs = _httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras (PublicacoesCatalogadas)");
                throw;
            }
        }

        public async Task<IEnumerable<PublisherVM>> EditorasSemLivros()
        {
            var uri = $"{_Uri}/EditorasSemLivros";

            try
            {
                var editorasSemLivros = await _httpClient.GetFromJsonAsync<IEnumerable<PublisherVM>>(uri);
                return editorasSemLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras (EditorasSemLivros)");
                throw;
            }
        }
        public async Task<bool> EditoraComLivros(int id)
        {
            var uri = $"{_Uri}/EditoraComLivros/{id}";
            try
            {
                var autorComLivros = await _httpClient.GetFromJsonAsync<bool>(uri);
                return autorComLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras (AutorComLivros)");
                throw;
            }
        }

        public async Task<List<PublisherVM>> Listar_Editora(string descricao)
        {
            var uri = $"{_Uri}/Listar_Editora/{descricao}";
            try
            {
                var editoras = await _httpClient.GetFromJsonAsync<List<PublisherVM>>(uri);
                return editoras;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API Editoras (listar editoras)");
                throw;
            }
        }

        public string RegistoComErros(PublisherVM editora)
        {

            EditoraValidator validator = new();
            ValidationResult results = validator.Validate(editora);

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
