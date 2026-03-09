using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public class BO_Formatos : IBO_Formatos
    {
        private readonly IFormatosRepository _formatosRepo;

  
        public BO_Formatos(IFormatosRepository formatosRepo)
        {
            _formatosRepo = formatosRepo;
        }

        public async Task<List<LookupTableReportModel>> GetFormats()
        {
            var formatos = await _formatosRepo.Listar_Formato(""); // Todos
            List<LookupTableReportModel> ListaFormatos = new();
            foreach (var item in formatos)
            {
                ListaFormatos.Add(new LookupTableReportModel
                {
                    Id = item.Id,
                    Descricao = item.Descricao,
                    TituloListagem = "Listagem de Formatos",
                    Header1 = "Id",
                    Header2 = "Descrição"                    
                });
            }
            return ListaFormatos;
        }
    }
}
