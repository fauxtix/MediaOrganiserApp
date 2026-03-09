using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class BO_Publicacoes : IBO_Publicacoes
    {
        private readonly IPublicacoesRepository _pubsRepo;
        private readonly IFormService _formsSvc;
        private readonly ILogger _logger;


        public BO_Publicacoes(IPublicacoesRepository pubsRepository, IFormService formsService, ILogger<BO_Publicacoes> logger)
        {
            _pubsRepo = pubsRepository;
            _formsSvc = formsService;
            _logger = logger;
        }

        public async Task<List<PublicationReportModel>> GetPublications(TipoPesquisa iTipo, int idId, string sTitleQuery = "")
        {
            List<PubsWithDescriptions> publicacoes = new();

            switch (iTipo)
            {
                case TipoPesquisa.Todos:
                    publicacoes = await _pubsRepo.Listar(Agrupamento.Todos);
                    break;
                case TipoPesquisa.Autor:
                    publicacoes = await _pubsRepo.Listar_IDAutor(idId);
                    break;
                case TipoPesquisa.Editora:
                    publicacoes = await _pubsRepo.Listar_IDEditora(idId);
                    break;
                case TipoPesquisa.Genero:
                    publicacoes = await _pubsRepo.Listar_IDCategoria(idId);
                    break;
                case TipoPesquisa.Formato:
                    publicacoes = await _pubsRepo.Listar_IDFormato(idId);
                    break;
                case TipoPesquisa.Arquivo:
                    publicacoes = await _pubsRepo.Listar_IDLocalArquivo(idId);
                    break;
                case TipoPesquisa.Lido:
                    publicacoes = await _pubsRepo.Listar_Lidos(Convert.ToBoolean(idId));
                    break;
                case TipoPesquisa.Titulo:
                    publicacoes = await _pubsRepo.Listar_PalavraTitulo(sTitleQuery);
                    break;
            }

            List<PublicationReportModel> ListaPublicacoes = new();
            foreach (var item in publicacoes)
            {
                var autor = _formsSvc.GetDescricao(item.IdAutor, "Autor");
                var editora = _formsSvc.GetDescricao(item.IdEditora, "Editora");
                var localArquivo = _formsSvc.GetDescricao(item.IdLocalArquivo, "LocalArquivo");
                var categoria = _formsSvc.GetDescricaoCategoria(item.IdCategoria, 1);

                ListaPublicacoes.Add(new PublicationReportModel
                {
                    Title = item.Titulo,
                    Author = autor,
                    Category = categoria,
                    Publisher = editora,
                    Storage = localArquivo,
                    YearPublished = item.AnoEdicao
                }
                );
            }
            return ListaPublicacoes;
        }

        public async Task<List<PublicationReportModel>> GetPublicationsData(TipoPesquisa iTipo, int idId)
        {
            List<PubsWithDescriptions> publicacoes = new();

            try
            {
                switch (iTipo)
                {
                    case TipoPesquisa.Todos:
                        publicacoes = await _pubsRepo.Listar(Agrupamento.Todos);
                        break;
                    case TipoPesquisa.Autor:
                        publicacoes = await _pubsRepo.Listar_IDAutor(idId);
                        break;
                    case TipoPesquisa.Editora:
                        publicacoes = await _pubsRepo.Listar_IDEditora(idId);
                        break;
                    case TipoPesquisa.Genero:
                        publicacoes = await _pubsRepo.Listar_IDCategoria(idId);
                        break;
                    case TipoPesquisa.Formato:
                        publicacoes = await _pubsRepo.Listar_IDFormato(idId);
                        break;
                    case TipoPesquisa.Arquivo:
                        publicacoes = await _pubsRepo.Listar_IDLocalArquivo(idId);
                        break;
                    case TipoPesquisa.Pago:
                        publicacoes = await _pubsRepo.Listar_Pagos(Convert.ToBoolean(idId));
                        break;
                    case TipoPesquisa.Lido:
                        publicacoes = await _pubsRepo.Listar_Lidos(Convert.ToBoolean(idId));
                        break;
                    case TipoPesquisa.SemPaginas:
                        publicacoes = await _pubsRepo.Listar_SemPaginas();
                        break;
                    case TipoPesquisa.SemCapa:
                        publicacoes = await _pubsRepo.Listar_SemCapa();
                        break;
                    case TipoPesquisa.Duplicados:
                        publicacoes = (List<PubsWithDescriptions>)await _pubsRepo.Listar_Duplicados();
                        break;
                    case TipoPesquisa.Utilizador:
                        publicacoes = await _pubsRepo.Listar_UserID(idId);
                        break;
                }

                List<PublicationReportModel> ListaPublicacoes = new();
                foreach (var item in publicacoes)
                {
                    ListaPublicacoes.Add(new PublicationReportModel
                    {
                        Title = item.Titulo,
                        Author = _formsSvc.GetDescricao(item.IdAutor, "Autor"),
                        Category = _formsSvc.GetDescricaoCategoria(item.IdCategoria, 1),
                        Publisher = _formsSvc.GetDescricao(item.IdEditora, "Editora"),
                        Storage = _formsSvc.GetDescricao(item.IdLocalArquivo, "LocalArquivo"),
                        YearPublished = item.AnoEdicao
                    }
                    );
                }
                return ListaPublicacoes;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetPublicationsByTitle(string sTitulo)
        {
            try
            {
                return await _pubsRepo.Listar_Titulo(sTitulo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
