using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Web.Blazor.Static;

namespace Web.Blazor.Services.Common
{
    public interface IGetYoutubeVideoMetadata
    {
        Task<YouTubeVideoDetails> GetAlbumArtistMetadata(string searchRequest_query);
        Task<YouTubeVideoDetails> GetSingleVideoMetadata(string searchRequest_query);
        Task<YouTubeVideoDetails> GetVideoMetadata(string searchRequest_Id);
        Task<YouTubeVideoDetails> SearchByArtistAndSong(string artistName, string songName);
    }

    public class GetYoutubeVideoMetadata : IGetYoutubeVideoMetadata
    {
        public async Task<YouTubeVideoDetails> GetVideoMetadata(string searchRequest_Id = "")
        {
            using (var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YouTubeApiKey,
                ApplicationName = Keys.YouTubeApplicationName,
            }))
            {
                var searchRequest = youtubeService.Videos.List("snippet,contentDetails");
                searchRequest.Id = searchRequest_Id;

                VideoListResponse searchResponse;
                try
                {
                    searchResponse = await searchRequest.ExecuteAsync();

                }
                catch (HttpRequestException rxc)
                {

                    YouTubeVideoDetails videoDetails = new YouTubeVideoDetails()
                    {
                        Title = $"error searching video id:{searchRequest_Id} ( HttpRequestException: {rxc.Message})",
                        VideoId = "-1"

                    };
                    return videoDetails;
                }

                catch (Exception exc)
                {

                    YouTubeVideoDetails videoDetails = new YouTubeVideoDetails()
                    {
                        Title = $"error searching video id:{searchRequest_Id} ( Exception: {exc.Message})",
                        VideoId = "-1"
                    };
                    return videoDetails;
                }

                var youTubeVideo = searchResponse.Items.FirstOrDefault();

                if (youTubeVideo != null)
                {
                    TimeSpan YouTubeDuration = System.Xml.XmlConvert.ToTimeSpan(youTubeVideo.ContentDetails.Duration);
                    string sDuration = YouTubeDuration.ToString();

                    YouTubeVideoDetails videoDetails = new YouTubeVideoDetails()
                    {
                        VideoId = youTubeVideo.Id,
                        Description = youTubeVideo.Snippet.Description,
                        Title = youTubeVideo.Snippet.Title,
                        ChannelTitle = youTubeVideo.Snippet.ChannelTitle,
                        PublicationDate = youTubeVideo.Snippet.PublishedAt,
                        Duration = sDuration,
                        Thumbnail = youTubeVideo.Snippet.Thumbnails.Standard is not null ?
                            youTubeVideo.Snippet.Thumbnails.Standard.Url :
                            youTubeVideo.Snippet.Thumbnails.Medium is not null ?
                            youTubeVideo.Snippet.Thumbnails.Medium.Url :
                            youTubeVideo.Snippet.Thumbnails.Maxres is not null ?
                            youTubeVideo.Snippet.Thumbnails.Maxres.Url : "Images/No-image-available.png"
                    };
                    return videoDetails;
                }

                return null;
            }
        }
        public async Task<YouTubeVideoDetails> GetAlbumArtistMetadata(string searchRequest_query)
        {
            using (var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YouTubeApiKey,
                ApplicationName = Keys.YouTubeApplicationName

            }))
            {
                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = searchRequest_query;
                searchListRequest.MaxResults = 10;

                List<string> videos = new List<string>();
                List<string> channels = new List<string>();
                List<string> playlists = new List<string>();
                List<string> thumbnails = new List<string>();

                var searchListResponse = await searchListRequest.ExecuteAsync();

                // Add each result to the appropriate list, and then display the lists of
                // matching videos, channels, and playlists.
                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                            break;

                        case "youtube#channel":
                            channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                            break;

                        case "youtube#playlist":
                            playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                            break;
                    }
                }

                var youTubeVideo = searchListResponse.Items.FirstOrDefault();
                YouTubeVideoDetails videoDetails = new YouTubeVideoDetails()
                {
                    VideoId = youTubeVideo.Id.VideoId,
                    Description = youTubeVideo.Snippet.Description,
                    Title = youTubeVideo.Snippet.Title,
                    ChannelTitle = youTubeVideo.Snippet.ChannelTitle,
                    PublicationDate = youTubeVideo.Snippet.PublishedAt,
                    Thumbnail = youTubeVideo.Snippet.Thumbnails.Standard is not null ?
                                youTubeVideo.Snippet.Thumbnails.Standard.Url :
                                youTubeVideo.Snippet.Thumbnails.Medium is not null ?
                                youTubeVideo.Snippet.Thumbnails.Medium.Url :
                                youTubeVideo.Snippet.Thumbnails.Maxres is not null ?
                                youTubeVideo.Snippet.Thumbnails.Maxres.Url : "Images/No-image-available.png"
                };

                return videoDetails;

            }
        }

        public async Task<YouTubeVideoDetails> GetSingleVideoMetadata(string searchRequest_query)
        {
            try
            {
                using (var YouTubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = Keys.YouTubeApiKey,
                    ApplicationName = Keys.YouTubeApplicationName

                }))
                {
                    var searchListRequest = YouTubeService.Videos.List("snippet");
                    searchListRequest.Id = searchRequest_query;
                    searchListRequest.MaxResults = 1;

                    var searchListResponse = await searchListRequest.ExecuteAsync();

                    var youTubeVideo = searchListResponse.Items.FirstOrDefault(); // redundant ? (MaxResults = 1...)
                    if (youTubeVideo is not null)
                    {
                        YouTubeVideoDetails videoDetail = new YouTubeVideoDetails()
                        {
                            VideoId = youTubeVideo.Id,
                            Description = youTubeVideo.Snippet.Description,
                            Title = youTubeVideo.Snippet.Title,
                            ChannelTitle = youTubeVideo.Snippet.ChannelTitle,
                            PublicationDate = youTubeVideo.Snippet.PublishedAt,
                            Thumbnail = youTubeVideo.Snippet.Thumbnails.Standard is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Standard.Url :
                                        youTubeVideo.Snippet.Thumbnails.Medium is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Medium.Url :
                                        youTubeVideo.Snippet.Thumbnails.Maxres is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Maxres.Url : "Images/No-image-available.png"
                        };

                        return videoDetail;
                    }
                    else
                        return null;

                }

            }
            catch
            {

                throw;
            }
        }

        public async Task<YouTubeVideoDetails> SearchByArtistAndSong(string artistName, string songName)
        {
            try
            {
                using (var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = Keys.YouTubeApiKey,
                    ApplicationName = Keys.YouTubeApplicationName
                }))
                {
                    // Construct a search query combining artist and song name
                    string searchQuery = $"{artistName} {songName} music video";

                    var searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.Q = searchQuery;
                    searchListRequest.MaxResults = 1;

                    var searchListResponse = await searchListRequest.ExecuteAsync();

                    var youTubeVideo = searchListResponse.Items.FirstOrDefault();

                    if (youTubeVideo is not null)
                    {
                        var videoId = youTubeVideo.Id.VideoId;
                        var duration = await GetVideoDuration(videoId);
                        YouTubeVideoDetails videoDetail = new YouTubeVideoDetails()
                        {
                            VideoId = videoId,
                            Description = youTubeVideo.Snippet.Description,
                            Title = youTubeVideo.Snippet.Title,
                            Duration = duration,
                            ChannelTitle = youTubeVideo.Snippet.ChannelTitle,
                            PublicationDate = youTubeVideo.Snippet.PublishedAt,
                            Thumbnail = youTubeVideo.Snippet.Thumbnails.Standard is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Standard.Url :
                                        youTubeVideo.Snippet.Thumbnails.Medium is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Medium.Url :
                                        youTubeVideo.Snippet.Thumbnails.Maxres is not null ?
                                        youTubeVideo.Snippet.Thumbnails.Maxres.Url : "Images/No-image-available.png"
                        };

                        return videoDetail;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        private async Task<string> GetVideoDuration(string searchRequest_Id)
        {
            string duration = "";
            using (var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Keys.YouTubeApiKey,
                ApplicationName = Keys.YouTubeApplicationName

            }))
            {

                var searchRequest = youtubeService.Videos.List("snippet,contentDetails");
                searchRequest.Id = searchRequest_Id;

                var searchResponse = await searchRequest.ExecuteAsync();
                var youTubeVideo = searchResponse.Items.FirstOrDefault();

                if (youTubeVideo != null)
                {
                    TimeSpan YouTubeDuration = System.Xml.XmlConvert.ToTimeSpan(youTubeVideo.ContentDetails.Duration);
                    duration = YouTubeDuration.ToString();

                }
            }
            return duration;
        }
    }
}
