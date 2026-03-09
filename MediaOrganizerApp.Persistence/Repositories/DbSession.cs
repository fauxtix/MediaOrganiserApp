using System;
using System.Data;
using MediaOrganizerApp.Application.Factories;

namespace MediaOrganizerApp.Persistence.Repositories
{
    public sealed class DbSession : IDisposable
    {
        public IDbConnection Connection { get; }
        public IDbTransaction Transaction { get; set; }

        public DbSession()
        {
            Connection = ConnectionManager.GetConnection();
            Connection.Open();
        }

        public void Dispose() => Connection?.Dispose();
    }
}
