using AutoMapper;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Api.MusicBrainz;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.LastFM;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Persistence.Services.MusicMetadata;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiKey]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class MediaController : ControllerBase
    {
        private readonly ILogger<MediaController> _logger;
        private readonly IMediaRepository _repoMedia;
        private IMapper _mapper;
        private readonly LastfmClient _lastFMclient;
        private readonly ILastFmService _lastFMService;
        private readonly IMusicMetadataService _MusicBrainzService;
        private readonly IAudioDbService _audioDbService;
        private IConfiguration _configuration;



        public MediaController(ILogger<MediaController> logger,
            IMediaRepository repoMedia,
            IMapper mapper,
            IMusicMetadataService musicBrainzService,
            LastfmClient lastFMclient,
            ILastFmService lastFMService,
            IConfiguration configuration,
            IAudioDbService audioDbService)
        {
            _logger = logger;
            _repoMedia = repoMedia;
            _mapper = mapper;
            _MusicBrainzService = musicBrainzService;
            _lastFMclient = lastFMclient;
            _lastFMService = lastFMService;
            _configuration = configuration;
            _audioDbService = audioDbService;
        }

        [HttpGet("{userId}/{role}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMediaFiles(string userId, string role)
        {
            var location = GetControllerActionNames();
            ApiResponse<string> response = new ApiResponse<string>();
            response.Success = false;

            try
            {
                var mediaFiles = await _repoMedia.GetAllMedia_Async(userId, role);
                if (mediaFiles.Any())
                {

                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetMediaFiles)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetAllMediaFromDB_Report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMediaFromDB_Report()
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetAllMediaFromDB_Report();
                if (mediaFiles.Any())
                {
                    _mapper.Map<IEnumerable<MediaVM>>(mediaFiles);
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetAllMediaFromDB_Report)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("getvideolocations/{idcategoria:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVideoLocationsAsync(int idCategoria = 0)
        {
            try
            {
                var output = await _repoMedia.GetVideoLocations(idCategoria);
                List<VideoCategoryLocations> videoList = new();
                foreach (var videoInfo in output)
                {
                    var formato = videoInfo.Formato;
                    var mediaCover = videoInfo.CoverFile is not null
                        ? videoInfo.CoverFile
                        : formato == "CD" ? "images/music-icon2.png"
                        : formato == "LP" ? "images/music-bg.jpg"
                        : formato.StartsWith("DVD") ? "/images/dvd-image.jpg"
                        : "images/mp3-music-file-icon.png";
                    videoList.Add(new VideoCategoryLocations
                    {
                        Categoria = videoInfo.Categoria,
                        IdCategoria = videoInfo.IdCategoria,
                        Id = videoInfo.Id,
                        Titulo = videoInfo.Titulo,
                        Autor = videoInfo.Autor,
                        Tempo = videoInfo.Tempo,
                        AnoEdicao = videoInfo.AnoEdicao,
                        FileUrl = videoInfo.FileUrl,
                        Formato = formato,
                        CoverFile = mediaCover
                    });
                }
                if (output.Any())
                    return Ok(videoList);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetVideoLocationsAsync) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }

        [HttpGet("getmusiclocations/{idcategoria:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMusicLocationsAsync(int idCategoria)
        {
            try
            {
                //                var output = await _repoMedia.GetMusicMedia(idCategoria);
                var output = await _repoMedia.GetMusicMedia(idCategoria);
                List<VideoCategoryLocations> musicList = new();
                foreach (var musicInfo in output)
                {
                    musicList.Add(new VideoCategoryLocations
                    {
                        Categoria = musicInfo.Categoria,
                        IdCategoria = musicInfo.IdCategoria,
                        Id = musicInfo.Id,
                        Titulo = musicInfo.Titulo,
                        Autor = musicInfo.Autor,
                        FileUrl = musicInfo.FileUrl,
                        CoverFile = musicInfo.CoverFile is not null ? musicInfo.CoverFile : "images/No-image-available.png"
                    });
                }
                if (output.Any())
                    return Ok(musicList);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (getmusiclocations) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }

        [HttpGet("getallmusicfiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllMusicFiles()
        {
            try
            {
                var output = await _repoMedia.GetAllMusicTitles();
                List<VideoCategoryLocations> musicList = new();
                foreach (var musicInfo in output)
                {
                    musicList.Add(new VideoCategoryLocations
                    {
                        Categoria = musicInfo.Categoria,
                        IdCategoria = musicInfo.IdCategoria,
                        Id = musicInfo.Id,
                        Titulo = musicInfo.Titulo,
                        Autor = musicInfo.Autor,
                        FileUrl = musicInfo.FileUrl
                        // Thumbnail will bet set later, in the gallery component
                    });
                }
                if (output.Any())
                    return Ok(musicList);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (getallmusicfiles) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }

        [HttpGet("getmusicfile/{Id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMusicFile(int Id)
        {
            try
            {
                var output = await _repoMedia.GetMusicFile(Id);
                VideoCategoryLocations musicFile = new()
                {
                    Categoria = output.Categoria,
                    IdCategoria = output.IdCategoria,
                    Id = output.Id,
                    Titulo = output.Titulo,
                    Autor = output.Autor,
                    FileUrl = output.FileUrl
                };
                if (output is not null)
                    return Ok(musicFile);
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetMusicFile) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }


        [HttpGet("getshazaminfo/{catid:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShazamInfo(int catId)
        {
            HttpClient _httpClient = new();

            try
            {
                var musicFiles = await _repoMedia.GetMusicMedia(catId);
                if (musicFiles.Any())
                {
                    List<VideoCategoryLocations> musicList = new();
                    foreach (var music in musicFiles)
                    {
                        var searchParam = $"{music.Titulo} {music.Autor}";
                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"https://shazam.p.rapidapi.com/search?term={searchParam}&locale=en-EN&offset=0&limit=5"),
                            Headers =
                                {
                                    { "x-rapidapi-host", "shazam.p.rapidapi.com" },
                                    { "x-rapidapi-key", "8229763029msh246a15560c42139p1e1a2djsn1ca0cdd178ee" },
                                },
                        };
                        using (var response = await _httpClient.SendAsync(request))
                        {
                            var status = response.EnsureSuccessStatusCode();
                            var result = await response.Content.ReadFromJsonAsync<ShazamMusicModel.Root>(); //.ReadAsStringAsync();
                            if (result is not null)
                                musicList.Add(new VideoCategoryLocations
                                {
                                    Categoria = music.Categoria,
                                    IdCategoria = music.IdCategoria,
                                    Id = music.Id,
                                    Titulo = music.Titulo,
                                    FileUrl = music.FileUrl,
                                    CoverFile = result.tracks is not null ? result.tracks.hits.FirstOrDefault().track.images.coverarthq : "Images/No-image-available.png"
                                });
                        }
                    }
                    return Ok(musicList);
                }
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (GetShazamInfo) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }


        [HttpGet("getlastfminfo/{catid:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLastFmInfo(int catId)
        {
            try
            {
                var musicFiles = await _repoMedia.GetMusicMedia(catId);
                if (musicFiles.Any())
                {
                    List<VideoCategoryLocations> musicList = new();
                    foreach (var music in musicFiles)
                    {
                        //var client = new LastfmClient("b937bd55da5593aaf00108c65dbe0bb5", "3d96efe5880bf77ce36d5641b94fad4c");
                        var response = await _lastFMclient.Auth.GetSessionTokenAsync("fauxtixluix", "Warmup00#");

                        if (response.Status == LastResponseStatus.Successful)
                        {
                            var trackInfo = await _lastFMclient.Track.GetInfoAsync(music.Titulo, music.Autor, "fauxtixluix");
                            if (trackInfo.Status == LastResponseStatus.Successful)
                            {
                                var artistInfo = await _lastFMclient.Artist.GetInfoAsync(music.Autor); // GetInfoByMbidAsync(trackInfo.Content.ArtistMbid);

                                //var track = $"{music.Autor}-{music.Titulo}";
                                //var albumInfo = await client.Album.SearchAsync(track);
                                //if (albumInfo.Content.Count > 0)
                                //{
                                //    string artistName = albumInfo.FirstOrDefault().Name;
                                //    var al = await client.Album.GetInfoAsync(music.Autor, artistName);
                                //}

                                var image = "Images/No-image-available.png";
                                if (artistInfo is not null && artistInfo.Content.MainImage.Medium is not null)
                                {
                                    image = artistInfo.Content.MainImage.Medium.AbsoluteUri;
                                }
                                musicList.Add(new VideoCategoryLocations
                                {
                                    Categoria = music.Categoria,
                                    IdCategoria = music.IdCategoria,
                                    Id = music.Id,
                                    Titulo = music.Titulo,
                                    FileUrl = music.FileUrl,
                                    CoverFile = image
                                }); ;

                            }
                        }

                    }
                    return Ok(musicList);
                }
                else
                    return NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro no Api (getlastfminfo) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao pesquisar imagens ({exc.Message})");
            }
        }

        [HttpGet("getalbuminfo/{artist}/{album}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAlbumInfo(string artist, string album)
        {
            MediaVM albumInfoObj = new();
            bool tracksInfoRetrievedOk = true;
            IEnumerable<LastTrack> tracks;

            try
            {
                if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(album))
                {
                    return BadRequest($"Please fill artist name and album to search");
                }

                LastResponse<LastAlbum> albumInfo = await _lastFMclient.Album.GetInfoAsync(artist, album, autocorrect: true, "fauxtixluix");

                if (albumInfo.Content is null)
                {
                    _logger.LogWarning($"Informação sobre {album} ({artist}) não encontradas");
                    return Ok(new());
                }

                if (albumInfo.Status == LastResponseStatus.Failure)
                {
                    return Ok(new());
                }

                TimeSpan totalDuration = new();
                var count = 0;
                try
                {
                    tracks = albumInfo.Content.Tracks;
                    var xpto = tracks.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"(LastFm - problema ao ler informações sobre as faixas de {album} ({artist}) - {ex.StackTrace}");
                    tracks = Enumerable.Empty<LastTrack>();
                    tracksInfoRetrievedOk = false;
                }

                if (tracksInfoRetrievedOk && tracks.Count() == 0)
                {
                    _logger.LogWarning($"Sem dados sobre faixas a mostrar {album} ({artist})");
                }

                var tracksInfo = new List<string>();

                try
                {
                    if (tracks.Any())
                    {
                        foreach (var trackInfo in tracks)
                        {
                            try
                            {
                                if (trackInfo is null)
                                {
                                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving album info");
                                }

                                count++;
                                if (trackInfo.Duration.HasValue)
                                {
                                    totalDuration += new TimeSpan(trackInfo.Duration.Value.Ticks);
                                }
                                else
                                {
                                    _logger.LogInformation($"Track #{count}: {album} - {trackInfo.Name} has no duration.");
                                }

                                tracksInfo.Add(trackInfo.Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error processing track #{count}: {trackInfo.Name}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tracks list.");
                }


                string formattedTotalDuration = totalDuration.ToString(@"hh\:mm\:ss");
                var releasedDate = albumInfo.Content.ReleaseDateUtc;
                var albumData = new
                {
                    albumInfo.Content.Name,
                    albumInfo.Content.ArtistMbid,
                   releasedDate,
                    Tracks = tracksInfoRetrievedOk ? albumInfo.Content.Tracks
                        .Select(t => new
                        {
                            t.Name,
                            Duration = t.Duration.HasValue ? t.Duration.Value.ToString(@"mm\:ss") : "00:00"
                        }).ToList() : new(),
                    Images = albumInfo.Content.Images.Select(i => i.AbsoluteUri).ToList()
                };

                var sourceTime = releasedDate;
                DateTime? targetTime = sourceTime?.DateTime;
                var dataMov = sourceTime.HasValue ? targetTime.Value : DateTime.UtcNow;

                StringBuilder sb = new StringBuilder();
                if (tracksInfo.Any())
                {
                    sb.Clear();
                    foreach (var track in tracksInfo)
                    {
                        sb.AppendLine(track);
                    }
                }

                var albumImages = albumData.Images;
                var albumImage = "/images/music-icon2.png";
                if (albumImages.Any())
                {
                    albumImage = albumInfo.Content.Images.Large.AbsoluteUri;
                }

                albumInfoObj = new()
                {
                    Autor = artist,
                    CoverFile = albumImage,
                    Titulo = album,
                    Tempo = formattedTotalDuration,
                    AnoEdicao = releasedDate?.Year.ToString() ?? "",
                    DataMov = dataMov,
                    Notas = sb.ToString(),
                };

                return Ok(albumInfoObj);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Error in API (getalbuminfo) {exc.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving album info ({exc.Message})");
            }
        }

        [HttpGet("GetFilesByMediaType/{opcao}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilesByMediaType(Query_TipoMedia opcao)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetFilesByMediaType(opcao);
                if (mediaFiles.Any())
                {
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFilesByMediaType)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFilesByMediaFormat/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilesByMediaFormat(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetFilesByMediaFormat(id);
                if (mediaFiles.Any())
                {
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFilesByMediaFormat)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFilesByMediaCategory/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFilesByMediaCategory(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = _repoMedia.GetFilesByMediaCategory(id);
                if (mediaFiles.Any())
                {
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFilesByMediaCategory)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFromDataBase/{filename}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFromDataBase(string fileName)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetFromDataBase(fileName);
                if (mediaFiles.Any())
                {
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFromDataBase)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMediaByID(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFile = await _repoMedia.GetMediaByID(id);

                if (mediaFile is not null)
                {
                    return Ok(mediaFile);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetMediaByID)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("ListarMedia_PorID/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListarMedia_PorID(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.ListarMedia_PorID(id);
                if (mediaFiles is not null)
                {
                    return Ok(mediaFiles);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (ListarMedia_PorID)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetMediaByTitle/{filename}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMediaByTitle(string fileName)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetMediaByTitle(fileName);
                if (mediaFiles is not null)
                {
                    return Ok(mediaFiles);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetMediaByTitle)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFormatos_Media")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormatos_Media()
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFormatFiles = await _repoMedia.GetFormatos_Media();
                if (mediaFormatFiles is not null)
                {
                    _mapper.Map<FormatoMediaVM>(mediaFormatFiles);
                    return Ok(mediaFormatFiles);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFormatos_Media)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFormatos_Media/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFormato_Media(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaDescription = await _repoMedia.GetFormatos_Media(id);
                if (!string.IsNullOrEmpty(mediaDescription))
                {
                    return Ok(mediaDescription);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFormato_Media(int))");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetMediaFormatByDescription/{descricao}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMediaFormatByDescription(string descricao)
        {
            var location = GetControllerActionNames();
            try
            {
                int mediaFormat = await _repoMedia.GetMediaFormatByDescription(descricao);
                if (mediaFormat > 0)
                {
                    return Ok(mediaFormat);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetMediaFormatByDescription)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetMediaCategoryByDescription/{descricao}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMediaCategoryByDescription(string descricao)
        {
            var location = GetControllerActionNames();
            try
            {
                int mediaCategory = await _repoMedia.GetMediaCategoryByDescription(descricao);
                if (mediaCategory > 0)
                {
                    return Ok(mediaCategory);
                }
                else
                    return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetMediaCategoryByDescription)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        [HttpGet("GetTitlesFromDataBase/{criteria}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTitlesFromDataBase(string criteria)
        {
            var location = GetControllerActionNames();
            try
            {
                var mediaFiles = await _repoMedia.GetTitlesFromDataBase(criteria);
                if (mediaFiles.Any())
                {
                    return Ok(mediaFiles);
                }
                else
                    return Ok(Enumerable.Empty<MediaVM>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetTitlesFromDataBase)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("MediaFileExists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MediaFileExists([FromQuery] string titulo, [FromQuery] string autor)
        {
            var location = GetControllerActionNames();
            try
            {
                var fileExist = await _repoMedia.MediaFileExists(titulo, autor);

                return Ok(fileExist);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (MediaFileExists)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetFirstId_Media")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFirstId_Media()
        {
            var location = GetControllerActionNames();
            try
            {
                var idMedia = await _repoMedia.GetFirstId_Media();

                return Ok(idMedia);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetFirstId_Media)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetLastId_Media")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLastId_Media()
        {
            var location = GetControllerActionNames();
            try
            {
                var idMedia = await _repoMedia.GetLastId_Media();

                return Ok(idMedia);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetLastId_Media)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpGet("GetTotalEspacoOcupado_Media/{s}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalEspacoOcupado_Media(string s)
        {
            var location = GetControllerActionNames();
            try
            {
                var output = await _repoMedia.GetTotalEspacoOcupado_Media(s);

                return Ok(output);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (GetTotalEspacoOcupado_Media)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(MediaVM), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMediaFile([FromBody] MediaVM mediaDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (mediaDTO is null)
                {
                    return BadRequest();
                }

                var mediaFile = _mapper.Map<Media>(mediaDTO);

                bool fileExists = await _repoMedia.MediaFileExists(mediaFile.FileName, mediaFile.Autor);
                if (fileExists)
                {
                    ModelState.AddModelError("Media", "Nome e Autor do ficheiro já existe na base de dados");
                    return BadRequest(ModelState);
                }

                await _repoMedia.Insert_Async(mediaFile);

                int createdMediayId = _repoMedia.GetLastId();
                var createdMedia = await GetMediaByID(createdMediayId);
                return CreatedAtAction(nameof(GetMediaByID), new { id = createdMediayId }, createdMedia);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (CreateMediaFile)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        [HttpPut("UpdateMediaFile/{id:int}")]
        [ProducesResponseType(typeof(MediaVM), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMediaFile(int id, [FromBody] MediaVM mediaDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                if (mediaDTO == null)
                {
                    _logger.LogWarning($"O ficheiro Média passado como paràmetro, é incorreto.");
                    return BadRequest($"O ficheiro Média é incorreto.");
                }
                if (id != mediaDTO.Id)
                {
                    _logger.LogWarning($"O ID do ficheiro Média ({id}) passado como paràmetro, está incorreto");
                    return BadRequest($"ID ({id}) passado como paràmetro, está incorreto");
                }

                var mediaEntityToUpdate = await _repoMedia.Query_ById_Async(id);
                if (mediaEntityToUpdate == null)
                {
                    _logger.LogWarning($"O ID ({id}) não foi encontrado");
                    return NotFound($"ID {id} não encontrado");
                }

                var recordMapped = _mapper.Map(mediaDTO, mediaEntityToUpdate);

                var updateOk = await _repoMedia.UpdateMediaFile(recordMapped);
                return Ok(updateOk);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (UpdateMediaFile)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpPut("UpdateMediaFile_RegPath/{id:int}")]
        [ProducesResponseType(typeof(MediaVM), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMediaFile_RegPath(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                await _repoMedia.UpdateMediaFile_RegPath(id);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (UpdateMediaFile_RegPath)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        [HttpPut("UpdateMediaFile_Cover/{Id:int}")]
        [ProducesResponseType(typeof(MediaVM), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMediaFile_Cover(int Id, [FromBody] UpdateCover args)
        {
            var location = GetControllerActionNames();
            try
            {
                string CoverUrl = args.CoverUrl;
                await _repoMedia.UpdateMediaFile_Cover(Id, CoverUrl);
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro no Api (UpdateMediaFile_Cover)");
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult> DeleteMedia(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                if (id < 1)
                {
                    _logger.LogWarning($"{location}: Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var mediaToDelete = await _repoMedia.Query_ById_Async(id);
                if (mediaToDelete is null)
                {
                    return NotFound($"Media com o Id {id} não encontrada");
                }

                await _repoMedia.DeleteById_Async(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        [Obsolete("Legacy endpoint - MusicBrainz integration (will be removed)")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("artist-releases")]
        public async Task<ActionResult<ArtistReleasesResponseDto>> GetArtistReleases(
       [FromBody] ArtistReleasesRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ArtistName))
                return BadRequest("ArtistName is required.");

            var result = await _MusicBrainzService.GetArtistReleasesAsync(request);
            return Ok(result);
        }

        [HttpGet("artist-albums/{artist}")]
        public async Task<IActionResult> GetArtistAlbums(string artist)
        {
            if (string.IsNullOrWhiteSpace(artist))
                return BadRequest("Artist is required.");

            try
            {
                var albums = await _lastFMService.GetArtistReleasesAsync(artist);

                return Ok(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Last.fm artist albums");
                return StatusCode(500, "Error retrieving artist albums");
            }
        }

        [HttpGet("audiodb-data/{artist}/{album}")]
        public async Task<IActionResult> GetAudioDbData(string artist, string album )
        {
            if (string.IsNullOrWhiteSpace(artist))
                return BadRequest("Artist is required.");

            try
            {
                var albums = await _audioDbService.GetAlbumDetailsAsync(artist, album);

                return Ok(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AudioDB artist albums");
                return StatusCode(500, "Error retrieving artist albums");
            }
        }


        [HttpGet("mp3-album/{artist}/{track}")]
        public async Task<IActionResult> GetAlbum(string artist, string track)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(track))
                return BadRequest("Artist and track are required.");
            try
            {
                var apiKey = _configuration["LastFmApiKey"];
                var albumName = await _lastFMService.GetAlbumAsync(artist, track, apiKey);

                return Ok(albumName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Last.fm song album");
                return StatusCode(500, "Error retrieving song album");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
    }
}
