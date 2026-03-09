using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IMostUsedService
    {
        string GetEditoraTop_CategoriaTop();
        Task<string> GetOpcaoMaisUsada(string field, string table, string key, string description, string tabelaAUsar = "publicacao");
    }
}