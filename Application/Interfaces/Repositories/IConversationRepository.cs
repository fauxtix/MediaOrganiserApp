using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Conversation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task InsertAsync(Conversation conv, CancellationToken cancellationToken = default);
        Task UpdateAsync(Conversation conv, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
