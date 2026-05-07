using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MediaOrganizerApp.WebApi.Services;

public sealed class SignalRProgressPublisher : IProgressPublisher
{
    private readonly IHubContext<MediaInfoProgressHub> _mediaHub;
    private readonly IHubContext<GoogleBooksProgressHub> _booksHub;

    public SignalRProgressPublisher(
        IHubContext<MediaInfoProgressHub> mediaHub,
        IHubContext<GoogleBooksProgressHub> booksHub)
    {
        _mediaHub = mediaHub;
        _booksHub = booksHub;
    }

    public async Task PublishAsync(
        ProgressChannel channel,
        ProgressUpdateDto update,
        string? connectionId,
        CancellationToken cancellationToken = default)
    {
        update.TimestampUtc = DateTime.UtcNow;

        if (channel == ProgressChannel.MediaInfo)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                await _mediaHub.Clients.Client(connectionId)
                    .SendAsync("ReceiveProgress", connectionId, update.Message, cancellationToken);
                await _mediaHub.Clients.Client(connectionId)
                    .SendAsync("ReceiveProgressEvent", update, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(update.CorrelationId))
            {
                await _mediaHub.Clients.Group(GroupName(update.CorrelationId))
                    .SendAsync("ReceiveProgress", connectionId, update.Message, cancellationToken);
                await _mediaHub.Clients.Group(GroupName(update.CorrelationId))
                    .SendAsync("ReceiveProgressEvent", update, cancellationToken);
            }

            return;
        }

        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            await _booksHub.Clients.Client(connectionId)
                .SendAsync("ReceiveProgress", update.Message, cancellationToken);
            await _booksHub.Clients.Client(connectionId)
                .SendAsync("ReceiveProgressEvent", update, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(update.CorrelationId))
        {
            await _booksHub.Clients.Group(GroupName(update.CorrelationId))
                .SendAsync("ReceiveProgress", update.Message, cancellationToken);
            await _booksHub.Clients.Group(GroupName(update.CorrelationId))
                .SendAsync("ReceiveProgressEvent", update, cancellationToken);
        }
    }

    public async Task PublishChunkAsync(
        ProgressChannel channel,
        string chunk,
        string? connectionId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        if (channel == ProgressChannel.MediaInfo)
            return;

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _booksHub.Clients.Client(connectionId).SendAsync("ReceiveChunk", chunk, cancellationToken);

        if (!string.IsNullOrWhiteSpace(correlationId))
            await _booksHub.Clients.Group(GroupName(correlationId)).SendAsync("ReceiveChunk", chunk, cancellationToken);
    }

    public async Task PublishDoneAsync(
        ProgressChannel channel,
        string? connectionId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        if (channel == ProgressChannel.MediaInfo)
            return;

        if (!string.IsNullOrWhiteSpace(connectionId))
            await _booksHub.Clients.Client(connectionId).SendAsync("ReceiveDone", cancellationToken);

        if (!string.IsNullOrWhiteSpace(correlationId))
            await _booksHub.Clients.Group(GroupName(correlationId)).SendAsync("ReceiveDone", cancellationToken);
    }

    private static string GroupName(string correlationId) => $"corr:{correlationId}";
}
