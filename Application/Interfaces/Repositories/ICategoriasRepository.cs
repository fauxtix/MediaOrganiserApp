using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface ICategoriasRepository : IBaseRepository<Categoria>
    {
        Task<bool> CategoriaComRegistos(int IdCategoria, bool m_CalledFromBooks);
        Task<string> GeNomeGenero(int idGenero);
        Task<int> GetIdCategoria(string sNome);
        Task<int> GetLastIdCategoria();
        Task<int> GetFirstIdCategoria(bool m_CalledFromBooks);
        Task<bool> CategoriaExiste(int id);
        Task<bool> CategoriaExiste(int tipo, string description);
        Task<List<Categoria>> Listar(bool m_CalledFromBooks);
        Task<List<Categoria>> Listar_Categoria(string sDescricao, int iTipoCat);
        Task<IDataReader> Listar_Categoria_DR(int iTipoCat);
        Task<List<CategoryVM>> Listar_Categoria_List(string sDescricao, bool CalledFromPubs);
        Task<List<Categoria>> Listar_ID(int CodCategoria, bool CalledFromBooks);
        Task<IEnumerable<MediaVM>> MediaCatalogados(int CodCategoria);
        Task<List<PublicacoesVM>> PublicacoesCatalogadas(int CodCategoria);
        Task<IEnumerable<CategoryVM>> GetCategorias_ExtendedAsync(int tipo);
        Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications();
        Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries();
        Task<IEnumerable<CategoryVM>> GetCategoriasAsync(int tipo);
    }
}