using Dapper;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System.Data;
using System.Text;

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class FormsHelper : IFormsHelper
    {
        private IDbConnection sqlConnection;
        public FormsHelper(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }
        public IDataReader GetCategoriasPublicacoes()
        {
            StringBuilder sb = new();
            sb.Append("SELECT c.Descricao from publicacao l ");
            sb.Append("INNER JOIN Categoria c ");
            sb.Append("ON l.IdCategoria = c.Id ");
            sb.Append("GROUP by c.descricao ");
            sb.Append("ORDER BY c.descricao");

            return sqlConnection.ExecuteReader(sb.ToString());
        }

        public string GetDescricao(int Id, string sTabela)
        {
            StringBuilder sb = new();
            sb.Append("SELECT Descricao ");
            sb.Append($"FROM {sTabela} ");
            sb.Append("WHERE Id = @Id");
            using (IDbConnection connection = sqlConnection)
            {
                var sDesc = connection.QuerySingle<string>(sb.ToString(), new { Id });
                return sDesc;
            }
        }

        // Obtém descrição das tabelas auxialiares (para preenchimento/posicionamento das combo-boxes)
        public string GetDescricaoCategoria(int Id, int TipoCategoria)
        {
            StringBuilder sb = new();
            sb.Append("SELECT Descricao ");
            sb.Append($"FROM Categoria ");
            sb.Append("WHERE Id = @Id AND TipoCategoria = @TipoCategoria");
            using (IDbConnection connection = sqlConnection)
            {
                var sDesc = connection.ExecuteScalar<string>(sb.ToString(), new { Id, TipoCategoria });
                return sDesc;
            }
        }
    }
}
