using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Shared.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web.Blazor.Static;

namespace Web.Blazor.Services.ApiRelated
{
    public class PublicacaoService : IPublicacaoService
    {
        private string _Uri = Endpoints.PublicationsEndpoint;
        private readonly HttpClient httpClient;
        private readonly ILogger<PublicacaoService> _logger;
        private readonly IFormService _formsService;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public PublicacaoService(HttpClient httpClient,
            ILogger<PublicacaoService> logger, IFormService formsService, IWebHostEnvironment hostingEnvironment)
        {
            this.httpClient = httpClient;
            _logger = logger;
            _formsService = formsService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IEnumerable<PubsWithDescriptions>> GetAllAsync()
        {
            _logger.LogInformation("Lendo todas as publicações...");
            try
            {
                var publications = (await httpClient.GetFromJsonAsync<IEnumerable<PubsWithDescriptions>>(_Uri)).ToList();
                return publications;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                return Enumerable.Empty<PubsWithDescriptions>();
            }
        }

        public async Task<PubsWithDescriptions> GetByIdAsync_Extended(int id)
        {
            _logger.LogInformation($"Pesquisando Publicação com o Id {id}");

            var uri = $"{_Uri}/{id}";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API por Id ({id})");

                throw;
            }
        }

        public async Task<bool> InsertPubAsync(PubsWithDescriptions publicationDTO)
        {
            _logger.LogInformation($"Criação de Publicação {publicationDTO.Titulo}");

            Publicacao pub = new Publicacao();
            try
            {

                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, publicationDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar registo");

                throw;
            }
        }
        public async Task UpdatePubAsync(int id, PubsWithDescriptions publicationDTO)
        {
            var uri = $"{_Uri}/{id}";

            try
            {
                _logger.LogInformation($"Atualiza Publicação '{publicationDTO.Titulo}'");
                await httpClient.PutAsJsonAsync(uri, publicationDTO);
            }
            catch (Exception exc)
            {
                _logger.LogError(AppLoggingEvents.Error, exc, $"Erro ao atualizar registo {id}");

                throw;
            }
        }
        public async Task<bool> DeletePubAsync(int id)
        {
            var uri = $"{_Uri}/{id}";
            try
            {
                _logger.LogInformation($"Apaga Publicação com o Id '{id}'");

                HttpResponseMessage result = await httpClient.DeleteAsync(uri);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao apagar registo {id}");
                throw;
            }
        }

        public async Task AtualizaTituloOriginal(int id, string sTituloOriginal)
        {
            var uri = $"{_Uri}/AtualizaTituloOriginal/{id}/{sTituloOriginal}";
            try
            {
                await httpClient.PutAsJsonAsync(uri, new PubsWithDescriptions());

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API por Id ({id})");

                throw;
            }
        }

        public async Task Atualiza_CapaPublicacao(int id, string sImageCoverPath)
        {
            var uri = $"{_Uri}/AtualizaCapaPublicacao/{id}/{sImageCoverPath}";
            try
            {
                await httpClient.PutAsJsonAsync(uri, new PubsWithDescriptions());

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar registo ({id})");

                throw;
            }
        }

        public async Task Atualiza_Publicacao(int id, PubsWithDescriptions pub)
        {
            var uri = $"{_Uri}/atualiza_publicacao/{id}";
            try
            {
                _logger.LogInformation($"Atualiza Publicação '{id} / {pub.Titulo}'");

                await httpClient.PutAsJsonAsync(uri, pub);

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar registo ({id})");

                throw;
            }
        }

        public async Task<List<AutorDTO>> AutoresSemLivros()
        {
            var uri = $"{_Uri}/AutoresSemLivros";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<List<AutorDTO>>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (AutoresSemLivros)");
                throw;
            }
        }

        public async Task<bool> AutorComLivros(int id)
        {
            var uri = $"{_Uri}/AutoresComLivros/{id}";
            try
            {
                var publisherWithPubs = await httpClient.GetFromJsonAsync<bool>(uri);
                return publisherWithPubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (AutorComLivros - {id})");
                throw;
            }
        }

        public async Task<List<PublisherVM>> EditorasSemLivros()
        {
            var uri = $"{_Uri}/EditorasSemLivros";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<List<PublisherVM>>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (EditorasSemLivros)");
                throw;
            }
        }

        public async Task<int> GetFirstIdLivro()
        {
            var uri = $"{_Uri}/GetFirstIdLivro";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<int>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetFirstIdLivro)");
                throw;
            }
        }

        public async Task<PubsWithDescriptions> Query_ById_Extended(int id)
        {
            var uri = $"{_Uri}/Query_ById_Extended/{id}";
            try
            {
                var publication = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return publication;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Query_ById_Extended)");
                throw;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Query_Async_Extended(string where)
        {
            var uri = $"{_Uri}/Query_ById_Extended/{where}";
            try
            {
                var publications = await httpClient.GetFromJsonAsync<IEnumerable<PubsWithDescriptions>>(uri);
                return publications;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Query_ById_Extended)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetID_ByCapa(string capa)
        {
            var uri = $"{_Uri}/GetID_ByCapa/{capa}";
            try
            {
                var publications = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return publications;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetID_ByCapa)");
                throw;
            }
        }

        public async Task<int> GetID_ByPath(string sTitulo)
        {
            var uri = $"{_Uri}/GetID_ByPath/{sTitulo}";
            try
            {
                var publicationId = await httpClient.GetFromJsonAsync<int>(uri);
                return publicationId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetID_ByPath)");
                throw;
            }
        }

        public async Task<int> GetLastIdLivro()
        {
            var uri = $"{_Uri}/GetLastIdLivro";
            try
            {
                var publicationId = await httpClient.GetFromJsonAsync<int>(uri);
                return publicationId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetLastIdLivro)");
                throw;
            }
        }

        public async Task<string> GetLastTitle()
        {
            var uri = $"{_Uri}/GetLastTitle";
            try
            {
                var lastTitle = await httpClient.GetFromJsonAsync<string>(uri);
                return lastTitle;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetLastTitle)");
                throw;
            }
        }

        public async Task<string> GetPubUserName(int iUserID)
        {
            var uri = $"{_Uri}/GetPubUserName/{iUserID}";
            try
            {
                var pubUserName = await httpClient.GetFromJsonAsync<string>(uri);
                return pubUserName;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetPubUserName)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetPubsByTitle(string sTitle)
        {
            var uri = $"{_Uri}/GetPubsByTitle/{sTitle}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetPubsByTitle)");
                throw;
            }
        }

        public async Task<bool> IDLivroExiste_BD(int id)
        {
            var uri = $"{_Uri}/IDLivroExiste_BD/{id}";
            try
            {
                var pubExists = await httpClient.GetFromJsonAsync<bool>(uri);
                return pubExists;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (IDLivroExiste_BD)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar(Agrupamento grupo)
        {
            var uri = $"{_Uri}/Listar/{grupo}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return output;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetPubsByTitle)");
                throw;
            }
        }

        public async Task<IEnumerable<PubsWithDescriptions>> Listar_Duplicados()
        {
            var uri = $"{_Uri}/Listar_Duplicados";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                if (output.Count > 0 || output is not null)
                    return output;
                else
                    return null;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_Duplicados)");
                throw;
            }
        }

        public async Task<PubsWithDescriptions> Listar_ID_Extended(int id)
        {
            var uri = $"{_Uri}/Listar_ID_Extended/{id}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<PubsWithDescriptions>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_ID_Extended)");
                throw;
            }
        }


        public async Task<List<PubsWithDescriptions>> Listar_Lidos(bool Lido)
        {
            var uri = $"{_Uri}/Listar_Lidos/{Lido}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_Lidos)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_Pagos(bool Pago)
        {
            var uri = $"{_Uri}/Listar_Pagos/{Pago}";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_Pagos)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemCapa()
        {
            var uri = $"{_Uri}/Listar_SemCapa";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                return pubs;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_SemCapa)");
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> Listar_SemPaginas()
        {
            var uri = $"{_Uri}/Listar_SemPaginas";
            try
            {
                var pubs = await httpClient.GetFromJsonAsync<List<PubsWithDescriptions>>(uri);
                if (pubs.Count > 0)
                    return pubs;
                else
                    return new List<PubsWithDescriptions>();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Listar_SemPaginas)");
                throw;
            }
        }

        public async Task<bool> TituloExiste_BD(string sTituloOriginal)
        {
            var uri = $"{_Uri}/TituloExiste_BD/{sTituloOriginal}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<bool>(uri);
                return output;
            }
            catch (NotSupportedException exception)
            {
                _logger.LogError(exception, $"Erro ao pesquisar API (Verificar se título existe na BD)");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (Verificar se título existe na BD)");
                throw;
            }
        }

        public Task<IEnumerable<BooksViewModel>> GetAllPubs_Grid_Async()
        {
            throw new NotImplementedException();
        }

        public List<string> RegistoComErros(PubsWithDescriptions publicacao)
        {
            PublicacaoValidator validator = new PublicacaoValidator();
            ValidationResult results = validator.Validate(publicacao);

            if (!results.IsValid)
            {
                List<string> sb = new();
                foreach (var failure in results.Errors)
                {
                    sb.Add(failure.ErrorMessage);
                }
                return sb;
            }

            return null;
        }

        public async Task<IEnumerable<ImageCategoryLocations>> GetImageLocations(int idCategoria)
        {
            var uri = $"{_Uri}/GetImageLocations/{idCategoria}";
            try
            {
                var output = await httpClient.GetFromJsonAsync<List<ImageCategoryLocations>>(uri);
                return output;
            }
            catch (NotSupportedException exception)
            {
                _logger.LogError(exception, $"Erro ao pesquisar API (GetImageLocations)");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetImageLocations)");
                throw;
            }
        }

        public async Task<bool> UpdatePubsFromContextMenu(string sUpdateQuery)
        {
            var uri = $"{_Uri}/updatepubsfromcontextmenu/{sUpdateQuery}";
            try
            {
                var UpdateOk = await httpClient.GetFromJsonAsync<bool>(uri);
                return UpdateOk;
            }
            catch (NotSupportedException exception)
            {
                _logger.LogError(exception, $"Erro ao pesquisar API (GetImageLocations)");
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API (GetImageLocations)");
                throw;
            }
        }
    }
}
