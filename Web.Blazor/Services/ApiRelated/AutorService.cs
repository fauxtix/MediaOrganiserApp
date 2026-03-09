using AutoMapper;
using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;
using MediaOrganizerApp.Domain.Entities.Author_Api_Models;
using MediaOrganizerApp.Shared.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Web.Blazor.Static;

namespace Web.Blazor.Services.ApiRelated
{
    public class AutorService : IAutorService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<AutorService> _logger;
        private readonly string _Uri = Endpoints.AuthorsEndpoint;

        public AutorService(HttpClient httpClient, IMapper mapper, ILogger<AutorService> logger)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AutorDTO>> GetAllAsync()
        {
            _logger.LogInformation("Lendo todos os Autores do API");
            try
            {
                var _AuthorsDTO = await httpClient
                    .GetFromJsonAsync<IEnumerable<AutorDTO>>(_Uri);
                return _AuthorsDTO;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<AutorDTO> GetAuthorsById(int id)
        {
            var uri = $"{_Uri}/{id}";
            _logger.LogInformation("Lendo Autor por Id");
            try
            {
                var author = await httpClient.GetFromJsonAsync<AutorDTO>(uri);
                return author;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreateAuthor(AutorDTO authorDTO)
        {
            _logger.LogInformation("Criação deAutor");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, authorDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Autor {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateAuthor(int id, AutorDTO authorDTO)
        {
            var uri = $"{_Uri}/{id}";
            try
            {

                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, authorDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Autor ({id})");
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
                _logger.LogError(exc, $"Erro ao apagar Autor ({id})");
                throw;
            }
        }

        public async Task<int> GetFirstId()
        {
            var uri = $"{_Uri}/getfirstid";
            try
            {
                var authorId = await httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetFirstId)");
                throw;
            }
        }

        public async Task<int> GetLastId()
        {
            var uri = $"{_Uri}/getlastid";
            try
            {
                var authorId = await httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        //public async Task<IEnumerable<AutorDTO>> Query(string where = null)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<AutorDTO> Query_ById(int Id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<int> GetIdByDescription(string description)
        {
            var uri = $"{_Uri}/GetIdByDescription/{description}";
            try
            {
                var authorId = await httpClient.GetFromJsonAsync<int>(uri);
                return authorId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetIdByDescription)");
                throw;
            }
        }

        public async Task<string> GetDescriptionById(int id)
        {
            var uri = $"{_Uri}/GetDescriptionById/{id}";
            try
            {
                var description = await httpClient.GetFromJsonAsync<string>(uri);
                return description;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetDescriptionById)");
                throw;
            }
        }

        //public async Task<IEnumerable<AutorDTO>> Query_Async(string where = null)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<AutorDTO> Query_ById_Async(int Id)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<int> GetIdByDescription_Async(string Description)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<string> GetDescriptionById_Async(int Id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> RecInUse(int id)
        {
            var uri = $"{_Uri}/RecInUse/{id}";
            try
            {
                bool inUse = await httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<bool> RecInUse(string descricao)
        {
            var uri = $"{_Uri}/RecInUse/{descricao}";
            try
            {
                bool inUse = await httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<bool> TableHasData()
        {
            var uri = $"{_Uri}/TableHasData";
            try
            {
                bool inUse = await httpClient.GetFromJsonAsync<bool>(uri);
                return inUse;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<List<AutorDTO>> Listar()
        {
            var uri = $"{_Uri}/Listar";
            try
            {
                var authors = await httpClient.GetFromJsonAsync<List<AutorDTO>>(uri);
                return authors;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<bool> AutorExiste(string nomeAutor)
        {
            var uri = $"{_Uri}/AutorExiste/{nomeAutor}";
            try
            {
                var authorExist = await httpClient.GetFromJsonAsync<bool>(uri);
                return authorExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<AuthorModel> GetAuthorInfo_Api(string nomeAutor)
        {
            var uri = $"{_Uri}/GetAuthorInfo/{nomeAutor}";
            try
            {
                var authorInfo = await httpClient.GetFromJsonAsync<AuthorModel>(uri);                
                return authorInfo;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetAuthorInfo_Api)");
                throw;
            }
        }

        //public async Task<IEnumerable<AutorDTO>> ListByDescription(string Description)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IDataReader> ListByDescription_Reader(string Descricao)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IEnumerable<AutorDTO>> Listar_ID(int Id)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<IDataReader> Listar_ID_DR(int Id)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<IEnumerable<AutorDTO>> AutoresSemLivros()
        {
            var uri = $"{_Uri}/AutoresSemLivros";
            try
            {
                var autoresSemLivros = await httpClient.GetFromJsonAsync<IEnumerable<AutorDTO>>(uri);
                return autoresSemLivros;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetLastId)");
                throw;
            }
        }

        public async Task<bool> AutorComLivros(int id)
        {
            var uri = $"{_Uri}/AutorComLivros/{id}";
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

        public IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor)
        {
            var uri = $"{_Uri}/PublicacoesCatalogadas/{CodAutor}";
            try
            {
                var pubs = httpClient.GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri).Result;
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (PublicacoesCatalogadas)");
                throw;
            }
        }

        public string RegistoComErros(AutorDTO autor)
        {
            AutorValidator validator = new();
            ValidationResult results = validator.Validate(autor);

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
