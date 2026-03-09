using MediaOrganizerApp.Application.DTOs;
using System.Data;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IHelpManagerRepository
    {
        IDataReader GetHelpData(string sFormName);
        HelpViewModel GetHelpData(int IdProjeto, string NomeForm);
        int GetIdProjeto(string NomeProjeto);
        bool HelpExists(int IdParent, string NomeForm);
    }
}