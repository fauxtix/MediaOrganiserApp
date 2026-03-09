using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface ICategoriasService
    {
        Task<bool> CreateCategory(CategoryVM categoria);
        Task<bool> DeleteCategory(int id);
        Task<IEnumerable<CategoryVM>> GetAllAsync(CategoryType category);
        Task<CategoryVM> GetCategoriesById(int id);
        Task<bool> UpdateCategory(int id, CategoryVM categoria);
        Task<List<CategoryVM>> Listar(int tipo);
        Task<List<CategoryVM>> ListByType(Query_TipoMedia tipoMedia);

        Task<int> GetIdCategoria(string description);
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int CodAutor);
        Task<bool> CategoriaComRegistos(int id);
        string RegistoComErros(CategoryVM categoria);

        List<MediaVM> MediaCatalogados(int CodCategoria);

        Task<IEnumerable<CategoryVM>> GetCategoriesWithPublications();
        Task<IEnumerable<CategoryVM>> GetCategoriesWithMediaEntries();
        Task<List<CategoryVM>> Listar_Categoria_List(string sDescricao, bool CalledFromPubs);
        Task<List<CategoryVM>> Listar_Categoria(bool CalledFromPubs);
        Task<IEnumerable<CategoryVM>> AllAsync(CategoryType category);
    }
}