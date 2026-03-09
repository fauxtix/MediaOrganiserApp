using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System.Data;
using System.Linq;

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class HelpManagerRepository : SQLServerBaseRepository<HelpIndex>, IHelpManagerRepository
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

        public IDataReader GetHelpData(string NomeForm)
        {
            string sProcedName = "usp_Help_GetData";
            using (var connection = ConnectionManager.GetConnection())
            {

                var result = connection.ExecuteReader(sProcedName, 
                    new { NomeForm }, commandType: CommandType.StoredProcedure);
                return result;
            }
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
            string sProcedName = "usp_Help_HasData";

            using (var connection = ConnectionManager.GetConnection())
            {
                int iFormExist = connection.Query<int>(sProcedName, 
                    new { IdParent, NomeForm }, commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
                return iFormExist > 0;
            }
        }
    }
}
