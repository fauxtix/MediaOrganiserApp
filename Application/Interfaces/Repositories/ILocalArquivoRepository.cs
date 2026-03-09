using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface ILocalArquivoRepository : IBaseRepository<LocalArquivo>
    {
        Task<bool> LocalArquivoExiste(int Id);
        Task<int> GetIdLocalArquivo(string sNome);
        Task<int> GetLastIdLocalArquivo();
        Task<string> GetNomeLocalArquivo(int idLocal);
        Task<List<LocalArquivo>> Listar_LocalArquivo(string sDescricao);
        Task<bool> LocalArquivoComLivros(int IdLocalArquivo);
        Task<List<PublicacoesVM>> PublicacoesCatalogadas(int CodLocal);

        Task<IEnumerable<LocalArquivo>> GetAllAsync();
    }
}