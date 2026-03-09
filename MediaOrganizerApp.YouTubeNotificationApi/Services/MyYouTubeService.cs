using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.YouTubeNotificationApi.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaOrganizerApp.YouTubeNotificationApi.Services
{
    public class MyYouTubeService : IMyYouTubeService
    {
        private readonly YouTubeService _youtubeService;

        public MyYouTubeService(UserCredential credential)
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Keys.ApplicationName
            });
        }

        public async Task<List<VideoNotification>> GetNewVideosAsync()
        {
            var subscriptionsRequest = _youtubeService.Subscriptions.List("snippet,contentDetails");
            subscriptionsRequest.Mine = true;
            subscriptionsRequest.MaxResults = 50;

            var subscriptionsResponse = await subscriptionsRequest.ExecuteAsync();
            var newVideos = new List<VideoNotification>();

            foreach (var subscription in subscriptionsResponse.Items)
            {
                var channelId = subscription.Snippet.ResourceId.ChannelId;

                var searchRequest = _youtubeService.Search.List("snippet");
                searchRequest.ChannelId = channelId;
                searchRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                searchRequest.MaxResults = 10;

                var searchResponse = await searchRequest.ExecuteAsync();

                newVideos.AddRange(searchResponse.Items.Select(item => new VideoNotification
                {
                    VideoId = item.Id.VideoId,
                    Title = item.Snippet.Title,
                    PublishedAt = item.Snippet.PublishedAt.Value
                }));
            }

            return newVideos;
        }
    }
}
