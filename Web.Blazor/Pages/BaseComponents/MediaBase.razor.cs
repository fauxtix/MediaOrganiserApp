using Google.Apis.YouTube.v3.Data;
using MediaInfo;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Drawing;
using System.Text;
using Web.Blazor.Data;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.Common;
using Web.Blazor.Services.DocumentHelpers;

namespace Web.Blazor.Pages.BaseComponents
{
    public class MediaBase : ComponentBase, IDisposable
    {
        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        [Inject] public IPdfGeneratorService PdfGeneratorService { get; set; }
        [Inject] public IMediaServiceWrapper MediaService { get; set; }
        [Inject] public IMediaThumbnailsService ThumbnailsService { get; set; }
        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public ILogger<App> logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IJSRuntime js { get; set; }
        [Inject] public IGetYoutubeVideoMetadata YouTubeService { get; set; }
        [Inject] public IWebHostEnvironment Env { get; set; }
        [Inject] public IMostUsedService mostUsedService { get; set; }
        [Inject] public IUtilitiesService Utils { get; set; }

        [Inject] public IUserService UserService { get; set; }

        protected SfGrid<MediaVM> gridObj;

        protected SfToast ToastObj;

        protected DialogEffect Effects = DialogEffect.SlideTop;

        protected SfSpinner SpinnerObj;
        protected string SpinnerLabel { get; set; } = "";

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";
        public int ToastTimeOut { get; set; } = 1500;
        protected string spinnerTarget { get; set; } = "#spinnerContainer";

        protected IEnumerable<MediaVM> mediaFiles;

        protected YouTubeVideoDetails VideoDetail = new();

        protected MediaVM SelectedRecord = new MediaVM();
        protected bool blnEdit = true;
        protected string OperationResult { get; set; }
        protected string CriarEditarMsg; // = "Editar Registo";
        protected string MediaTypeMsg = "Play Mp3";

        protected bool isMp3 = false;
        protected bool isMp4 = false;
        protected bool isCD = false;
        protected bool isLP = false;

        protected Image mp3Cover { get; set; }
        protected string mediaFile { get; set; } = "";
        protected string mediaFilename { get; set; } = "";
        protected string mediaExtension { get; set; } = "";

        protected Mp4Handler jsMp4Handler;


        // Dialogs visibility flags
        protected bool MediaCardInfoVisibility { get; set; } = false;
        protected bool Mp3CardInfoVisibility { get; set; } = false;
        public bool ReadPdfVisibility { get; set; } = false;
        public bool YoutubeDialogVisibility { get; set; }
        protected bool Mp3DialogVisibility { get; set; } = false;
        protected bool ErrorVisibility { get; set; } = false;
        protected bool YouTubeDialogVisibility { get; set; } = false;
        protected bool YouTubePlayerDialogVisibility { get; set; } = false;
        protected bool EditMediaDialogVisibility { get; set; } = false;
        protected bool ValidationErrorsVisibility { get; set; } = false;
        protected bool PlayerDialogVisibility { get; set; } = false;
        public bool MaintainEditDialogOpen { get; set; } = false;
        public bool MaintainYoutubeDialogOpen { get; set; } = false;
        public bool UploadMediaVisibility { get; set; } = false;
        public bool AlertUserVisibility { get; set; } = false;
        public string AlertUserTitle { get; set; } = string.Empty;
        public string AlertUserMessage { get; set; } = string.Empty;
        public AppDefinitions.OpcoesRegisto AlertUserMessageType { get; set; } = AppDefinitions.OpcoesRegisto.Info;

        protected string ErrorTitle { get; set; } // = "Dados inválidos ou em falta";
        protected bool DeleteRecordConfirmVisibility { get; set; } = false;

        protected int MediaID { get; set; }
        protected string authorImage { get; set; }
        protected int pubsCatalogadas { get; set; }
        protected List<string> ValidationMessages;
        protected bool isValidMedia = true;
        protected string dialogWidth;
        protected string dialogHeight;
        protected string dialogMinHeight;

        public string PdfFilePath { get; set; } = "";
        protected MediaVM selectedRowData { get; set; }

        protected bool InsertUpdateOk;

        protected int primaryIndex;

        protected AppDefinitions.Query_TipoMedia reportOption = AppDefinitions.Query_TipoMedia.Todos;
        protected SfRadioButton<string> RadioButtonInstance;

        protected bool showBadge;
        protected string badgeCaption;
        protected bool showStoredPathEntry = true;

        protected bool AddToPlaylistDialogVisibility { get; set; } = false;

        protected bool CanAddSelectedMediaToPlaylist =>
            IsEligibleForPlaylist(SelectedRecord);

        protected override async Task OnInitializedAsync()
        {
            showBadge = false;
            badgeCaption = "";
            MediaCardInfoVisibility = false;
            Mp3CardInfoVisibility = false;
            isMp3 = false;
            isMp4 = false;
            isCD = false;
            isLP = false;
            ErrorTitle = localizerApp["TituloDadosInvalidos"];
            CriarEditarMsg = localizerApp["EditMsg"];
            SpinnerLabel = localizerApp["LoadingMsg"];

            jsMp4Handler = new Mp4Handler(js);
            mediaFiles = await GetAllMediaFiles();
        }

        protected async void rbValueChange(ChangeArgs<string> args)
        {
            var index = await gridObj.GetRowIndexByPrimaryKeyAsync(args.Value); // Fetch the row index based on the unique value of RadioButton.
            await gridObj.SelectRowAsync(index); // Select the corresponding Grid row.
        }
        public async void OnRowSelected(RowSelectEventArgs<MediaVM> args)
        {
            selectedRowData = args.Data;
            primaryIndex = await this.gridObj.GetRowIndexByPrimaryKeyAsync(selectedRowData.Id);
        }

        protected async void OnRecordDoubleClick(RecordDoubleClickEventArgs<MediaVM> args)
        {
            var data = args.RowData;
            MediaID = data.Id;
            SelectedRecord = await MediaService.GetMediaByID(MediaID);

            EditMediaDialogVisibility = true;
            blnEdit = true;
            CriarEditarMsg = localizerApp["EditMsg"];
            StateHasChanged();
        }

        public async Task OnCommandClicked(CommandClickEventArgs<MediaVM> args)
        {
            var data = args.RowData;
            var optiontype = args.CommandColumn.Title.ToLower();
            MediaID = data.Id;
            primaryIndex = await gridObj.GetRowIndexByPrimaryKeyAsync(data.Id);

            if (primaryIndex >= 0)
            {
                await gridObj.SelectRowAsync(primaryIndex, true);
            }

            SelectedRecord = await MediaService.GetMediaByID(MediaID);
            var mediaFormatIsLP_or_CD = (SelectedRecord.FormatoMedia.ToLower() == "cd" || SelectedRecord.FormatoMedia.ToLower() == "lp");

            if (mediaFormatIsLP_or_CD)
            {
                await ProcessCDOrLPInfo(optiontype);
            }

            if (!string.IsNullOrEmpty(SelectedRecord.FileName))
            {
                ProcessMediaFileDetails(mediaFormatIsLP_or_CD);
            }

            HandleCommand(args);
        }

        private async Task ProcessCDOrLPInfo(string optionTitle)
        {
            SpinnerLabel = "Accessing youtube Api (SearchByArtistAndSong)";
            var mediaArtist = SelectedRecord.Autor;
            await Task.Delay(100);
            await SpinnerObj?.ShowAsync();
            var cdMetadata = await YouTubeService.SearchByArtistAndSong(mediaArtist, SelectedRecord.FileName);
            await SpinnerObj?.HideAsync();

            if (cdMetadata != null)
            {
                var videoId = cdMetadata.VideoId;
                SelectedRecord.FileUrl = videoId != null
                    ? $"https://www.youtube.com/watch?v={videoId}"
                    : "https://www.youtube.com/watch?v=83-EE2MnUVU"; // 'nothing was found' clip

            }
            else
            {
                YouTubePlayerDialogVisibility = false;
                EditMediaDialogVisibility = false;
                ErrorVisibility = false;
                AlertUserVisibility = false;


                if (optionTitle == "info") // 'info' option
                {
                    AlertUserTitle = $"{localizerApp["TituloConsultarInfo"]} - '{SelectedRecord.FileName} ({SelectedRecord.Autor}')";
                    AlertUserMessage = $"{localizerApp["TituloNoDataToShow"]}.";
                    isValidMedia = false;
                    AlertUserVisibility = true;
                }
                else if (optionTitle == "play")
                {
                    AlertUserMessage = $"{localizerApp["lblTitulo"]}'{SelectedRecord.FileName}' ({SelectedRecord.Autor}) {localizerApp["TituloNaoPodeSerReproduzido"]}.";
                    AlertUserTitle = $"{localizerApp["TipoFicheiro"]} ({SelectedRecord.FormatoMedia})";
                    isValidMedia = false;
                    AlertUserVisibility = true;
                }
            }
        }

        private async void HandleCommand(CommandClickEventArgs<MediaVM> args)
        {
            switch (args.CommandColumn.Type)
            {
                case CommandButtonType.None:
                    if (args.CommandColumn.Title.ToLower() == "info" && isMp4)
                    {
                        MediaCardInfoVisibility = false;
                        YouTubePlayerDialogVisibility = false;
                        Mp3CardInfoVisibility = false;
                        await ProcessMp4InfoRequest(DialogsVisibility.MediaCardInfo);
                    }
                    if (args.CommandColumn.Title.ToLower() == "info" && isMp3)
                    {
                        Mp3CardInfoVisibility = true;
                        MediaCardInfoVisibility = false;
                    }
                    //if (args.CommandColumn.Title.ToLower() == "playlist" && isMp3)
                    //{
                    //    Mp3CardInfoVisibility = true;
                    //    MediaCardInfoVisibility = false;
                    //}
                    else if (args.CommandColumn.Title.ToLower() == "play")
                    {
                        if (isMp4)
                        {
                            SetMp4DialogProperties();
                            await ProcessMp4InfoRequest(DialogsVisibility.YouTubePlayer);
                        }
                        else if (isMp3)
                        {
                            if (!File.Exists(SelectedRecord.FilePath))
                            {
                                var fileTitle = localizerApp["lblTitulo"];
                                var noPlay = localizerApp["TitleCannotBeReproduced"] + ".";
                                var notFound = localizerApp["TituloFicheiroNaoExisteLocalEspecificado"] + ".";
                                var contactAdmin = localizerApp["ContactAdmin"];
                                StringBuilder sb = new StringBuilder();
                                sb.Append(notFound);
                                sb.AppendLine("\n");
                                sb.AppendLine(contactAdmin);
                                AlertUserTitle = $"Mp3 player - {noPlay}";
                                AlertUserMessage = $"{fileTitle} '{SelectedRecord.FileName} ({SelectedRecord.Autor})' {sb.ToString()}";
                                isValidMedia = false;
                                Mp3DialogVisibility = false;
                                AlertUserVisibility = true;
                                AlertUserMessageType = AppDefinitions.OpcoesRegisto.Warning;
                                StateHasChanged();
                                return;
                            }

                            ProcessOtherInfoRequests();
                        }
                    }
                    break;
                case CommandButtonType.Edit:
                    EditMediaDialogVisibility = true;
                    blnEdit = true;
                    CriarEditarMsg = localizerApp["EditMsg"];
                    StateHasChanged();
                    break;
                case CommandButtonType.Delete:
                    DeleteRecordConfirmVisibility = true;
                    StateHasChanged();
                    break;
            }
        }
        private void ProcessMediaFileDetails(bool mediaFormatIsLP_or_CD)
        {
            if (!string.IsNullOrEmpty(SelectedRecord.FilePath))
            {
                mediaFile = SelectedRecord.FilePath;

                System.IO.FileInfo fi = new System.IO.FileInfo(mediaFile);
                mediaFilename = fi.Name;

                mediaExtension = fi.Extension.ToLower();
                isMp3 = (mediaExtension == ".mp3");
                isMp4 = (mediaExtension == ".mp4");
            }
            else
            {
                ProcessMediaFileDetailsForUrl(SelectedRecord.FileUrl, mediaFormatIsLP_or_CD);
            }
        }

        private void ProcessMediaFileDetailsForUrl(string fileUrl, bool mediaFormatIsLP_or_CD)
        {
            if (!string.IsNullOrEmpty(fileUrl)) // && !mediaFormatIsLP_or_CD)
            {
                if (fileUrl.ToLower().Contains("watch"))
                {
                    isMp4 = true;
                    mediaExtension = ".mp4";
                }
            }
            else
            {
                isMp3 = false; // TODO validate better if the file has no extension
                isMp4 = false;
                mediaExtension = "";
            }
        }

        private async Task ProcessMp4InfoRequest(DialogsVisibility visibility)
        {
            if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
            {
                int iPos = SelectedRecord.FileUrl.IndexOf("v=") + 2;
                string videoId = SelectedRecord.FileUrl.Substring(iPos);
                var output = await YouTubeService.GetVideoMetadata(videoId);
                if (output is not null)
                {

                    if (output.VideoId == "-1") // value retuened from method in case of an error
                    {
                        AlertUserTitle = $"{localizerApp["TituloConsultarInfo"]} - '{SelectedRecord.FileName} ({SelectedRecord.Autor}')";
                        AlertUserMessage = $"{localizerApp["TituloNoDataToShow"]}.";
                        isValidMedia = false;
                        AlertUserVisibility = true;
                        return;
                    }


                    VideoDetail = output;
                    if (visibility == DialogsVisibility.MediaCardInfo)
                    {
                        MediaCardInfoVisibility = true;
                        YoutubeDialogVisibility = false;
                    }
                    else
                    {
                        YoutubeDialogVisibility = true;
                        MediaCardInfoVisibility = false;
                    }
                }
                else
                {
                    YoutubeDialogVisibility = false;
                    ErrorVisibility = true;
                    ValidationMessages = new() { $"{localizerApp["msgNoResults"]}  ({SelectedRecord.FileName}) - {localizerApp["lblAutor"]}: {SelectedRecord.Autor}" };
                    ErrorTitle = $"Google Api";
                    VideoDetail = new();
                }
            }
            else if (!string.IsNullOrEmpty(SelectedRecord.FilePath))
            {
                try
                {
                    var fileName = SelectedRecord.FilePath.Replace(@"/", @"\");
                    var media = new MediaInfoWrapper(fileName, logger);
                    if (media.Success)
                    {
                        var containerFormat = media.Format;
                        var mediaHasVideo = media.HasVideo;
                        var videoBitRate = media.VideoRate;
                        foreach (var stream in media.AudioStreams)
                        {
                            var codec = stream.Codec;
                        }
                        VideoDetail = new()
                        {
                            ChannelTitle = media.AudioChannels == 0 ? "" : "",
                            Description = media.Text,
                            Duration = media.VideoStreams[0].Duration.ToString(),
                            PublicationDate = SelectedRecord.DataMov,
                            Thumbnail = "/images/No-image-available.png",
                            Title = SelectedRecord.Titulo,
                            VideoId = ""
                        };
                        if (visibility == DialogsVisibility.MediaCardInfo)
                        {
                            MediaCardInfoVisibility = true;
                            YoutubeDialogVisibility = false;
                        }
                        else
                        {
                            YoutubeDialogVisibility = true;
                            MediaCardInfoVisibility = false;
                        }
                    }

                }
                catch (Exception ex)
                {
                    MediaCardInfoVisibility = false;
                    ErrorVisibility = true;
                    ValidationMessages = new() { $"{localizerApp["lblTitulo"]} ({SelectedRecord.FileName}) {ex.Message}." };
                    ErrorTitle = localizerApp["msgNoResults"];

                }
            }
            else
            {
                MediaCardInfoVisibility = false;
                ErrorVisibility = true;
                ValidationMessages = new() { $"{localizerApp["lblTitulo"]} ({SelectedRecord.FileName}) - {localizerApp["lblAutor"]}: {SelectedRecord.Autor} está guardado em disco. Pesquisa ao API do Youtube não efetuada." };
                ErrorTitle = localizerApp["msgNoResults"];
            }

            isMp3 = false;
            StateHasChanged();

        }

        private void ProcessOtherInfoRequests()
        {
            if (isMp3)
            {
                SetMp3DialogProperties();
            }
            else if (isMp4) // should never get hitted !!
            {
                SetMp4DialogProperties();
            }
            else if (!string.IsNullOrEmpty(SelectedRecord.FileName) &&
                string.IsNullOrEmpty(SelectedRecord.FileUrl) &&
                string.IsNullOrEmpty(SelectedRecord.FilePath))
            {
                Mp3DialogVisibility = false;
                EditMediaDialogVisibility = false;
                ErrorVisibility = true;
                ValidationMessages = new() { $"{localizerApp["lblTitulo"]} '{SelectedRecord.FileName} ({SelectedRecord.Autor})' {localizerApp["TituloNaoPodeSerReproduzido"]}." };
                ErrorTitle = $"{localizerApp["TipoFicheiro"]} ({SelectedRecord.FormatoMedia}) {localizerApp["TituloIncompativel"]}";
                isValidMedia = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedRecord.FileUrl))
                {
                    mediaFilename = SelectedRecord.FileUrl;
                    mediaExtension = ".mp4";
                    YouTubePlayerDialogVisibility = true;
                    EditMediaDialogVisibility = false;
                }
                else
                {
                    Mp3DialogVisibility = false;
                    EditMediaDialogVisibility = false;
                    ErrorVisibility = true;
                    ValidationMessages = new() { $"{localizerApp["TituloFicheiro"]} {Path.GetFileName(SelectedRecord.FilePath)} {localizerApp["TituloFormatoMediaInvalido"]}" };
                    isValidMedia = false;
                }
            }
            StateHasChanged();
        }

        private void SetMp3DialogProperties()
        {
            MediaTypeMsg = "Play Mp3";
            mediaExtension = ".mp3";
            dialogWidth = "400px";
            dialogHeight = "auto";
            dialogMinHeight = "400px";
            Mp3DialogVisibility = true;
            isMp4 = false;
            StateHasChanged();
        }

        private void SetMp4DialogProperties()
        {
            YouTubePlayerDialogVisibility = true;
            MediaTypeMsg = "Play Mp4";
            mediaExtension = ".mp4";
            dialogWidth = "800px";
            dialogHeight = "800px";
            dialogMinHeight = "800px";
            isMp3 = false;
            StateHasChanged();
        }
        protected async Task StopAudio()
        {
            await js.InvokeVoidAsync("StopPlayAudioFile");
        }

        protected async Task DisposeAudio()
        {
            await js.InvokeVoidAsync("DisposeAudioFile");
            mediaFilename = "";
            StateHasChanged();
        }

        protected async void PlayVideo()
        {
            if (!File.Exists(mediaFilename))
            {
                await jsMp4Handler.SelectedMp4Changed(SelectedRecord.FileUrl);
            }
            else
            {
                await jsMp4Handler.SelectedMp4Changed($"/uploads/media/{mediaFilename}");
            }
        }
        protected async void PauseVideo()
        {
            await jsMp4Handler.PauseVideo($"/uploads/media/{mediaFilename}");
        }
        protected async Task StopVideo()
        {
            await jsMp4Handler.StopVideo($"/uploads/media/{mediaFilename}");
        }

        public async Task OnToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            string userId;
            string role;
            var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);
            userId = userDetails.UserId;
            role = userDetails.Role;

            switch (args.Item.Id.ToLower())
            {
                case "clear":
                    showBadge = false;
                    await gridObj.SearchAsync("");
                    break;
                case "aud":
                    mediaFiles = await MediaService.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Audio);
                    reportOption = AppDefinitions.Query_TipoMedia.Audio;
                    showBadge = true;
                    badgeCaption = "Audio";
                    break;
                case "vid":
                    mediaFiles = await MediaService.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Video);
                    reportOption = AppDefinitions.Query_TipoMedia.Video;
                    showBadge = true;
                    badgeCaption = "Video";

                    break;
                case "others":
                    mediaFiles = await MediaService.GetFilesByMediaType(AppDefinitions.Query_TipoMedia.Others);
                    reportOption = AppDefinitions.Query_TipoMedia.Others;
                    showBadge = true;
                    badgeCaption = "Outros";

                    break;
                case "all":
                    mediaFiles = await MediaService.GetAllMedia_Async(userId, role);
                    reportOption = AppDefinitions.Query_TipoMedia.Todos;
                    showBadge = false;
                    badgeCaption = "";
                    break;
                case "cover":
                    await ThumbnailsService.UpdateMediaCoverRecords();
                    break;
                case "mlpdf":
                    await SpinnerObj.ShowAsync();
                    var pdfStream = PdfGeneratorService.CreateMediaPdf(mediaFiles.ToList());
                    PdfFilePath = $"data:application/pdf;base64,{Convert.ToBase64String(pdfStream.ToArray())}";

                    await SpinnerObj.HideAsync();
                    ReadPdfVisibility = true;
                    await InvokeAsync(StateHasChanged);

                    break;
                default:
                    mediaFiles = await MediaService.GetAllMedia_Async(userId, role);
                    showBadge = false;
                    badgeCaption = "";

                    break;
            }


        }
        public async Task CloseDialog(DialogsVisibility option)
        {
            switch (option)
            {
                case DialogsVisibility.EditDialog:
                    MaintainEditDialogOpen = false;
                    break;
                case DialogsVisibility.MediaCardInfo:
                    MediaCardInfoVisibility = false;
                    break;
                case DialogsVisibility.Mp3CardInfo:
                    Mp3CardInfoVisibility = false;
                    break;
                case DialogsVisibility.Youtube:
                    YouTubeDialogVisibility = false;
                    mediaFilename = "";
                    break;
                case DialogsVisibility.YouTubePlayer:
                    YouTubePlayerDialogVisibility = false;
                    mediaFilename = "";
                    SelectedRecord = new MediaVM();
                    await StopVideo();
                    break;

                case DialogsVisibility.Mp3Player:
                    await DisposeAudio();
                    Mp3DialogVisibility = false;
                    SelectedRecord = new MediaVM();
                    StateHasChanged();

                    break;

                case DialogsVisibility.ValidationErrors:
                    ValidationErrorsVisibility = false;
                    MaintainEditDialogOpen = true;
                    break;
                case DialogsVisibility.Error:
                    ErrorVisibility = false;
                    MaintainEditDialogOpen = true;
                    break;
                case DialogsVisibility.DeleteRecordConfirmation:
                    DeleteRecordConfirmVisibility = false;
                    break;
                case DialogsVisibility.UploadFilesDialog:
                    UploadMediaVisibility = false;
                    break;
            }
            EditMediaDialogVisibility = MaintainEditDialogOpen;
            StateHasChanged();
        }

        public async Task<IEnumerable<MediaVM>> GetAllMediaFiles()
        {

            string userId;
            string role;
            var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);
            userId = userDetails.UserId;
            role = userDetails.Role;

            if (userId is null)
            {
                // inform user is not authenticated...
            }

            var mediaList = await MediaService.GetAllMedia_Async(userId, role);
            return mediaList;
        }

        public async void OnLoad()
        {
            await SpinnerObj.ShowAsync();
        }
        public async void DataBound()
        {
            await SpinnerObj.HideAsync();
        }
        public async void OnActionBegin(ActionEventArgs<MediaVM> args)
        {
            if (args.RequestType.ToString() == "Sorting")
            {
                await SpinnerObj.ShowAsync();
            }
            if (args.RequestType.ToString() == "Grouping")
            {
                await SpinnerObj.ShowAsync();
            }
        }
        public async Task AddOrSaveRecord()
        {
            //Validate entries
            ValidationMessages = MediaService.RegistoComErros(SelectedRecord);
            if (ValidationMessages.Any() == false)
            {
                ToastCssClass = "e-toast-success";
                if (blnEdit)
                {
                    await UpdateRecord();
                }
                else
                {
                    await InsertRecord();
                }

                await InformUserAndRefreshGrid();
            }
            else
            {
                ErrorVisibility = true;
                MaintainEditDialogOpen = true;
            }
        }

        private async Task UpdateRecord()
        {
            var album = SelectedRecord.FileName;
            var artist = SelectedRecord.Autor;
            var isLPorCD = SelectedRecord.IdFormato_Media > 1 && SelectedRecord.IdFormato_Media < 4;
            if (isLPorCD)
            {
                var artirstAndAlbumData = await MediaService.GetAlbumInfo(artist, album);
                if (artirstAndAlbumData is not null)
                {
                    SelectedRecord.Tempo = artirstAndAlbumData.Tempo;
                    SelectedRecord.CoverFile = artirstAndAlbumData.CoverFile;
                    SelectedRecord.Notas = artirstAndAlbumData.Notas;
                }
            }

            InsertUpdateOk = await MediaService.UpdateMediaFile(SelectedRecord);
            ToastContent = InsertUpdateOk ? localizerApp["SuccessUpdate"] : localizerApp["FalhaGravacaoRegisto"];
            ToastCssClass = InsertUpdateOk ? "e-toast-success" : "e-toast-danger";
            ToastTitle = localizerApp["editionMsg"];
            ToastTimeOut = 3000;
            if (!InsertUpdateOk)
            {
                logger.LogWarning(ToastContent);
            }

            //StateHasChanged();
            //await Task.Delay(100);
            //await ToastObj.ShowAsync();
        }

        private async Task InsertRecord()
        {
            var isLPorCD = SelectedRecord.IdFormato_Media > 1 && SelectedRecord.IdFormato_Media < 6;

            var mediaTitle = SelectedRecord.FileName;
            var artist = SelectedRecord.Autor;
            var titleExist = await MediaService.MediaFileExists(mediaTitle, artist);
            if (titleExist)
            {
                EditMediaDialogVisibility = false;
                ErrorVisibility = true;
                ValidationMessages = new() { $"'{SelectedRecord.FileName} ({SelectedRecord.Autor})' {localizerApp["msgUploadCreationError"]}." };
                ErrorTitle = $"{localizerApp["creationMsg"]} {localizerApp["Record"]}";
                isValidMedia = false;
                return;
            }

            var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);

            SelectedRecord.CreatedBy = userDetails.UserId;

            ToastTitle = localizerApp["creationMsg"] + " Media";
            InsertUpdateOk = await MediaService.CreateMediaFile(SelectedRecord);
            if (InsertUpdateOk == false)
            {
                ToastCssClass = "e-toast-danger";
                ToastContent = localizerApp["FalhaCriacaoRegisto"] + " (Media)";
                ToastTimeOut = 4000;
                logger.LogWarning(ToastContent);
                //StateHasChanged();
                //await Task.Delay(100);
                //await ToastObj.ShowAsync();

            }
            else
            {
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessInsert"];
                ToastTimeOut = 3000;
                SelectedRecord = new();
                //StateHasChanged();
                //await Task.Delay(100);
                //await ToastObj.ShowAsync();
            }
        }

        protected void NotifyYoutubeDialog()
        {
            YouTubeDialogVisibility = true;
        }
        public async Task NotifyAddingRecord()
        {
            showStoredPathEntry = false; // 01/2026

            string mostUsedMediaFormat = await mostUsedService.GetOpcaoMaisUsada("idFormato_Media", "Formato_Media", "Formato_Media.Id", "Descricao", "Media");
            var formatoMediaId = Utils.GetPkValueFromTable("Formato_Media", "Id", "Descricao", !string.IsNullOrEmpty(mostUsedMediaFormat) ? mostUsedMediaFormat : "A partir de URL");
            string mostUsedCategory = await mostUsedService.GetOpcaoMaisUsada("idGenero", "Categoria", "Categoria.Id", "Descricao", "Media");
            var categoriaMediaId = Utils.GetPkValueFromTable("Categoria", "Id", "Descricao", !string.IsNullOrEmpty(mostUsedCategory) ? mostUsedCategory : "Sem Género");

            SelectedRecord = new MediaVM()
            {
                DataMov = DateTime.Now,
                AnoEdicao = DateTime.Now.Year.ToString(),
                TipoMedia = 2,
                Visualizado = true, // vamos presumir que já ouvimos/vimos o título que vamos registar
                IdFormato_Media = formatoMediaId,
                IdGenero = categoriaMediaId
            };

            blnEdit = false;
            CriarEditarMsg = localizerApp["NewMsg"];

            EditMediaDialogVisibility = true;
            StateHasChanged();
        }

        public async Task NotifyAddingYoutubeRecord()
        {
            int iFormato = await MediaService.GetMediaFormatByDescription("A partir de URL");
            int iGenero = await MediaService.GetMediaCategoryByDescription("Tutorial - Vídeo - .Net");
            SelectedRecord = new MediaVM()
            {
                Titulo = "",
                DataMov = DateTime.Now,
                TipoMedia = 1, // Default Audio -- doesn't work !!               
                Visualizado = true, // we'll assume that we've already listen/read the title about to be created
                IdFormato_Media = iFormato,
                IdGenero = iGenero
            };

            blnEdit = false;
            CriarEditarMsg = localizerApp["NewMsg"] + " (Youtube)";

            YouTubeDialogVisibility = true;
            StateHasChanged();
        }
        public async Task ConfirmDeleteYes()
        {
            if (await MediaService.DeleteMediaFile(MediaID))
            {
                var filepath = SelectedRecord.FilePath;
                if (!string.IsNullOrEmpty(filepath))
                {
                    try
                    {
                        if (File.Exists(filepath))
                        {
                            File.Delete(filepath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ToastTitle = localizerApp["deletionMsg"] + " Media";
                        ToastCssClass = "e-toast-danger";
                        ToastContent = $"Erro ao apagar ficheiro {SelectedRecord.FilePath} ({ex.Message}. Verifique, por favor)";
                    }
                }
                var coverFilePath = SelectedRecord.CoverFile;
                if (!string.IsNullOrEmpty(coverFilePath))
                {
                    try
                    {
                        if (File.Exists(coverFilePath))
                        {
                            File.Delete(coverFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ToastTitle = localizerApp["deletionMsg"] + " Media";
                        ToastCssClass = "e-toast-danger";
                        ToastContent = $"Erro ao apagar ficheiro {coverFilePath} ({ex.Message}. Verifique, por favor)";
                    }
                }


                DeleteRecordConfirmVisibility = false;
                mediaFiles = await GetAllMediaFiles();
                await gridObj.Refresh();

                ToastTitle = localizerApp["deletionMsg"] + " Media";
                ToastCssClass = "e-toast-success";
                ToastContent = localizerApp["SuccessDelete"];
            }
            else
            {
                ToastTitle = localizerApp["deletionMsg"] + " Media";
                ToastCssClass = "e-toast-error";
                ToastContent = localizerApp["FalhaAnulacaoRegisto"];
                logger.LogError(ToastContent);
            }

            StateHasChanged();
            await Task.Delay(100);
            await ToastObj.ShowAsync();
        }

        public void FecharPdf()
        {
            PdfFilePath = "";
            ReadPdfVisibility = false;
            StateHasChanged();
        }

        protected async Task ShowUploadResult(UploadMediaResult uploadMediaResult)
        {
            if (uploadMediaResult.Flag)
            {
                mediaFiles = await GetAllMediaFiles();
                ToastTimeOut = 5000;
                ToastTitle = "Upload Media";
                ToastCssClass = "e-toast-info";
                ToastContent = localizerApp["TituloFicheiroProcessadoSucesso"];

                UploadMediaVisibility = false;
                await Task.Delay(200);
                StateHasChanged();
                await ToastObj.ShowAsync();

            }
            else
            {
                AlertUserVisibility = true;
                AlertUserTitle = "Ficheiros locais";
                AlertUserMessage = uploadMediaResult.Message;
                AlertUserMessageType = AppDefinitions.OpcoesRegisto.Warning;
                StateHasChanged();
            }
        }


        protected async Task HideToast()
        {
            await Task.Delay(100);
            await ToastObj?.HideAsync();
        }


        protected async Task GetMediaInfo()
        {
            var isLPorCD = SelectedRecord.IdFormato_Media > 1 && SelectedRecord.IdFormato_Media < 6;

            var mediaTitle = SelectedRecord.FileName;
            var artist = SelectedRecord.Autor;

            if (SelectedRecord.IdFormato_Media == 4) // DVD
            {
                var dvdInfo = await MediaService.GetOMDbInfo(mediaTitle, artist);
                if (dvdInfo is not null)
                {
                    if (dvdInfo.Runtime is not null)
                    {
                        SelectedRecord.Tempo = MediaService.ConvertIOMDbTimeStringAsTime(dvdInfo.Runtime);
                    }
                    if (dvdInfo.Released is not null)
                    {
                        SelectedRecord.DataMov = MediaService.ConvertIOMDbStringAsDate(dvdInfo.Released);
                    }
                    if (dvdInfo.imdbRating is not null)
                    {
                        SelectedRecord.Rating = MediaService.ConvertIMDbRatingToFiveScale(dvdInfo.imdbRating);
                    }
                    SelectedRecord.Autor = dvdInfo.Actors;
                    SelectedRecord.AnoEdicao = dvdInfo.Year;
                    SelectedRecord.Notas = dvdInfo.Awards;
                    SelectedRecord.CoverFile = dvdInfo.Poster;
                }
            }
            else if (isLPorCD)
            {
                var artirstAndAlbumData = await MediaService.GetAlbumInfo(artist, mediaTitle);
                if (artirstAndAlbumData.Titulo is not null)
                {
                    SelectedRecord.Tempo = artirstAndAlbumData.Tempo;
                    SelectedRecord.CoverFile = artirstAndAlbumData.CoverFile;
                    SelectedRecord.Notas = artirstAndAlbumData.Notas;
                }
            }

        }

        private async Task InformUserAndRefreshGrid()
        {
            YouTubePlayerDialogVisibility = false;
            EditMediaDialogVisibility = false;
            YouTubeDialogVisibility = false;
            StateHasChanged();

            await Task.Delay(200);
            await ToastObj.ShowAsync();


            mediaFiles = await GetAllMediaFiles();
            // await gridObj.Refresh();

        }

        protected async Task OpenAddToPlaylistDialog()
        {
            if (!CanAddSelectedMediaToPlaylist)
            {
                ToastTitle = localizerApp["Info"];
                ToastCssClass = "e-toast-warning";
                ToastContent = "Only .mp3 files or URL-based media (e.g., YouTube) can be added to playlists.";
                ToastTimeOut = 3500;

                StateHasChanged();
                await ToastObj.ShowAsync();
                return;
            }

            AddToPlaylistDialogVisibility = true;
            StateHasChanged();
        }

        protected void CloseAddToPlaylistDialog()
        {
            AddToPlaylistDialogVisibility = false;
            StateHasChanged();
        }

        protected async Task HandleMediaAddedToPlaylist(string playlistName)
        {
            AddToPlaylistDialogVisibility = false;

            ToastTitle = localizerApp["Info"];
            ToastCssClass = "e-toast-success";
            ToastContent = $"Added to playlist '{playlistName}'.";
            ToastTimeOut = 2500;

            StateHasChanged();
            await ToastObj.ShowAsync();
        }

        private static bool IsEligibleForPlaylist(MediaVM? media)
        {
            if (media is null)
                return false;

            var fileUrl = media.FileUrl ?? string.Empty;
            var filePath = media.FilePath ?? string.Empty;

            if (fileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return true;

            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return true;

            var ext = Path.GetExtension(filePath);
            return string.Equals(ext, ".mp3", StringComparison.OrdinalIgnoreCase);
        }



        public void Dispose()
        {
            //GC.SuppressFinalize(this);
            ToastObj?.Dispose();
            gridObj?.Dispose();
            SpinnerObj?.Dispose();
            jsMp4Handler?.Dispose();
        }
    }
}
