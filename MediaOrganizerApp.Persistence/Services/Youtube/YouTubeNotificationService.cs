//using MediaOrganizerApp.Domain.Entities;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//using MediaOrganizerApp.Persistence.Services.Youtube.Data;
//using YourAppNamespace.Services;

//namespace MediaOrganizerApp.Persistence.Services.Youtube
//{
//    public class YouTubeNotificationService    {
//        private readonly IYouTubeSubscriptionService _youtubeService;
//        private readonly IServiceScopeFactory _serviceScopeFactory;
//        private readonly ILogger<YouTubeNotificationService> _logger;

//        public YouTubeNotificationService(
//            IYouTubeSubscriptionService youtubeService,
//            IServiceScopeFactory serviceScopeFactory,
//            ILogger<YouTubeNotificationService> logger)
//        {
//            _youtubeService = youtubeService;
//            _serviceScopeFactory = serviceScopeFactory;
//            _logger = logger;
//        }

//        protected  async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                _logger.LogInformation("Checking for new YouTube posts...");

//                var newVideos = await _youtubeService.GetSubscriptionsAsync();

//                using (var scope = _serviceScopeFactory.CreateScope())
//                {
//                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                    foreach (var video in newVideos)
//                    {
//                        if (!dbContext.VideoNotifications.Any(v => v.VideoId == video.Id))
//                        {
//                            dbContext.VideoNotifications.Add(new YouTubeSubscription
//                            {
//                                VideoId = video.Id,
//                                Title = video.Title,
//                                PublishedAt = video.PublishedAt
//                            });
//                        }
//                    }
//                    await dbContext.SaveChangesAsync();
//                }

//                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//            }
//        }
//    }
//}
