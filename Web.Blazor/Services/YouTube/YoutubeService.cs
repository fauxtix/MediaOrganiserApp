using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;



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
                    if (!snippet.TryGetProperty("thumbnails", out var thumbs))
                        continue;
                    if (!thumbs.TryGetProperty("medium", out var thumbDefault))
                        continue;
                    if (!thumbDefault.TryGetProperty("url", out var thumbUrlProp))
                        continue;
                    DateOnly publishedAt = DateOnly.FromDateTime(snippet.GetProperty("publishedAt").GetDateTime());
                    var videoId = videoIdProp.GetString();
                    var title = titleProp.GetString();
                    var thumb = thumbUrlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(videoId) && !string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(thumb))
                        videos.Add(new YoutubeVideoDto { VideoId = videoId, Title = title, ThumbnailUrl = thumb, PublishedAt = publishedAt });
                }
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
    }
}
