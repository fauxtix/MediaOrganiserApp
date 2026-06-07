using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.PlayLists;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Charts.Chart.Internal;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;



namespace Web.Blazor.Services.YouTube
{
    public class YoutubeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<YoutubeService> _logger;
        private readonly string _apiKey;

        public YoutubeService(HttpClient httpClient, string apiKey, ILogger<YoutubeService> logger)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _logger = logger;
        }

        public async Task<List<YoutubeVideoDto>> GetVideosFromPlaylist(string playlistIdOrUrl)
        {
            try
            {
                string playlistId = ExtractPlaylistId(playlistIdOrUrl);
                var url = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistId}&maxResults=50&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var videos = new List<YoutubeVideoDto>();
                foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
                {
                    if (!item.TryGetProperty("snippet", out var snippet))
                        continue;
                    if (!snippet.TryGetProperty("resourceId", out var resourceId))
                        continue;
                    if (!resourceId.TryGetProperty("videoId", out var videoIdProp))
                        continue;
                    if (!snippet.TryGetProperty("title", out var titleProp))
                        continue;
                    if (!snippet.TryGetProperty("description", out var descriptionProp))
                        continue;
                    if (!snippet.TryGetProperty("thumbnails", out var thumbs))
                        continue;
                    if (!thumbs.TryGetProperty("medium", out var thumbDefault))
                        continue;
                    if (!thumbDefault.TryGetProperty("url", out var thumbUrlProp))
                        continue;
                    DateOnly publishedAt = DateOnly.FromDateTime(snippet.GetProperty("publishedAt").GetDateTime());
                    var videoId = videoIdProp.GetString();
                    var title = titleProp.GetString();
                    var description = descriptionProp.GetString();
                    var thumb = thumbUrlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(videoId) && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(thumb))
                        videos.Add(
                            new YoutubeVideoDto { 
                                VideoId = videoId, 
                                Title = title,  
                                Description = description, 
                                ThumbnailUrl = thumb, 
                                PublishedAt = publishedAt });
                }

                await FillVideoDurations(videos);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        private string ExtractPlaylistId(string input)
        {
            // Se for só o ID, retorna direto
            if (!input.Contains("list="))
                return input.Trim();

            // Extrai o valor do parâmetro 'list' de uma URL
            var query = input;
            int idx = query.IndexOf("list=");
            if (idx == -1)
                return input.Trim();
            idx += 5;
            int end = query.IndexOf('&', idx);
            string playlistId = end > idx ? query.Substring(idx, end - idx) : query.Substring(idx);
            // Remove possíveis fragmentos (#...)
            int hashIdx = playlistId.IndexOf('#');
            if (hashIdx >= 0)
                playlistId = playlistId.Substring(0, hashIdx);
            return playlistId.Trim();
        }

        public async Task<List<YoutubePlaylistItem>> GetPlaylistsAsync(string query)
        {
            string url =
                $"https://www.googleapis.com/youtube/v3/search" +
                $"?part=snippet" +
                $"&q={Uri.EscapeDataString(query)}" +
                $"&type=playlist" +
                $"&maxResults=10" +
                $"&key={_apiKey}";

            var json = await _httpClient.GetStringAsync(url);

            return ParsePlaylistItems(json);
        }


        public List<YoutubePlaylistItem> ParsePlaylistItems(string json)
        {
            var data = JsonSerializer.Deserialize<YouTubeSearchResponse>(json);

            if (data?.Items == null)
                return new List<YoutubePlaylistItem>();

            return data.Items
                .Where(x => x?.Id?.PlaylistId != null)
                .Select(x => new YoutubePlaylistItem
                {
                    Id = x.Id.PlaylistId,
                    Title = x.Snippet?.Title ?? "Sem título"
                })
                .ToList();
        }

        private async Task FillVideoDurations(List<YoutubeVideoDto> videos)
        {
            if (!videos.Any())
                return;

            var ids = string.Join(",", videos.Select(v => v.VideoId));

            var url =
                $"https://www.googleapis.com/youtube/v3/videos" +
                $"?part=contentDetails" +
                $"&id={ids}" +
                $"&key={_apiKey}";

            var json = await _httpClient.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);

            var durations = doc.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .ToDictionary(
                    item => item.GetProperty("id").GetString(),
                    item => XmlConvert.ToTimeSpan(
                        item.GetProperty("contentDetails")
                            .GetProperty("duration")
                            .GetString()));

            foreach (var video in videos)
            {
                if (durations.TryGetValue(video.VideoId, out var duration))
                {
                    video.Duration = duration;
                }
            }
        }
    }
}
