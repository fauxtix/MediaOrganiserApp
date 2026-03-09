using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.YouTubeNotificationApi.Services;

public interface IMyYouTubeService
{
    Task<List<VideoNotification>> GetNewVideosAsync();
}
