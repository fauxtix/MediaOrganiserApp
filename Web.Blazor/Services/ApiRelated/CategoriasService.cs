using AutoMapper;
using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;
using MediaOrganizerApp.Domain.Entities;
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
    public class CategoriasService : ICategoriasService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<CategoriasService> _logger;
        private string _Uri = Endpoints.CategoriesEndpoint;

        public CategoriasService(HttpClient httpClient, IMapper mapper,
            ILogger<CategoriasService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryVM>> GetAllAsync(int tipo)
        {
            var uri = $"{_Uri}/GetAllAsync/{tipo}";

            _logger.LogInformation($"Lendo todas Categorias do API por tipo {tipo}");
            try
            {
                var _CategoriesDTO = await httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Autores - Erro ao pesquisar API (GetAllAsync)");
                return null;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications()
        {
            var uri = $"{_Uri}/GetCategoriesWithPublications";

            _logger.LogInformation($"Filtrando Categorias com Publicações");
            try
            {
                var _CategoriesDTO = await httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _CategoriesDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API de Categorias (GetCategoriesWithPublications)");
                return null;
            }
        }

        public async Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries()
        {
            var uri = $"{_Uri}/GetCategoriesWithMediaEntries";

            _logger.LogInformation($"Filtrando Categorias com Media...");
            try
            {
                var _categories = await httpClient.GetFromJsonAsync<IEnumerable<CategoryVM>>(uri);
                return _categories;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API de Categorias");
                return null;
            }
        }


        public async Task<CategoryVM> GetCategoriesById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo Categorias por Id");
            try
            {
                var category = await httpClient.GetFromJsonAsync<CategoryVM>(uri);
                return category;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                return null;
            }
        }

        public async Task<bool> CreateCategory(Categoria categoria)
        {
            _logger.LogInformation($"Criando Categoria {categoria.Descricao}");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, categoria);
                var success = result.IsSuccessStatusCode;
                return success;
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
            _logger.LogInformation($"Atualização de Categoria {categoria.Descricao}");

            try
            {

                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, categoria);
                var success = result.IsSuccessStatusCode;
                return success;
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
            _logger.LogInformation($"Apaga Categoria com o Id {id}");

            try
            {
                HttpResponseMessage result = await httpClient.DeleteAsync(uri);
                var success = result.IsSuccessStatusCode;
                return success;
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
                var result = await httpClient.GetFromJsonAsync<List<CategoryVM>>(uri);
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
                var result = await httpClient.GetFromJsonAsync<int>(uri);
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
                var pubs =  httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri);
                return pubs.Result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (PublicacoesCatalogadas)");
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
                var autorComLivros = await httpClient.GetFromJsonAsync<bool>(uri);
                return autorComLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (CategorisComRegistos)");
                throw;
            }
        }

        public async Task<List<MediaVM>> MediaCatalogados(int id)
        {

            var uri = $"{_Uri}/MediaCatalogados/{id}";
            try
            {
                var result = await httpClient.GetFromJsonAsync<List<MediaVM>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (MediaCatalogados)");
                throw;
            }
        }

        public string RegistoComErros(CategoryVM categoria)
        {
            CategoriaValidator validator = new CategoriaValidator();
            ValidationResult results = validator.Validate(categoria);

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
