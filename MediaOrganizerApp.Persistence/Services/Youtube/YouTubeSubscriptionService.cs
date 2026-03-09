using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MediaOrganizerApp.Persistence.Services.Youtube.Settings;
using System;
using Google.Apis.Util.Store;
using System.Threading;

namespace MediaOrganizerApp.Persistence.Services.Youtube
{

    [Authorize]
    public class YouTubeSubscriptionService : IYouTubeSubscriptionService
    {
        private static readonly string[] Scopes = { YouTubeService.Scope.YoutubeReadonly };

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDapperContext _dbContext;

        public YouTubeSubscriptionService(IHttpContextAccessor httpContextAccessor, IDapperContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<IList<Google.Apis.YouTube.v3.Data.Subscription>> GetSubscriptionsAsync()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YouTubeApiKey,
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
                ApplicationName = "MediaOrganizer"
            });

            try
            {
                var clientSecrets = new ClientSecrets
                {
                    ClientId = Keys.ClientId,
                    ClientSecret = Keys.ClientSecret,
                };
                string credPath = "token.json"; // Path to store the token


                var output = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                );

                var request = youtubeService.Subscriptions.List("snippet,contentDetails");
                request.Mine = true;
                var response = await request.ExecuteAsync();

                return response.Items;

            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                throw;
            }
        }

        public async Task<IList<YouTubeSubscription>> GetAndSaveSubscriptionsAsync()
        {
            var apiSubscriptions = await GetSubscriptionsAsync();
            var subscriptions = new List<YouTubeSubscription>();

            using (var connection = ConnectionManager.GetConnection())
            {
                foreach (var item in apiSubscriptions)
                {
                    var subscription = new YouTubeSubscription
                    {
                        Title = item.Snippet.Title,
                        ChannelId = item.Snippet.ResourceId.ChannelId
                    };

                    subscriptions.Add(subscription);

                    // Salvar subscrição no banco de dados usando Dapper
                    var sql = "INSERT INTO YouTubeSubscriptions (Title, ChannelId) VALUES (@Title, @ChannelId)";
                    await connection.ExecuteAsync(sql, subscription);
                }
            }

            return subscriptions;
        }
        public async Task SaveSubscriptionsAsync()
        {
            var subscriptions = await GetSubscriptionsAsync();
            StringBuilder sb = new StringBuilder();

            foreach (var item in subscriptions)
            {
                var subscription = new YouTubeSubscription
                {
                    Title = item.Snippet.Title,
                    ChannelId = item.Snippet.ResourceId.ChannelId
                };

                sb.Append("INSERT INTO YouTubeSubscription (Title, ChannelId ");
                sb.Append("Values (");
                sb.Append($"{subscription.Title}, {subscription.ChannelId}");
                sb.Append(")");
                using (var connection = ConnectionManager.GetConnection())
                {
                    await connection.ExecuteAsync(sb.ToString());
                }
            }

            sb.Clear();

            // Salvar detalhes dos vídeos para cada canal
            foreach (var item in subscriptions)
            {
                var channelId = item.Snippet.ResourceId.ChannelId;
                var videos = await GetVideosByChannelIdAsync(channelId);

                foreach (var video in videos)
                {
                    // Adicionar os detalhes dos vídeos ao banco de dados
                    var videoDetails = new YouTubeVideo
                    {
                        ChannelId = channelId,
                        VideoId = video.VideoId,
                        VideoTitle = video.VideoTitle,
                        VideoDescription = video.VideoDescription,
                        PublishedAt = video.PublishedAt,
                        ThumbnailUrl = video.ThumbnailUrl
                    };

                    sb.Append("INSERT INTO YouTubeVideo (ChannelId, VideoId, VideoTitle,  ");
                    sb.Append("VideoDescription, PublishedAt, ThumbnailUrl ");
                    sb.Append("Values (");
                    sb.Append($"{videoDetails.ChannelId}, {videoDetails.VideoId},");
                    sb.Append($"{videoDetails.VideoTitle}, {videoDetails.VideoDescription},");
                    sb.Append($"{videoDetails.PublishedAt}, {videoDetails.ThumbnailUrl}");
                    sb.Append(")");

                    using (var connection = ConnectionManager.GetConnection())
                    {
                        await connection.ExecuteAsync(sb.ToString());
                    }
                }
            }
        }

        public async Task<IList<YouTubeVideo>> GetVideosByChannelIdAsync(string channelId)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
                ApplicationName = "MediaOrganizerApp"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = channelId;
            searchListRequest.MaxResults = 10;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            var videos = new List<YouTubeVideo>();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    var videoRequest = youtubeService.Videos.List("snippet,contentDetails,statistics");
                    videoRequest.Id = searchResult.Id.VideoId;

                    var videoResponse = await videoRequest.ExecuteAsync();
                    if (videoResponse.Items.Count > 0)
                    {
                        var video = videoResponse.Items[0];
                        videos.Add(new YouTubeVideo
                        {
                            VideoId = video.Id,
                            VideoTitle = video.Snippet.Title,
                            VideoDescription = video.Snippet.Description,
                            PublishedAt = video.Snippet.PublishedAt,
                            ThumbnailUrl = video.Snippet.Thumbnails.Default__.Url,
                            ChannelId = channelId
                        });
                    }
                }
            }

            return videos;
        }
    }
}
