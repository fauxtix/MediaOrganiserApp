using System.Data;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();

    }
}