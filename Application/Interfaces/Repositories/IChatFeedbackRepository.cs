using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs.AI;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    /// <summary>
    /// Persistence of chat feedback ratings (ChatFeedback table).
    /// Rows are isolated per user (UserID filtering at repository level).
    /// </summary>
    public interface IChatFeedbackRepository
    {
        Task UpsertAsync(ChatFeedbackDto feedback, CancellationToken ct = default);

        Task<List<ChatFeedbackDto>> GetByConversationAsync(
            string userId,
            string @interface,
            string? conversationId,
            CancellationToken ct = default);
    }
}