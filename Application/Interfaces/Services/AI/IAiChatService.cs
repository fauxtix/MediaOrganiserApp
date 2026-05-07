using MediaOrganizerApp.Application.DTOs.AI;
using System.Collections.Generic;
using System.Threading;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI
{
    public interface IAiChatService
    {
        IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            string? correlationId,
            CancellationToken cancellationToken);
    }
}
