using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetAllAsync(string? userId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
        Task<Conversation?> GetByIdAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
        Task InsertAsync(Conversation conv, CancellationToken cancellationToken = default);
        Task UpdateAsync(Conversation conv, string? userId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken cancellationToken = default);
    }
}