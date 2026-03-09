using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Data;
using System.Linq;
using System.Text;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class HelpManagerRepository : BaseRepository<HelpIndex>, IHelpManagerRepository
    {
        public HelpViewModel GetHelpData(int IdProjeto, string NomeForm)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = "SELECT * FROM vwHelp WHERE Id_Projeto = @IdProjeto AND NomeForm = @NomeForm";

                HelpViewModel result = connection.Query<HelpViewModel>(sql, new { IdProjeto, NomeForm }).SingleOrDefault();
                return result;
            }
        }

        public IDataReader GetHelpData(string sFormName)
        {
            throw new System.NotImplementedException();
        }

        public int GetIdProjeto(string NomeProjeto)
        {

            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = "SELECT Id FROM HelpIndex_Parent WHERE NomeProjeto = @NomeProjeto";

                var result = connection.Query<int>(sql, new { NomeProjeto }).FirstOrDefault();
                return result;
            }
        }

        public bool HelpExists(int IdParent, string NomeForm)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                StringBuilder sb = new();
                sb.Append("SELECT COUNT(1) FROM HelpIndex ");
                sb.Append("WHERE Id_Parent = @IdParent AND NomeForm = @NomeForm");
                int iFormExist = connection.Query<int>(sb.ToString(), new { IdParent, NomeForm }).FirstOrDefault();
                return iFormExist > 0;
            }
        }
    }
}
