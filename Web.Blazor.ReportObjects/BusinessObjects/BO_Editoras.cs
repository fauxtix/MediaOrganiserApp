using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public class BO_Editoras : IBO_Editoras
    {
        private readonly IEditorasRepository _editorasRepo;

        public BO_Editoras(IEditorasRepository editorasRepo)
        {
            _editorasRepo = editorasRepo;
        }

        public async Task<List<PublisherReportModel>> GetPublishers()
        {
            var editoras = await _editorasRepo.Listar_Editora(""); // Todos
            List<PublisherReportModel> ListaEditoras = new List<PublisherReportModel>();
            foreach (var item in editoras)
            {
                ListaEditoras.Add(new PublisherReportModel
                {
                    Id = item.Id,
                    Descricao = item.Descricao,
                    Morada = item.Morada,
                    Telefone = item.Telefone,
                    EMail = item.EMail,
                    Site = item.Site,
                    TituloListagem = "Listagem de Editoras",
                    Header1 = "Editora",
                    Header2 = "eMail",
                    Header3 = "Contacto"                   
                });
            }
            return ListaEditoras;
        }

        public async Task<List<PublisherReportModel>> GetPublishersWithoutPubs()
        {
            var editoras = await _editorasRepo.EditorasSemLivros();
            List<PublisherReportModel> ListaEditoras = new List<PublisherReportModel>();
            foreach (var item in editoras)
            {
                ListaEditoras.Add(new PublisherReportModel
                {
                    Id = item.Id,
                    Descricao = item.Descricao,
                    Morada = item.Morada,
                    Telefone = item.Telefone,
                    EMail = item.EMail,
                    Site = item.Site
                });
            }
            return ListaEditoras;
        }
    }
}
