using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class Reports_CustomObjects : IReports_CustomObjects
    {
        private readonly IPublicacaoService pubSvc;
        private readonly IFormService formsSvc;

        public Reports_CustomObjects(IPublicacaoService pusbService, IFormService formsService)
        {
            pubSvc = pusbService;
            formsSvc = formsService;
        }

   
        public async Task<List<PublicationReportModel>> GetBooks(TipoPesquisa iTipo, int idCodigo, string sTitleQuery = "")
        {
            List<PubsWithDescriptions> publicacoes = new List<PubsWithDescriptions>();
            switch (iTipo)
            {
                case TipoPesquisa.Todos:
                    publicacoes = await pubSvc.Listar(Agrupamento.Todos);
                    break;
                case TipoPesquisa.Autor:
                    publicacoes = await pubSvc.Listar_IDAutor(idCodigo);
                    break;
                case TipoPesquisa.Editora:
                    publicacoes = await pubSvc.Listar_IDEditora(idCodigo);
                    break;
                case TipoPesquisa.Genero:
                    publicacoes = await pubSvc.Listar_IDCategoria(idCodigo);
                    break;
                case TipoPesquisa.Formato:
                    publicacoes = await pubSvc.Listar_IDFormato(idCodigo);
                    break;
                case TipoPesquisa.Arquivo:
                    publicacoes = await pubSvc.Listar_IDLocalArquivo(idCodigo);
                    break;
                case TipoPesquisa.Lido:
                    publicacoes = await pubSvc.Listar_Lidos(Convert.ToBoolean(idCodigo));
                    break;
                case TipoPesquisa.Titulo:
                    publicacoes = await pubSvc.Listar_PalavraTitulo(sTitleQuery);
                    break;
            }

            //dtBooks = oBooks.Listar();
            List<PublicationReportModel> ListaLivros = new List<PublicationReportModel>();
            foreach (var publicacao in publicacoes)
            {
                ListaLivros.Add(new PublicationReportModel
                {
                    Title = publicacao.Titulo,
                    Author = formsSvc.GetDescricao(publicacao.IdAutor, "Autor"),
                    Category = formsSvc.GetDescricao(publicacao.IdCategoria, "Categoria"),
                    Publisher = formsSvc.GetDescricao(publicacao.IdEditora, "Editora"),
                    Storage = formsSvc.GetDescricao(publicacao.IdLocalArquivo, "LocalArquivo"),
                    YearPublished = publicacao.AnoEdicao
                });
            }
            return ListaLivros;
        }

        public async Task<List<PubsWithDescriptions>> GetBooksData(TipoPesquisa iTipo, int idCodigo)
        {
            List<PubsWithDescriptions> publicacoes = new List<PubsWithDescriptions>();

            try
            {
                switch (iTipo)
                {
                    case TipoPesquisa.Todos:
                        publicacoes = await pubSvc.Listar(Agrupamento.Todos);
                        break;
                    case TipoPesquisa.Autor:
                        publicacoes = await pubSvc.Listar_IDAutor(idCodigo);
                        break;
                    case TipoPesquisa.Editora:
                        publicacoes = await pubSvc.Listar_IDEditora(idCodigo);
                        break;
                    case TipoPesquisa.Genero:
                        publicacoes = await pubSvc.Listar_IDCategoria(idCodigo);
                        break;
                    case TipoPesquisa.Formato:
                        publicacoes = await pubSvc.Listar_IDFormato(idCodigo);
                        break;
                    case TipoPesquisa.Arquivo:
                        publicacoes = await pubSvc.Listar_IDLocalArquivo(idCodigo);
                        break;
                    case TipoPesquisa.Pago:
                        publicacoes = await pubSvc.Listar_Pagos(Convert.ToBoolean(idCodigo));
                        break;
                    case TipoPesquisa.Lido:
                        publicacoes = await pubSvc.Listar_Lidos(Convert.ToBoolean(idCodigo));
                        break;
                    case TipoPesquisa.SemPaginas:
                        publicacoes = await pubSvc.Listar_SemPaginas();
                        break;
                    case TipoPesquisa.SemCapa:
                        publicacoes = await pubSvc.Listar_SemCapa();
                        break;
                    case TipoPesquisa.Duplicados:
                        publicacoes = (List<PubsWithDescriptions>)await pubSvc.Listar_Duplicados();
                        break;
                    case TipoPesquisa.Utilizador:
                        publicacoes = await pubSvc.Listar_UserID(idCodigo);
                        break;
                }

                return publicacoes;

            }
            catch
            {
                throw;
            }
        }

        public async Task<List<PubsWithDescriptions>> GetBooksByTitle(string sTitulo)
        {

            try
            {
                return await pubSvc.Listar_Titulo(sTitulo);

            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
