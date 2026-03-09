using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IEditorasRepository : IBaseRepository<Editora>
    {
        Task<bool> EditoraComLivros(int IdEditora);
        Task<bool> EditoraExiste(int Id);
        Task<List<PublisherVM>> EditorasSemLivros();
        Task<int> GetIdEditora(string sNome);
        Task<int> GetLastIdEditora();
        Task<string> GetDescricao(int idEditora);
        Task<List<Editora>> Listar_Editora(string Descricao);
        Task<IDataReader> Listar_Editora_DR(string Descricao);
        List<PublicacoesVM> PublicacoesCatalogadas(int CodEditora);
        Task<List<PublicacoesVM>> PublicacoesCatalogadas_Async(int CodEditora);
    }
}