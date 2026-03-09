using System;

namespace MediaOrganizerApp.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}
