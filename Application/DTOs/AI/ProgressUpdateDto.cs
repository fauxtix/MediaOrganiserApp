using System;

namespace MediaOrganizerApp.Application.DTOs.AI;

public enum ProgressChannel
{
    MediaInfo = 0,
    BooksInfo = 1,
    Chat = 2
}

public sealed class ProgressUpdateDto
{
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public int? Percent { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "info";
    public string? Service { get; set; }
    public string? Operation { get; set; }
    public string? ErrorCode { get; set; }
}
