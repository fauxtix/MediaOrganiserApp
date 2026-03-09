using System;
using System.Data;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IDapperContext
    {
        public IDbConnection CreateConnection();
        public void Execute(Action<IDbConnection> @event);
    }
}
