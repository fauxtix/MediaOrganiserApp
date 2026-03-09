using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IFormatosRepository : IBaseRepository<Formato>
    {
        Task<bool> FormatoComLivros(int IdFormato);
        Task<bool> FormatoExiste(int Id);
        Task<int> GetIdFormato(string sNome);
        Task<int> GetLastIdFormato();
        Task<string> GetNomeFormato(int idFormato);
        Task<List<Formato>> Listar_Formato(string sDescricao);
        List<PublicacoesVM> PublicacoesCatalogadas(int CodFormato);
    }
}