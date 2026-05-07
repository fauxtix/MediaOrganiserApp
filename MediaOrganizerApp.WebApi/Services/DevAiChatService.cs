using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;

namespace MediaOrganizerApp.WebApi.Services
{
    /// <summary>
    /// Minimal development placeholder for IAiChatService so DI resolves and the API returns a clear message.
    /// Replace with a real provider (Ollama/OpenAI) for production behavior.
    /// </summary>
    public sealed class DevAiChatService : IAiChatService
    {
        public async IAsyncEnumerable<string> StreamChatAsync(
            List<ChatMessageDto> messages,
            string? connectionId,
            string? correlationId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // One obvious chunk so clients see a response and DI is satisfied.
            yield return "DEV_PLACEHOLDER: No IAiChatService implementation is registered. Implement a real provider (Ollama/OpenAI) and register it in WebApi Program.cs.";
            await Task.CompletedTask;
        }
    }
}
