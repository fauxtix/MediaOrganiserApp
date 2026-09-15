using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface IConversationService
    {
        Task<IEnumerable<Conversation>> GetAllAsync(string? userId = null, bool isAdmin = false, CancellationToken ct = default);
        Task<Conversation?> GetByIdAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken ct = default);
        Task<Conversation> UpsertAsync(Conversation conv, string? userId = null, bool isAdmin = false, CancellationToken ct = default);
        Task DeleteAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken ct = default);
    }
}