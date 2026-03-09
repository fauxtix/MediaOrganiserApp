using AutoMapper;
using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Shared.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Web.Blazor.Services.Common;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.ApiRelated
{
    public class MediaService : IMediaService
    {
        private readonly HttpClient httpClient;
        protected IMapper _mapper;
        private readonly ILogger<MediaService> _logger;
        private string _Uri = Endpoints.MediaEndpoint;
        private readonly IGetYoutubeVideoMetadata _YouTubeService;

        public MediaService(HttpClient httpClient, IMapper mapper, ILogger<MediaService> logger, IGetYoutubeVideoMetadata YouTubeService)
        {
            this.httpClient = httpClient;
            _mapper = mapper;
            _logger = logger;
            _YouTubeService = YouTubeService;
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia_Async()
        {
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(_Uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report()
        {
            string uri = $"{_Uri}/GetAllMediaFromDB_Report";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo)
        {
            string uri = $"{_Uri}/GetFilesByMediaType/{Tipo}";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int id)
        {
            string uri = $"{_Uri}/GetFilesByMediaFormat/{id}";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaCategory(int id)
        {
            string uri = $"{_Uri}/GetFilesByMediaCategory/{id}";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<string> GetFromDataBase(string fileName)
        {
            string uri = $"{_Uri}/GetFromDataBase/{fileName}";
            try
            {
                var _mediaFile = await httpClient
                    .GetFromJsonAsync<string>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<MediaVM> GetMediaByID(int id)
        {
            string uri = $"{_Uri}/{id}";
            try
            {
                var _mediaFile = await httpClient
                    .GetFromJsonAsync<MediaVM>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sCriteria)
        {
            string uri = $"{_Uri}/GetTitlesFromDataBase/{sCriteria}";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFile = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> MediaFileExists(string sTitulo)
        {
            string uri = $"{_Uri}/MediaFileExists/{sTitulo}";
            _logger.LogInformation("Lendo todos os ficheiros Media");
            try
            {
                var _mediaFileExist = await httpClient
                    .GetFromJsonAsync<bool>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<bool> CreateMediaFile(MediaVM mediaFileDTO)
        {
            _logger.LogInformation("Criando Media file");
            try
            {
                HttpResponseMessage result = await httpClient.PostAsJsonAsync(_Uri, mediaFileDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao criar Media file {exc.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateMediaFile(MediaVM mediaFileDTO)
        {
            string uri = $"{_Uri}/UpdateMediaFile/{mediaFileDTO.Id}";
            _logger.LogInformation("Atualização de ficheiro Média");
            try
            {
                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, mediaFileDTO);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Media file {exc.Message}");
                throw;
            }
        }


        public async Task<bool> UpdateMediaFile_RegPath(int id)
        {
            string uri = $"{_Uri}/UpdateMediaFile_RegPath/{id}";

            _logger.LogInformation("Atualizando Media file");
            try
            {
                HttpResponseMessage result = await httpClient.PutAsJsonAsync(uri, id);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao atualizar Media file {exc.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteMediaFile(int id)
        {
            var uri = $"{_Uri}/{id}";

            _logger.LogInformation("Apagando Media file");
            try
            {
                HttpResponseMessage result = await httpClient.DeleteAsync(uri);
                var success = result.IsSuccessStatusCode;
                return success;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao apagar Media file {exc.Message}");
                throw;
            }
        }

        public async Task<int> GetFirstId_Media()
        {
            string uri = $"{_Uri}/GetFirstId_Media";
            try
            {
                var _mediaFileExist = await httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetFirstId_Media - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<int> GetLastId_Media()
        {
            string uri = $"{_Uri}/GetLastId_Media";
            try
            {
                var _mediaFileExist = await httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetLastId_Media - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<decimal> GetTotalEspacoOcupado_Media(string s)
        {
            string uri = $"{_Uri}/GetTotalEspacoOcupado_Media/{s}";
            try
            {
                var _mediaFileExist = await httpClient
                    .GetFromJsonAsync<decimal>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetTotalEspacoOcupado_Media - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> ListarMedia_PorID(int id)
        {
            string uri = $"{_Uri}/ListarMedia_PorID/{id}";
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<MediaVM> GetMediaByTitle(string sFileName)
        {
            string uri = $"{_Uri}/GetMediaByTitle/{sFileName}";
            try
            {
                var _mediaFile = await httpClient
                    .GetFromJsonAsync<MediaVM>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<Formato_Media>> GetFormatos_Media()
        {
            string uri = $"{_Uri}/GetFormatos_Media";
            try
            {
                var _mediaFiles = await httpClient
                    .GetFromJsonAsync<IEnumerable<Formato_Media>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<string> GetFormatos_Media(int id)
        {
            string uri = $"{_Uri}/GetFormato_Media/{id}";
            try
            {
                var _mediaFormat = await httpClient
                    .GetFromJsonAsync<string>(uri);
                return _mediaFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<int> GetMediaFormatByDescription(string formatDescription)
        {
            string uri = $"{_Uri}/GetMediaFormatByDescription/{formatDescription}";
            try
            {
                var _mediaFormatId = await httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFormatId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<int> GetMediaCategoryByDescription(string descricao)
        {
            string uri = $"{_Uri}/GetMediaCategoryByDescription/{descricao}";
            try
            {
                var _mediaCategoryd = await httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaCategoryd;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }
        public async Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria)
        {
            string uri = $"{_Uri}/GetVideoLocations/{idCategoria}";
            try
            {
                var _queryResult = await httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uri);


                //foreach (var item in _queryResult)
                //{
                //    int iPos = item.FileUrl.IndexOf("v=") + 2;
                //    string videoId = item.FileUrl.Substring(iPos);
                //    var videoMetadata = await youtubeMetadata.GetVideoMetadata(videoId);

                //    if (videoMetadata is not null)
                //    {
                //        if (videoMetadata.Thumbnail is not null)
                //        {
                //            item.Thumbnail = videoMetadata.Thumbnail;
                //        }
                //        else
                //        {
                //            item.Thumbnail = "Images/No-image-available.png";
                //        }
                //    }
                //    else
                //    {
                //        item.Thumbnail = "Images/No-image-available.png";
                //    }
                //}

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }



        public async Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int catId)
        {
            string uri = $"{_Uri}/getlastfminfo/{catId}";
            //string uri = $"{_Uri}/getshazaminfo/{catId}";
            try
            {
                var _queryResult = await httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uri);

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<IEnumerable<VideoCategoryLocations>> GetYoutubeMusicMedia(int catId)
        {
            string uri = $"{_Uri}/GetMusicLocations/{catId}";
            try
            {
                var _queryResult = await httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uri);

                //foreach (var item in _queryResult)
                //{
                //    var searchQuery = $"{item.Autor} - {item.Titulo}";
                //    var videoMetadata = await youtubeMetadata.GetAlbumArtistMetadata(searchQuery);

                //    if (videoMetadata is not null)
                //    {
                //        if (videoMetadata.Thumbnail is not null)
                //        {
                //            item.Thumbnail = videoMetadata.Thumbnail;
                //        }
                //        else
                //        {
                //            item.Thumbnail = "Images/No-image-available.png";
                //        }
                //    }
                //    else
                //    {
                //        item.Thumbnail = "Images/No-image-available.png";
                //    }
                //}

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API");
                throw;
            }
        }


        public string RegistoComErros(MediaVM mediaFile)
        {
            MediaValidator validator = new MediaValidator();
            ValidationResult results = validator.Validate(mediaFile);

            if (!results.IsValid)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var failure in results.Errors)
                {
                    sb.AppendLine(failure.ErrorMessage);
                }
                return sb.ToString();
            }

            return "";
        }

    }
}

