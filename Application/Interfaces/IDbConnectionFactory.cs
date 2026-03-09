using System.Data;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateSqlConnection(string connectionName);

        IDbConnection CreateSqLiteConnection(string connectionName);

        IDbConnection CreateMySqlConnection(string connectionName);

        //IDbConnection CreatePostgreSqlConnection(string connectionName);
    }
}
