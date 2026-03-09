using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IFormatos_MediaRepository : IBaseRepository<Formato_Media>
    {
        Task<bool> FormatoComLivros(int IdFormato);
        Task<int> GetIdFormato(string sNome);
        Task<int> GetLastIdFormato();
        Task<string> GetNomeFormato(int idFormato);
        Task<List<Formato_Media>> Listar_Formato(string sDescricao);
        Task<List<MediaVM>> Media_Catalogados();
        Task<List<MediaVM>> Media_CatalogadosByFormato(int CodFormato);

        Task<bool> FormatoExiste(int Id);
    }
}