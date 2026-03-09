using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.YouTubeNotificationApi.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaOrganizerApp.YouTubeNotificationApi.Services
{
    public class YouTubeNotificationService : BackgroundService
    {
        private readonly IMyYouTubeService _youtubeService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<YouTubeNotificationService> _logger;

        public YouTubeNotificationService(
            IMyYouTubeService youtubeService,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<YouTubeNotificationService> logger)
        {
            _youtubeService = youtubeService;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking for new YouTube posts...");

                var newVideos = await _youtubeService.GetNewVideosAsync();

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach (var video in newVideos)
                    {
                        if (!dbContext.VideoNotifications.Any(v => v.VideoId == video.VideoId))
                        {
                            dbContext.VideoNotifications.Add(new VideoNotification
                            {
                                VideoId = video.VideoId,
                                Title = video.Title,
                                PublishedAt = video.PublishedAt
                            });
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
