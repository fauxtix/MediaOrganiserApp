using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class BO_LocalArquivo : IBO_LocalArquivo
    {
        private ILocalArquivoRepository _locaisArquivoRepo;

        public BO_LocalArquivo(ILocalArquivoRepository locaisArquivoRepo)
        {
            _locaisArquivoRepo = locaisArquivoRepo;
        }

        public async Task<List<LookupTableReportModel>> GetLocaisArquivo()
        {
            var locaisArquivo = await _locaisArquivoRepo.Listar_LocalArquivo(""); // Todos
            List<LookupTableReportModel> ListaLocaisArquivo = new List<LookupTableReportModel>();
            foreach (var item in locaisArquivo)
            {
                ListaLocaisArquivo.Add(new LookupTableReportModel
                {
                    Id = item.Id,
                    Descricao = item.Descricao,
                    TituloListagem = "Listagem de Locais de Arquivo",
                    Header1 = "Id",
                    Header2 = "Descrição"
                });
            }
            return ListaLocaisArquivo;
        }
    }
}
