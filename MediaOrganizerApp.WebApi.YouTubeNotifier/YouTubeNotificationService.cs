namespace MediaOrganizerApp.WebApi.YouTubeNotifier
{
    using Google.Apis.Services;
    using Google.Apis.YouTube.v3;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class YouTubeNotificationService : BackgroundService
    {
        private readonly YouTubeService _youtubeService;
        private readonly string _apiKey = "YOUR_API_KEY";

        public YouTubeNotificationService()
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SubscribeToChannelsAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Reassinar a cada hora
            }
        }

        private async Task<List<string>> GetSubscribedChannelsAsync()
        {
            var subscriptions = new List<string>();

            var request = _youtubeService.Subscriptions.List("snippet");
            request.Mine = true;

            var response = await request.ExecuteAsync();
            foreach (var subscription in response.Items)
            {
                subscriptions.Add(subscription.Snippet.ResourceId.ChannelId);
            }

            return subscriptions;
        }

        private async Task SubscribeToChannel(string channelId)
        {
            var hubUrl = "https://pubsubhubbub.appspot.com/subscribe";
            var callbackUrl = "https://yourdomain.com/api/notifications"; // Substitua pelo seu domínio e endpoint de callback

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("hub.mode", "subscribe"),
            new KeyValuePair<string, string>("hub.topic", $"https://www.youtube.com/xml/feeds/videos.xml?channel_id={channelId}"),
            new KeyValuePair<string, string>("hub.callback", callbackUrl),
            new KeyValuePair<string, string>("hub.verify", "async")
        });

            await client.PostAsync(hubUrl, content);
        }

        private async Task SubscribeToChannelsAsync()
        {
            var channels = await GetSubscribedChannelsAsync();
            foreach (var channelId in channels)
            {
                await SubscribeToChannel(channelId);
            }
        }
    }
}
