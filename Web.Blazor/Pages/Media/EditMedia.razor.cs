using ATL;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Formatting;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Globalization;
using System.Text;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.Common;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Pages.Media
{
    public partial class EditMedia
    {
        [Parameter] public MediaVM SelectedRecord { get; set; }
        [Parameter] public bool blnEdit { get; set; }
        [Parameter] public bool ShowStoredPathEntry { get; set; }

        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IGetYoutubeVideoMetadata YoutubeService { get; set; }
        [Inject] public IMediaServiceWrapper MediaService { get; set; }
        [Inject] public IAudioDbServiceWrapper AudioDbService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }

        [Inject] public AiMediaClientService AiMediaClient { get; set; }

        [Inject] public IFormatos_MediaService formatosService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<EditMedia> Logger { get; set; }

        [Inject] Microsoft.Extensions.Localization.IStringLocalizer<App> L { get; set; }

        [Inject] IWebHostEnvironment Env { get; set; }

        [Inject] TranslateClientService TranslateClient { get; set; }

        protected ApplicationSettingsDto AppSettings;

        private async Task<string> GetApiBaseUrlAsync()
        {
            var settings = await AppSettingsService.GetSettingsAsync();

            var url = Env.IsDevelopment()
                ? settings.ApiBaseUrl_Development
                : settings.ApiBaseUrl_Production;

            if (string.IsNullOrWhiteSpace(url))
                throw new System.InvalidOperationException(
                    "ApplicationSettings does not define an API base URL for the current environment.");

            return url.TrimEnd('/');
        }

        protected SfUploader sfUploaderCover;
        private SfSpinner spinner;

        public string uploadedCoverFile { get; set; } = "";

        private bool SearchArtistRecordsVisibility = false;

        protected SfRadioButton<byte> chkVideo;
        protected SfRadioButton<byte> chkAudio;

        private bool isMp3;
        private bool isMp4;

        private SfToast ToastObj;

        public string CssClass { get; set; }

        private string ToastTitle = "";
        private string ToastContent = "";
        private string ToastCssClass = "";
        private string ToastIcon = "e-icon-ThumbsUp";
        private int ToastTimeOut = 2000;

        protected string mediaFile { get; set; }
        protected string mediaFilename { get; set; }

        public DialogEffect Effects = DialogEffect.Zoom;

        private bool YouTubeDialogVisibility = false;

        SfCheckBox<bool> chkVisualizado;

        protected string ext { get; set; }

        private IEnumerable<FormatoMediaVM> formatList;
        private IEnumerable<CategoryVM> categoryList;

        protected YouTubeVideoDetails VideoDetail = new();

        private bool showAlertMessage = false;
        private string alertMessage { get; set; } = "";
        private string alertMessageColor { get; set; } = "text-white";

        private string busyStatusMessage { get; set; } = "";
        private bool isBusy;
        private CancellationTokenSource mediaInfoCts;

        [Inject] public MediaInfoProgressClient ProgressClient { get; set; }

        protected bool fileVerified;

        private bool TranslateVisible { get; set; } = false;

        MessageSeverity messageSeverity = MessageSeverity.Info;

        protected override async Task OnInitializedAsync()
        {
            formatList = await formatosService.Listar();
            categoryList = await categoriasService.AllAsync(CategoryType.Media); // true = Publicacoes, false = Media
            AppSettings = await AppSettingsService.GetSettingsAsync();
        }

        protected override void OnParametersSet()
        {
            fileVerified = LocalFileOnDisk();
            if (!fileVerified)
            {
                ShowMessage(L["TituloFicheiroNaoExisteLocalEspecificado"] + ".", MessageSeverity.Warning);
                var notFound = L["TituloFicheiroNaoExisteLocalEspecificado"] + ".";
                var contactAdmin = L["ContactAdmin"];
                var fileTitle = L["TituloFicheiro"];

                alertMessage = fileTitle + " " + notFound + " " + contactAdmin;
                return;
            }

            if (SelectedRecord.FileName != null && !string.IsNullOrEmpty(SelectedRecord.FileName) && !string.IsNullOrEmpty(SelectedRecord.FilePath))
            {
                mediaFile = SelectedRecord.FilePath;

                System.IO.FileInfo fi = new System.IO.FileInfo(mediaFile);
                mediaFilename = fi.Name;

                ext = fi.Extension.ToLower();
                isMp3 = (ext == ".mp3");
                isMp4 = (ext == ".mp4");

                if (!string.IsNullOrEmpty(SelectedRecord.Tempo) && isMp3)
                {
                    SelectedRecord.Tempo = GetMp3Duration(SelectedRecord.FilePath);
                }

                var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                string formatted = string.Empty;
                nfi.NumberGroupSeparator = " ";

                if (string.IsNullOrEmpty(SelectedRecord.Tamanho))
                {
                    if (fi.Exists)
                    {
                        formatted = (fi.Length / 1024).ToString("#,0.00", nfi); // "1 234 897.11"
                        SelectedRecord.Tamanho = formatted + " MB";
                    }
                    else
                    {
                        SelectedRecord.Tamanho = "N/A";
                    }
                }
                else
                {
                    var idx = SelectedRecord.Tamanho.IndexOf("N/A");
                    if (idx == -1)
                    {
                        var textLocation = SelectedRecord.Tamanho.IndexOf("MB");
                        if (textLocation > 0) return;

                        if (textLocation > 0)
                        {
                            string numericPart = SelectedRecord.Tamanho.Replace("MB", "").Trim();

                            // Remover espa�os dentro do n�mero
                            numericPart = numericPart.Replace(" ", "");

                            // Converter para double
                            double megabytes = double.Parse(numericPart, CultureInfo.InvariantCulture);

                            // Converter para long
                            long result = (long)megabytes;

                            long fileSize = result;
                            formatted = (fileSize / 1024).ToString("#,0.00", nfi); // "1 234 897.11"
                            SelectedRecord.Tamanho = formatted + " MB";
                        }
                        else
                        {
                            formatted = (fi.Length / 1024).ToString("#,0.00", nfi); // "1 234 897.11"
                            SelectedRecord.Tamanho = formatted + " MB";
                        }

                    }
                }

            }
            else if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                var mediaFormat = SelectedRecord.FormatoMedia;
                if (mediaFormat is null) return;
                if (mediaFormat.ToLower() == "cd" || mediaFormat.ToLower() == "lp")
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;
                }
                else
                {
                    ext = ".mp4";
                    isMp4 = true;
                }

                mediaFilename = SelectedRecord.FileUrl;
            }
            else
            {
                var mediaFormat = SelectedRecord.FormatoMedia;
                if (mediaFormat is null)
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;

                }
                else if (mediaFormat.ToLower() == "cd" || mediaFormat.ToLower() == "lp")
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;
                }
                else
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;
                }
            }

            StateHasChanged();
        }

        protected async Task GetMediaInfo()
        {
            if (string.IsNullOrWhiteSpace(SelectedRecord.FileName) || string.IsNullOrWhiteSpace(SelectedRecord.Autor))
            {
                ShowMessage(L["FillAuthorAndTitleFields"], MessageSeverity.Warning);
                return;
            }

            if (isMp3 && blnEdit &&
                string.IsNullOrWhiteSpace(SelectedRecord.Notas))
            {
                var albumName = await GetMp3Album(SelectedRecord.Autor, SelectedRecord.FileName);
                if (!string.IsNullOrWhiteSpace(albumName))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(L["AlbumNameLabel", albumName]);
                    var albumInfo = await MediaService.GetAlbumInfo(SelectedRecord.Autor, albumName);
                    if (!string.IsNullOrEmpty(albumInfo.Notas))
                    {
                        sb.AppendLine();
                        sb.AppendLine(L["TracksLabel"]);
                        sb.AppendLine();
                        sb.AppendLine(albumInfo.Notas);
                        SelectedRecord.Notas = sb.ToString();
                    }
                    else
                        SelectedRecord.Notas = albumName;

                    if (!string.IsNullOrEmpty(albumInfo.AnoEdicao))
                        SelectedRecord.AnoEdicao = albumInfo.AnoEdicao != DateTime.Now.Year.ToString() ? albumInfo.AnoEdicao : string.Empty;

                    if (!string.IsNullOrEmpty(albumInfo.CoverFile))
                        SelectedRecord.CoverFile = albumInfo.CoverFile;

                    await InvokeAsync(StateHasChanged);
                    return;
                }
            }

            var shouldAIBeUsed = SelectedRecord.IdFormato_Media > 1 && SelectedRecord.IdFormato_Media < 6;

            var apiUrlAddress = await GetApiBaseUrlAsync();
            await ProgressClient.StartAsync(apiUrlAddress);


            isBusy = true;
            busyStatusMessage = "";
            mediaInfoCts?.Cancel();
            mediaInfoCts?.Dispose();
            mediaInfoCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));

            if (spinner is not null)
                await spinner.ShowAsync();

            showAlertMessage = false;
            alertMessage = "";
            string? aiProviderMessage = null;

            try
            {
                var mediaTitle = SelectedRecord.FileName;
                var artist = SelectedRecord.Autor;
                var uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                // Try external APIs first
                bool notasFilled = !string.IsNullOrWhiteSpace(SelectedRecord.Notas);
                bool anoEdicaoFilled = !string.IsNullOrWhiteSpace(SelectedRecord.AnoEdicao);

                // External API for Notas (example: OMDb, AlbumInfo, etc.)
                if (!notasFilled)
                {
                    var dvdInfo = await MediaService.GetOMDbInfo(mediaTitle, artist);
                    if (dvdInfo != null && !string.IsNullOrEmpty(dvdInfo.Awards))
                    {
                        SelectedRecord.Notas = dvdInfo.Awards;
                        notasFilled = true;
                    }
                    else
                    {
                        var albumInfo = await MediaService.GetAlbumInfo(artist, mediaTitle);
                        if (albumInfo != null && !string.IsNullOrEmpty(albumInfo.Notas))
                        {
                            SelectedRecord.Notas = albumInfo.Notas;
                            notasFilled = true;
                        }
                    }
                }

                // External API for AnoEdicao (example: OMDb, AlbumInfo, etc.)
                if (!anoEdicaoFilled)
                {
                    var dvdInfo = await MediaService.GetOMDbInfo(mediaTitle, artist);
                    if (dvdInfo != null && !string.IsNullOrEmpty(dvdInfo.Year))
                    {
                        SelectedRecord.AnoEdicao = dvdInfo.Year != DateTime.Now.Year.ToString() ? dvdInfo.Year : string.Empty;
                        anoEdicaoFilled = true;
                    }
                    else
                    {
                        var albumInfo = await MediaService.GetAlbumInfo(artist, mediaTitle);
                        if (albumInfo != null && !string.IsNullOrEmpty(albumInfo.AnoEdicao))
                        {
                            SelectedRecord.AnoEdicao = albumInfo.AnoEdicao != DateTime.Now.Year.ToString() ? albumInfo.AnoEdicao : string.Empty;
                            anoEdicaoFilled = true;
                        }
                    }
                }

                ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
                ProgressClient.ProgressEventReceived += OnProgressEventReceived;

                // AI fallback (apenas para formatos suportados CD/LP/DVD/Audio-DVD)
                if (shouldAIBeUsed)
                {
                    // AI fallback for Notas (OpenRouter primeiro, depois Ollama)
                    if (!notasFilled || (notasFilled && SelectedRecord.Notas == "N/A"))
                    {
                        var ai = await AiMediaClient.GetMediaNotesCloudAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId, null, "media-api", mediaInfoCts.Token);
                        if (ai is null)
                        {
                            ai = await AiMediaClient.GetMediaNotesAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId, null, "media-api", mediaInfoCts.Token);
                            if (ai is not null)
                                aiProviderMessage = L["MediaInfoProviderOllama"];
                        }
                        else
                        {
                            aiProviderMessage = L["MediaInfoProviderOpenRouter"];
                        }

                        if (ai == null)
                        {
                            ShowMessage(L["AINotesServiceFailed"] ?? L["AINotesServiceFailedFallback"], MessageSeverity.Warning);
                        }
                        else
                        {
                            var notesValue = ai.Notes;
                            if (!string.IsNullOrWhiteSpace(notesValue))
                            {
                                SelectedRecord.Notas = notesValue;
                                if (!string.IsNullOrWhiteSpace(ai.Warnings))
                                {
                                    ShowMessage(ai.Warnings, MessageSeverity.Warning);
                                    alertMessage = ai.Warnings;
                                    aiProviderMessage = null;
                                }
                            }
                            else
                            {
                                aiProviderMessage = null;
                            }
                        }
                    }

                    // AI fallback for AnoEdicao (OpenRouter primeiro, depois Ollama)
                    if (!anoEdicaoFilled || SelectedRecord.AnoEdicao == DateTime.Now.Year.ToString())
                    {
                        var year = await AiMediaClient.GetMediaEditionYearCloudAsync(
                            artist,
                            mediaTitle,
                            uiLanguage,
                            ProgressClient.ConnectionId,
                            null,
                            "media-api",
                            mediaInfoCts.Token);
                        var yearProviderMessage = L["MediaInfoProviderOpenRouter"];
                        if (year is null)
                        {
                            year = await AiMediaClient.GetMediaEditionYearAsync(
                                artist,
                                mediaTitle,
                                uiLanguage,
                                ProgressClient.ConnectionId,
                                null,
                                "media-api",
                                mediaInfoCts.Token);
                            if (year is not null)
                                yearProviderMessage = L["MediaInfoProviderOllama"];
                        }

                        var invalidYearReturned = false;
                        if (!string.IsNullOrWhiteSpace(year) && year == "N/A")
                        {
                            ShowMessage(L["NoEditionYearReturned"], MessageSeverity.Warning);
                            invalidYearReturned = true;
                        }
                        else if (!string.IsNullOrWhiteSpace(year) && !DataFormat.IsNumeric(year))
                        {
                            ShowMessage(L["InvalidEditionYearReturned", year], MessageSeverity.Warning);
                            invalidYearReturned = true;
                        }

                        if (!string.IsNullOrWhiteSpace(year) && !invalidYearReturned)
                        {
                            SelectedRecord.AnoEdicao = year;
                            aiProviderMessage ??= yearProviderMessage;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                ShowMessage(L["OperacaoCancelada"], MessageSeverity.Warning);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching media info for {MediaTitle} by {Artist}", SelectedRecord.FileName, SelectedRecord.Autor);
            }
            finally
            {
                if (spinner is not null)
                    await spinner.HideAsync();
                isBusy = false;
                mediaInfoCts?.Dispose();
                mediaInfoCts = null;
                ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
                await ProgressClient.StopAsync();

                // Show UI explanation if no notes were retrieved
                if (string.IsNullOrWhiteSpace(SelectedRecord.Notas))
                {
                    ShowMessage(L["NoNotesFound"] ?? L["NoNotesFoundFallback"], MessageSeverity.Warning);
                }
                else if (aiProviderMessage is not null)
                {
                    ShowMessage(aiProviderMessage, MessageSeverity.Info);
                }

                StateHasChanged();
            }
        }

        private void OnProgressEventReceived(ProgressUpdateDto update)
        {
            if (update is null)
                return;

            busyStatusMessage = update.Stage switch
            {
                "preparing_prompt" or "request_sent" => L["AIProcessingPreparing"],
                "waiting_response" => L["AIProcessingWaiting"],
                "parsing" => L["AIProcessingParsing"],
                "completed" => L["AIProcessingDone"],
                "failed" => L["AIProcessingFailed"],
                "cancelled" => L["AIProcessingCancelled"],
                "cloud_try" => string.Format(L["AIProcessingCloudTrying"].Value, update.Message),
                "cloud_failed" => string.Format(L["AIProcessingCloudFailed"].Value, update.Message),
                "cloud_empty" => string.Format(L["AIProcessingCloudEmpty"].Value, update.Message),
                _ => update.Message
            };
            InvokeAsync(StateHasChanged);
        }

        private void CancelMediaInfo()
        {
            mediaInfoCts?.Cancel();
        }


        private Dictionary<string, object> htmlAttribute { get; set; } = new Dictionary<string, object>()
{
            {"rows", "9" }
    };

        private async Task GetMetadata()
        {
            showAlertMessage = false;
            alertMessage = "";
            if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                string videoId = YouTubeHelpers.GetVideoId(SelectedRecord.FileUrl);
                if (string.IsNullOrEmpty(videoId))
                {
                    ShowMessage(L["ApiNoResults"], MessageSeverity.Warning);
                    return;
                }

                var videoMetadata = await YoutubeService.GetVideoMetadata(videoId);

                if (videoMetadata is not null)
                {
                    if (!blnEdit) // Create new entry
                    {
                        var categoriasMedia = (await categoriasService.GetCategoriesWithMediaEntries()).ToList();
                        var idCategoriaMedia = (byte)categoriasMedia.SingleOrDefault(f => f.Descricao.ToLower().Contains("other")).Id;

                        var formatosMedia = (await formatosService.GetAllAsync()).ToList();
                        var idFormatoMedia = formatosMedia.SingleOrDefault(f => f.Descricao.ToLower().Contains("armazenamento")).Id;
                        SelectedRecord.AnoEdicao = videoMetadata.PublicationDate.Value.Year.ToString();
                        SelectedRecord.Autor = videoMetadata.ChannelTitle;
                        SelectedRecord.FileName = videoMetadata.Title;
                        SelectedRecord.Tempo = videoMetadata.Duration;
                        SelectedRecord.IdGenero = idCategoriaMedia;
                        SelectedRecord.IdFormato_Media = idFormatoMedia;
                        SelectedRecord.Notas = videoMetadata.Description;
                        SelectedRecord.CoverFile = videoMetadata.Thumbnail ?? "";
                        StateHasChanged();
                    }
                    else
                    {
                        SelectedRecord.Tempo = videoMetadata.Duration;
                    }
                }
                else
                {
                    ShowMessage(L["ApiNoResults"], MessageSeverity.Warning);
                }
            }
            else
            {
            }
        }

        private string GetMp3Duration(string fileName)
        {
            using (TagLib.File tagFile = TagLib.File.Create(fileName))
            {
                return tagFile.Properties.Duration.ToString(@"hh\:mm\:ss");
            }
        }

        private async Task<string> GetMp3Album(string artist, string track)
        {
            var albumName = await MediaService.GetMp3Album(artist, track);
            return albumName ?? string.Empty;
        }


        private bool LocalFileOnDisk() // mp3 ou mp4
        {
            if (string.IsNullOrEmpty(SelectedRecord.FilePath))
            {
                return true;
            }

            return File.Exists(SelectedRecord.FilePath);
        }

        public void onMediaTypeChange(ChangeArgs<byte> args)
        {
            SelectedRecord.TipoMedia = args.Value;
        }

        public void onVisualizadoChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
        {
            SelectedRecord.Visualizado = chkVisualizado.Checked;
            StateHasChanged();
        }


        public void OnInput(InputEventArgs args)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(args.Value, "^[0-9]*$") || args.Value.Length != 4)
            {
                CssClass = "e-error";
                ToastIcon = "e-icon-ThumbsDown";
            }
            else
            {
                CssClass = "e-success";
                ToastIcon = "e-icon-ThumbsUp";
            }
            this.StateHasChanged();
        }

        public void UploadYoutubeFile()
        {
            StateHasChanged();
        }

        private void onChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, CategoryVM> args)
        {
            SelectedRecord.IdGenero = args.Value;
        }

        private void onChangeFormat(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, FormatoMediaVM> args)
        {
            SelectedRecord.IdFormato_Media = args.Value;
        }

        private void OnUrlChanged()
        {
            if (string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                ShowMessage("", MessageSeverity.Info);
                StateHasChanged();
            }
        }

        protected async void OnChangeCoverUpload(UploadChangeEventArgs args)
        {
            foreach (var file in args.Files)
            {
                // Pasta física para capas de media (ajuste o nome se quiser)
                var pastaCapasMedia = Path.Combine(Env.WebRootPath, "images", "Media");

                Directory.CreateDirectory(pastaCapasMedia);

                uploadedCoverFile = Path.Combine(pastaCapasMedia, file.FileInfo.Name);

                if (File.Exists(uploadedCoverFile) && !blnEdit)
                {
                    SelectedRecord.CoverFile = $"images/media/{file.FileInfo.Name}";
                    StateHasChanged();
                    continue;
                }

                using (FileStream filestream = new FileStream(uploadedCoverFile, FileMode.Create, FileAccess.Write))
                {
                    await file.File.OpenReadStream(long.MaxValue).CopyToAsync(filestream);
                }

                SelectedRecord.CoverFile = $"images/media/{file.FileInfo.Name}";
                StateHasChanged();
            }
        }

        protected void OnCoverUploadSuccess(Syncfusion.Blazor.Inputs.SuccessEventArgs args)
        {
            // opcional: não é necessário para salvar, já salvamos no ValueChange
        }
        public async Task Voltar()
        {
            ToastTitle = L["DownloadYoutube"];
            ToastCssClass = "e-toast-info";
            ToastContent = L["OperationSaveCancelled"];
            ToastIcon = "e-icon-ThumbsDown";

            YouTubeDialogVisibility = false;
            StateHasChanged();

            await Task.Delay(200);
            await ToastObj.ShowAsync();
        }


        public object[] Tools = new object[]{
        "InsertCode"
    };
        /// <summary>
        /// Populates media fields using AI when only title and author are present (for new records).
        /// Does not affect existing edit logic.
        /// </summary>
        public async Task FillMediaInfoWithAI()
        {
            ShowMessage("", MessageSeverity.Info);
            if (SelectedRecord == null || string.IsNullOrWhiteSpace(SelectedRecord.FileName) || string.IsNullOrWhiteSpace(SelectedRecord.Autor))
            {
                ShowMessage(L["FillAuthorAndTitleFields"], MessageSeverity.Warning);
                StateHasChanged();
                return;
            }

            isBusy = true;
            busyStatusMessage = L["FetchingMetadataViaAlbumFirst"];
            mediaInfoCts?.Cancel();
            mediaInfoCts?.Dispose();
            mediaInfoCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));

            if (spinner is not null)
                await spinner.ShowAsync();

            try
            {
                var uiLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var mediaTitle = SelectedRecord.FileName;
                var artist = SelectedRecord.Autor;


                // AudioDb first
                busyStatusMessage = L["FetchingMetadataViaAudioDb"];
                StateHasChanged();
                var audioDbData = await AudioDbService.GetAlbumDetailsAsync(artist, mediaTitle);
                if (audioDbData is not null)
                {
                    var totalDuration = audioDbData.TotalDuration.ToString(@"hh\:mm\:ss");
                    SelectedRecord.Tempo = totalDuration != "00:00:00" ? totalDuration : SelectedRecord.Tempo;

                    var ranking = ConvertToFiveStarInt(audioDbData.Ranking);
                    if (ranking is not null)
                        SelectedRecord.Rating = (float)ranking;

                    if (audioDbData.Tracks.Count > 0)
                    {
                        var tracks = "";
                        foreach (var track in audioDbData.Tracks)
                        {
                            tracks += track.Title + "\n";
                        }
                        SelectedRecord.Notas = tracks;
                    }
                    if (audioDbData.YearPublished is not null)
                    {
                        SelectedRecord.AnoEdicao = audioDbData.YearPublished.ToString();
                    }
                    if (!string.IsNullOrEmpty(audioDbData.CoverUrl))
                    {
                        SelectedRecord.CoverFile = audioDbData.CoverUrl;
                    }
                    ShowMessage(L["DataObtainedViaAudioDb"], MessageSeverity.Info);

                    return;
                }

                // Try AlbumInfo next
                var artirstAndAlbumData = await MediaService.GetAlbumInfo(artist, mediaTitle);
                if (artirstAndAlbumData.Titulo != null &&
                    artirstAndAlbumData.Tempo != "00:00:00" &&
                    !string.IsNullOrEmpty(artirstAndAlbumData.Notas))
                {
                    SelectedRecord.Tempo = artirstAndAlbumData.Tempo;

                    if (!string.IsNullOrEmpty(artirstAndAlbumData.CoverFile))
                        SelectedRecord.CoverFile = artirstAndAlbumData.CoverFile;

                    SelectedRecord.Notas = artirstAndAlbumData.Notas;
                    ShowMessage(L["DataObtainedViaAlbumFirst"], MessageSeverity.Info);
                    return;
                }


                // Try OMDb next -- commented, api should not be used for this kind of work (just for movies, etc)

                //busyStatusMessage = "A obter metadados via OMDb Api...";
                //StateHasChanged();

                //var dvdInfo = await MediaService.GetOMDbInfo(mediaTitle, artist);
                //if (dvdInfo != null)
                //{
                //    if (!string.IsNullOrEmpty(dvdInfo.Runtime))
                //        SelectedRecord.Tempo = MediaService.ConvertIOMDbTimeStringAsTime(dvdInfo.Runtime);
                //    if (!string.IsNullOrEmpty(dvdInfo.Released) && DataFormat.IsValidDate(dvdInfo.Released))
                //        SelectedRecord.DataMov = MediaService.ConvertIOMDbStringAsDate(dvdInfo.Released);
                //    if (!string.IsNullOrEmpty(dvdInfo.imdbRating))
                //        SelectedRecord.Rating = MediaService.ConvertIMDbRatingToFiveScale(dvdInfo.imdbRating);
                //    if (!string.IsNullOrEmpty(dvdInfo.Actors))
                //        SelectedRecord.Autor = dvdInfo.Actors;
                //    if (!string.IsNullOrEmpty(dvdInfo.Year))
                //        SelectedRecord.AnoEdicao = dvdInfo.Year;
                //    if (!string.IsNullOrEmpty(dvdInfo.Awards))
                //        SelectedRecord.Notas = dvdInfo.Awards;
                //    if (!string.IsNullOrEmpty(dvdInfo.Poster))
                //        SelectedRecord.CoverFile = dvdInfo.Poster;

                //    ShowMessage("Dados obtidos via OMDb Api", MessageSeverity.Info);

                //    return;
                //}


                // If all above fail, use AI (OpenRouter primeiro, depois Ollama)

                busyStatusMessage = L["FetchingMetadataViaAI"];
                StateHasChanged();

                var apiUrlAddress = await GetApiBaseUrlAsync();
                await ProgressClient.StartAsync(apiUrlAddress);

                ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
                ProgressClient.ProgressEventReceived += OnProgressEventReceived;

                var ai = await AiMediaClient.GetMediaMetadataCloudAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId, null, "media-api", mediaInfoCts.Token);
                var aiProviderMessage = L["MediaInfoProviderOpenRouter"];
                if (ai is null)
                {
                    ai = await AiMediaClient.GetMediaMetadataAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId, null, "media-api", mediaInfoCts.Token);
                    if (ai is not null)
                        aiProviderMessage = L["MediaInfoProviderOllama"];
                }

                if (ai is not null)
                {
                    if (!string.IsNullOrEmpty(ai.Runtime))
                        SelectedRecord.Tempo = ai.Runtime;
                    else if (!string.IsNullOrEmpty(ai.Tempo))
                        SelectedRecord.Tempo = ai.Tempo;
                    if (!string.IsNullOrEmpty(ai.Poster) && ai.Poster != "Unknown poster image")
                        SelectedRecord.CoverFile = ai.Poster;
                    else if (!string.IsNullOrEmpty(ai.CoverFile) && ai.CoverFile != "Unknown poster image")
                        SelectedRecord.CoverFile = ai.CoverFile;
                    if (!string.IsNullOrEmpty(ai.Notes))
                        SelectedRecord.Notas = ai.Notes;
                    if (!string.IsNullOrEmpty(ai.Released))
                    {
                        if (DateTime.TryParse(ai.Released, out var releasedDate))
                            SelectedRecord.DataMov = releasedDate;
                    }
                    if (!string.IsNullOrEmpty(ai.ImdbRating))
                    {
                        if (double.TryParse(ai.ImdbRating, NumberStyles.Any, CultureInfo.InvariantCulture, out var rating))
                            SelectedRecord.Rating = rating;
                    }
                    if (!string.IsNullOrEmpty(ai.Actors))
                        SelectedRecord.Autor = ai.Actors;
                    if (ai.Year != null)
                    {
                        var yearStr = ai.Year.ToString();
                        if (!string.IsNullOrEmpty(yearStr))
                            SelectedRecord.AnoEdicao = yearStr;
                    }

                    var warningMsg = !string.IsNullOrEmpty(ai.Warnings) ? ai.Warnings : ai.Warning;
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        ShowMessage(warningMsg, MessageSeverity.Info);
                    }
                    else
                    {
                        ShowMessage(aiProviderMessage, MessageSeverity.Info);
                    }
                }
                else
                {
                    ShowMessage(L["AINoResults"], MessageSeverity.Warning);
                }
            }
            catch (OperationCanceledException)
            {
                ShowMessage(L["OperacaoCancelada"], MessageSeverity.Warning);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching AI media info for {MediaTitle} by {Artist}", SelectedRecord.FileName, SelectedRecord.Autor);
                ShowMessage(L["ErrorFetchingMetadataViaAI", ex.Message], MessageSeverity.Error);
            }
            finally
            {
                if (spinner is not null)
                    await spinner.HideAsync();
                isBusy = false;
                mediaInfoCts?.Dispose();
                ProgressClient.ProgressEventReceived -= OnProgressEventReceived;
                mediaInfoCts = null;
                StateHasChanged();
            }
        }

        private Task OpenTranslateDialog()
        {
            // Ensure InitialText is read by the TranslateText component (it reads InitialText on parameters set)
            // Set default languages if you want different defaults
            TranslateVisible = true;
            return Task.CompletedTask;
        }
        private void ClearNotes()
        {
            SelectedRecord.Notas = "";
            StateHasChanged();
        }

        private async Task HandleTranslateRequested(TranslateRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
                return;

            SelectedRecord.Notas = dto.Text;
            TranslateVisible = false;
            StateHasChanged();
            await Task.CompletedTask;
        }

        private static int? ConvertToFiveStarInt(double? value)
        {
            if (!value.HasValue)
                return null;

            return Math.Clamp(
                (int)Math.Round(value.Value / 2.0),
                0,
                5);
        }
        private void ShowMessage(string message, MessageSeverity severity = MessageSeverity.Warning)
        {
            if (!showAlertMessage)
                showAlertMessage = true;

            alertMessage = message;
            messageSeverity = severity;
            alertMessageColor = severity switch
            {
                MessageSeverity.Info => "text-white",
                MessageSeverity.Warning => "text-black",
                MessageSeverity.Error => "text-red ",
                _ => "text-white"
            };

            StateHasChanged();
        }
    }
}
