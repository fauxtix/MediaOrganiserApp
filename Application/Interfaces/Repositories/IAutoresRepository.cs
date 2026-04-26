using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IAutoresRepository : IBaseRepository<Autor>
    {
        Task<IEnumerable<AutorDTO>> AutoresSemLivros();
        IEnumerable<PublicacoesVM> PublicacoesCatalogadas(int IdAutor);
        Task<IEnumerable<PublicacoesVM>> PublicacoesCatalogadas_Async(int Id);
        Task<bool> AutorComLivros(int IdAutor);
        Task<bool> AutorExiste(string NomeAutor);
        Task<IEnumerable<AutorDTO>> GetAutores_Extended_Async();
        Task<AutorDTO> GetAuthorById_Async(int Id);

        Task UpdateAutorAsync(Autor entity);
    }
}