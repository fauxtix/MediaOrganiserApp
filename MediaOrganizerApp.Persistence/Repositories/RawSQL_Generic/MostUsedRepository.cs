using Dapper;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class MostUsedRepository : IMostUsedRepository
    {
        private readonly StringBuilder sb = new();
        private IDbConnection sqlConnection;

        public MostUsedRepository(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public async Task<string> GetOpcaoMaisUsada(string sCampoAAnalisar, string sTabelaAux,
            string sKeyAux, string sCampoDescrAux, string tabelaAUsar = "publicacao")
        {
            string sRet;
            sb.Clear();

            sb.Append($"SELECT {sCampoDescrAux} ");
            sb.Append($"from publicacao INNER JOIN {sTabelaAux} ");
            sb.Append($"ON {sCampoAAnalisar} = {sKeyAux} ");
            sb.Append($"GROUP BY {sCampoDescrAux} ");
            sb.Append($"ORDER BY COUNT({sCampoAAnalisar}) DESC ");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    sRet = await connection.QuerySingleAsync<string>(sb.ToString());
                    return sRet;
                }
            }
            catch (Exception exc)
            {
                //Logger.WriteLog(exc);
                throw new ApplicationException(exc.StackTrace);
            }
        }

        public string GetEditoraTop_CategoriaTop()
        {
            string sRet;
            sb.Clear();

            sb.Append("SELECT Descricao ");
            sb.Append("from publicacao INNER JOIN Editora ON Publicacao.idEditora = Editora.Id ");
            sb.Append("WHERE idCategoria = (SELECT idCategoria FROM vwGetTop1Category) ");
            sb.Append("GROUP BY Descricao ");
            sb.Append("ORDER BY COUNT(*) DESC ");

            try
            {
                using (IDbConnection connection = sqlConnection)
                {
                    sRet = connection.Query<string>(sb.ToString()).FirstOrDefault();
                    return sRet;
                }
            }
            catch
            {
                throw;
            }
        }

    }
}
