using MediaOrganizerApp.Application.DTOs.AI;

namespace MediaOrganizerApp.Application.Interfaces.Services.AI;

public interface IProgressPublisher
{
    Task PublishAsync(
        ProgressChannel channel,
        ProgressUpdateDto update,
        string? connectionId,
        CancellationToken cancellationToken = default);

    Task PublishChunkAsync(
        ProgressChannel channel,
        string chunk,
        string? connectionId,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task PublishDoneAsync(
        ProgressChannel channel,
        string? connectionId,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
