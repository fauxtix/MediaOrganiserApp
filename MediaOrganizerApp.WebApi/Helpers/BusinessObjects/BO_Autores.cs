using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class BO_Autores : IBO_Autores
    {
        private readonly IAutoresRepository m_AutoresRepo;
  
        public BO_Autores(IAutoresRepository autoresRepo)
        {
            m_AutoresRepo = autoresRepo;
        }
        public async Task<List<AuthorReportModel>> GetBO_AuthorsPubs()
        {
            var autores = await m_AutoresRepo.GetAutores_Extended_Async(); // Todos
            var ListaAutores = new List<AuthorReportModel>(); 
            foreach (var obj in autores)
            {
                ListaAutores.Add(new AuthorReportModel
                {
                    AuthorName = obj.Descricao,
                    About = obj.Sobre,
                    AuthorImage = obj.ImagemAutor,
                    EditoraPub = obj.NomeEditora,
                    TituloPub = obj.Titulo,
                    TituloListagem = "Listagem geral de Autores",
                    Header1 = "Autor",
                    Header2 = "Titulos"
                });
            }
            return ListaAutores;
        }

        public List<AuthorReportModel> GetBO_AuthorsBooksAndPubs()
        {
            IEnumerable<PublicacoesVM> autores = m_AutoresRepo.PublicacoesCatalogadas(0); // Todos
            List<AuthorReportModel> ListaAutores = new List<AuthorReportModel>();
            foreach (var item in autores)
            {
                ListaAutores.Add(new AuthorReportModel
                {
                    AuthorName = item.Descricao,
                    TituloPub = item.Titulo,
                    EditoraPub = item.Descricao,
                    TituloListagem = "Listagem geral de Autores",
                    Header1 = "Autor",
                    Header2 = "Titulos"
                });
            }
            return ListaAutores;
        }

        public async Task<List<AuthorReportModel>> GetBO_AuthorsWithoutPubs()
        {
            var autores = await m_AutoresRepo.AutoresSemLivros();
            List<AuthorReportModel> ListaAutores = new List<AuthorReportModel>();
            foreach (var item in autores)
            {
                ListaAutores.Add(new AuthorReportModel
                {
                    AuthorName = item.Descricao,
                    TituloListagem = "Autores sem Publicações",
                    Header1 = "Autor",
                    Header2 = "Titulos"

                });
            }
            return ListaAutores;
        }
    }
}


