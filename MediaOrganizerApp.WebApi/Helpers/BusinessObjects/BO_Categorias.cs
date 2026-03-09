using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Helpers.BusinessObjects
{
    public class BO_Categorias : IBO_Categorias
    {
        private readonly ICategoriasRepository m_CategoriasRepo;
        public BO_Categorias(ICategoriasRepository categoriasRepo)
        {
            m_CategoriasRepo = categoriasRepo;
        }
        public async Task<List<CategoryReportModel>> GetBO_Categories(bool CalledFromPubs)
        {
            var categorias = await m_CategoriasRepo.GetCategorias_ExtendedAsync(CalledFromPubs ? 1 : 2); // Todos
            List<CategoryReportModel> ListaCategorias = new List<CategoryReportModel>();
            foreach (var item in categorias)
            {
                ListaCategorias.Add(new CategoryReportModel
                {
                    Id = item.Id,
                    Descricao = item.Descricao,
                    TipoCategoria = item.TipoCategoria,
                    DescricaoTipoCategoria = item.DescricaoTipoCategoria,
                    TituloListagem = "Listagem de Categorias",
                    Header1 = "Código",
                    Header2 = "Descrição"
                });
            }
            return ListaCategorias;
        }
    }
}
