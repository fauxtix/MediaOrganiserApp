using Web.Blazor.Static;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web.Blazor.Services.Common;
using Web.Blazor.Services.ApiWrappers;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Data
{
    public interface IMediaThumbnailsService
    {
        Task UpdateMediaCoverRecords();
        Task<(string, string)> UpdateMediaCoverRecord(int Id);
    }

    public class MediaThumbnailsService : IMediaThumbnailsService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<MediaThumbnailsService> _logger;
        private readonly IGetYoutubeVideoMetadata _YouTubeServices;
        private string _Uri;
        private readonly IConfiguration _env;

        public MediaThumbnailsService(HttpClient client, 
            ILogger<MediaThumbnailsService> logger, 
            IGetYoutubeVideoMetadata YouTubeServices,
            IConfiguration Env)
        {
            httpClient = client;
            _logger = logger;
            _env = Env;
            httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/media";

            _YouTubeServices = YouTubeServices;
        }

        public async Task UpdateMediaCoverRecords()
        {
            string uriGetAll = $"{_Uri}/GetAllMusicFiles";
            try
            {
                var _queryResult = await httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uriGetAll);

                foreach (var item in _queryResult)
                {
                    int iPos = item.FileUrl.IndexOf("v=") + 2;
                    string videoId = item.FileUrl.Substring(iPos);
                    var output = await _YouTubeServices.GetVideoMetadata(videoId);

                    var searchQuery = $"{videoId}";
                    var videoMetadata = await _YouTubeServices.GetVideoMetadata(searchQuery);
                    if (videoMetadata is not null)
                    {
                        UpdateCover pars = new();
                        pars.Id = item.Id;
                        pars.CoverUrl = videoMetadata.Thumbnail;

                        string uriUpdateCover = $"{_Uri}/UpdateMediaFile_Cover/{item.Id}";

                        if (videoMetadata.Thumbnail is not null)
                        {
                            await httpClient.PutAsJsonAsync(uriUpdateCover, pars);
                        }
                        else
                        {
                            await httpClient.PutAsJsonAsync(uriUpdateCover, new string[] { item.Id.ToString(), "Images/No-image-available.png" });
                        }
                    }
                    else
                    {
                        _logger.LogError("No videometadata found");
                    }
                }

                return;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "UpdateMediaCoverRecords - Erro no API");
                throw;
            }

        }

        public async Task<(string, string)> UpdateMediaCoverRecord(int Id)
        {
            string uriGetMediaFile = $"{_Uri}/GetMusicFile/{Id}";
            string ResultMsg;
            string clsName;

            try
            {
                var _queryResult = await httpClient
                    .GetFromJsonAsync<VideoCategoryLocations>(uriGetMediaFile);

                int iPos = _queryResult.FileUrl.IndexOf("v=") + 2;
                string videoId = _queryResult.FileUrl.Substring(iPos);

                var searchQuery = $"{videoId}";
                var videoMetadata = await _YouTubeServices.GetSingleVideoMetadata(searchQuery);
                if (videoMetadata is not null)
                {
                    UpdateCover pars = new();
                    pars.Id = _queryResult.Id;
                    pars.CoverUrl = videoMetadata.Thumbnail;

                    string uriUpdateCover = $"{_Uri}/UpdateMediaFile_Cover/{_queryResult.Id}";

                    if (videoMetadata.Thumbnail is not null)
                    {
                        await httpClient.PutAsJsonAsync(uriUpdateCover, pars);
                        clsName = "e-toast-success";
                        ResultMsg ="Capa atualizada com sucesso";
                    }
                    else
                    {
                        ResultMsg = "Sem informação sobre thumbnail ";
                        clsName = "e-toast-error";
                    }
                }
                else
                {
                    ResultMsg = "Api não devolveu informação";
                    clsName = "e-toast-error";
                }

            }
            catch (Exception ex)
            {
                ResultMsg = $"Erro no Api. {ex.Message}";
                clsName = "e-toast-error";
            }

            return (ResultMsg, clsName);
        }
    }
}
