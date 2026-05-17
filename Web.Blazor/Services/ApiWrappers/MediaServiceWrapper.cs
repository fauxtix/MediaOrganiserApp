using FluentValidation.Results;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Validators;
using MediaOrganizerApp.Domain.Entities;
using Newtonsoft.Json;
using System.Globalization;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.ApiWrappers
{
    public class MediaServiceWrapper : IMediaServiceWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;
        private readonly ILogger<MediaServiceWrapper> _logger;
        private readonly string _Uri;

        public MediaServiceWrapper(HttpClient httpClient,
            ILogger<MediaServiceWrapper> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/media";
        }

        public async Task<IEnumerable<MediaVM>> GetAllMedia_Async(string userId, string role)
        {
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>($"{_Uri}/{userId}/{role}");
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                return Enumerable.Empty<MediaVM>();
            }
        }

        public async Task<IEnumerable<MediaVM>> GetAllMediaFromDB_Report()
        {
            string uri = $"{_Uri}/GetAllMediaFromDB_Report";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaType(Query_TipoMedia Tipo)
        {
            string uri = $"{_Uri}/GetFilesByMediaType/{Tipo}";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaFormat(int id)
        {
            string uri = $"{_Uri}/GetFilesByMediaFormat/{id}";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetFilesByMediaCategory(int id)
        {
            string uri = $"{_Uri}/GetFilesByMediaCategory/{id}";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<string> GetFromDataBase(string fileName)
        {
            string uri = $"{_Uri}/GetFromDataBase/{fileName}";
            try
            {
                var _mediaFile = await _httpClient
                    .GetFromJsonAsync<string>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<MediaVM> GetMediaByID(int id)
        {
            string uri = $"{_Uri}/{id}";
            try
            {
                var _mediaFile = await _httpClient
                    .GetFromJsonAsync<MediaVM>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> GetTitlesFromDataBase(string sCriteria)
        {
            string uri = $"{_Uri}/GetTitlesFromDataBase/{sCriteria}";
            try
            {
                var _mediaFile = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<bool> MediaFileExists(string mediaTitle, string mediaAuthor)
        {
            string uri = $"{_Uri}/MediaFileExists/{mediaTitle}/{mediaAuthor}";
            try
            {
                var _mediaFileExist = await _httpClient
                    .GetFromJsonAsync<bool>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<bool> CreateMediaFile(MediaVM mediaFileDTO)
        {
            try
            {
                using (HttpResponseMessage result = await _httpClient.PostAsJsonAsync(_Uri, mediaFileDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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
            try
            {
                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, mediaFileDTO))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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

            try
            {
                using (HttpResponseMessage result = await _httpClient.PutAsJsonAsync(uri, id))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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

            try
            {
                using (HttpResponseMessage result = await _httpClient.DeleteAsync(uri))
                {
                    var success = result.IsSuccessStatusCode;
                    return success;
                }
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
                var _mediaFileExist = await _httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetFirstId_Media - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<int> GetLastId_Media()
        {
            string uri = $"{_Uri}/GetLastId_Media";
            try
            {
                var _mediaFileExist = await _httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetLastId_Media - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<decimal> GetTotalEspacoOcupado_Media(string s)
        {
            string uri = $"{_Uri}/GetTotalEspacoOcupado_Media/{s}";
            try
            {
                var _mediaFileExist = await _httpClient
                    .GetFromJsonAsync<decimal>(uri);
                return _mediaFileExist;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "GetTotalEspacoOcupado_Media - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<MediaVM>> ListarMedia_PorID(int id)
        {
            string uri = $"{_Uri}/ListarMedia_PorID/{id}";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<MediaVM>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<MediaVM> GetMediaByTitle(string sFileName)
        {
            string uri = $"{_Uri}/GetMediaByTitle/{sFileName}";
            try
            {
                var _mediaFile = await _httpClient
                    .GetFromJsonAsync<MediaVM>(uri);
                return _mediaFile;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IEnumerable<Formato_Media>> GetFormatos_Media()
        {
            string uri = $"{_Uri}/GetFormatos_Media";
            try
            {
                var _mediaFiles = await _httpClient
                    .GetFromJsonAsync<IEnumerable<Formato_Media>>(uri);
                return _mediaFiles;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<string> GetFormatos_Media(int id)
        {
            string uri = $"{_Uri}/GetFormato_Media/{id}";
            try
            {
                var _mediaFormat = await _httpClient
                    .GetFromJsonAsync<string>(uri);
                return _mediaFormat;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<int> GetMediaFormatByDescription(string formatDescription)
        {
            string uri = $"{_Uri}/GetMediaFormatByDescription/{formatDescription}";
            try
            {
                var _mediaFormatId = await _httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaFormatId;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<int> GetMediaCategoryByDescription(string descricao)
        {
            string uri = $"{_Uri}/GetMediaCategoryByDescription/{descricao}";
            try
            {
                var _mediaCategoryd = await _httpClient
                    .GetFromJsonAsync<int>(uri);
                return _mediaCategoryd;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }
        public async Task<IEnumerable<VideoCategoryLocations>> GetVideoLocations(int idCategoria)
        {
            string uri = $"{_Uri}/GetVideoLocations/{idCategoria}";
            try
            {
                var _queryResult = await _httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uri);

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }



        public async Task<IEnumerable<VideoCategoryLocations>> GetMusicMedia(int catId)
        {
            string uri = $"{_Uri}/getlastfminfo/{catId}";
            //string uri = $"{_Uri}/getshazaminfo/{catId}";
            try
            {
                var _queryResult = await _httpClient
                    .GetFromJsonAsync<IEnumerable<VideoCategoryLocations>>(uri);

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<string> GetMp3Album(string artist, string track)
        {
            string uri = $"{_Uri}/mp3-album/{artist}/{track}";
            try
            {
                string result = await _httpClient.GetStringAsync(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Media Files - Erro emGetMp3Album");
                throw;
            }
        }


        public async Task<MediaVM> GetAlbumInfo(string artist, string album)
        {
            string uri = $"{_Uri}/getalbuminfo/{artist}/{album}";
            try
            {
                var _queryResult = await _httpClient
                    .GetFromJsonAsync<MediaVM>(uri);

                return _queryResult;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Media Files - Erro ao pesquisar API Media ({exc.Message})");
                return new();
            }
        }


        public async Task<IEnumerable<VideoCategoryLocations>> GetYoutubeMusicMedia(int catId)
        {
            string uri = $"{_Uri}/GetMusicLocations/{catId}";
            try
            {
                var _queryResult = await _httpClient
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
                _logger.LogError(exc, "Media Files - Erro ao pesquisar API Media");
                throw;
            }
        }

        public async Task<IMDbModel> GetOMDbInfo(string title, string actor)
        {
            // Primeiro, procurar o filme pelo título para obter o IMDb ID
            var searchQuery = $"http://www.omdbapi.com/?apikey=8ffca6ea&s={title}";
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, searchQuery);
            var searchResponse = await _httpClient.SendAsync(searchRequest);
            searchResponse.EnsureSuccessStatusCode();
            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<IMDbSearchResult>(searchJson);

            if (searchResult.Response.ToLower() == "false")
            {
                return null;
            }

            foreach (var movie in searchResult.Search)
            {
                // Buscar os detalhes completos usando o IMDb ID de cada filme
                var detailQuery = $"http://www.omdbapi.com/?apikey=8ffca6ea&i={movie.imdbID}";
                var detailRequest = new HttpRequestMessage(HttpMethod.Get, detailQuery);
                var detailResponse = await _httpClient.SendAsync(detailRequest);
                detailResponse.EnsureSuccessStatusCode();
                var detailJson = await detailResponse.Content.ReadAsStringAsync();
                var detailResult = JsonConvert.DeserializeObject<IMDbModel>(detailJson);

                if (detailResult.Actors != null && detailResult.Actors.Contains(actor, StringComparison.OrdinalIgnoreCase))
                {
                    return detailResult;
                }
            }

            return null;
        }

        public string ConvertIOMDbTimeStringAsTime(string input)
        {
            int totalMinutes = int.Parse(input.Replace(" min", ""));
            TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);
            string formattedTime = timeSpan.ToString(@"hh\:mm\:ss");
            return formattedTime;
        }

        public DateTime ConvertIOMDbStringAsDate(string input)
        {

            string format = "dd MMM yyyy";

            DateTime dateTime = DateTime.ParseExact(input, format, CultureInfo.InvariantCulture);

            return dateTime;
        }

        public int ConvertIMDbRatingToFiveScale(string imdbRating)
        {
            if (double.TryParse(imdbRating, NumberStyles.Any, CultureInfo.InvariantCulture, out double rating))
            {
                return (int)Math.Round(rating / 2);
            }

            return 0;
        }


        // AudioDB API Models



        private class IMDbSearchResult
        {
            public string Response { get; set; }
            public Search[] Search { get; set; }
        }
        public List<string> RegistoComErros(MediaVM mediaFile)
        {
            MediaValidator validator = new();
            ValidationResult results = validator.Validate(mediaFile);

            if (!results.IsValid)
            {
                List<string> errors = new();
                foreach (var failure in results.Errors)
                {
                    errors.Add(failure.ErrorMessage);
                }
                return errors;
            }

            return new();
        }

    }
}

