using MediaOrganizerApp.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IStatsRepository
    {
        Task<List<StatRecord>> GetTitulosDuplicados();
        Task<int> GetCountFromTableNotClassified(string sTabela, string sCampoGrupo, string sCampoAux, string sToCheck);
        Task<int> GetCountFromPubsWithoutPages();
        Task<int> GetCountFromPubsWithoutCover();
        Task<int> GetCountFromAuthorsWithoutPubs();
        Task<int> GetCountFromGenerosWithoutPubs();
        Task<int> GetCountFromEditorasWithoutPubs();
        Task<int> GetCountFromFormatosWithoutPubs();
        Task<int> GetCountFromLocaisArquivoWithoutPubs();
        Task<List<StatRecord>> GetStatEntities();

    }
}