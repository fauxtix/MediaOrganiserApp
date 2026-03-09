using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using System.Data;

namespace Web.Blazor.Services.Common
{
    public class HelpManagerService : IHelpManagerService
    {
        private readonly IHelpManagerRepository repo;
        public HelpManagerService(IHelpManagerRepository repoHelp)
        {
            repo = repoHelp;
        }

        public IDataReader GetHelpData(string sFormName)
        {
            return repo.GetHelpData(sFormName);
        }

        public HelpViewModel GetHelpData(int IdProjeto, string NomeForm)
        {
            return repo.GetHelpData(IdProjeto, NomeForm);
        }

        public int GetIdProjeto(string NomeProjeto)
        {
            return repo.GetIdProjeto(NomeProjeto);
        }

        public bool HelpExists(int IdParent, string NomeForm)
        {
            return repo.HelpExists(IdParent, NomeForm);
        }
    }
}
