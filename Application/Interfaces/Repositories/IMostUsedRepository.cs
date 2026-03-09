using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IMostUsedRepository
    {
        string GetEditoraTop_CategoriaTop();
        Task<string> GetOpcaoMaisUsada(string sCampoAAnalisar, string sTabelaAux,
                    string sKeyAux, string sCampoDescrAux, string tabelaAUsar = "publicacao");
    }
}
