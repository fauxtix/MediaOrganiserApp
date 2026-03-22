using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Globalization;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.Common;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;
using MediaOrganizerApp.Application.Formatting;

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

        [Inject] public AiMediaClientService AiMediaClient { get; set; }

        [Inject] public IFormatos_MediaService formatosService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<EditMedia> Logger { get; set; }

        [Inject] Microsoft.Extensions.Localization.IStringLocalizer<App> L { get; set; }

        [Inject] IWebHostEnvironment Env { get; set; }

        protected SfUploader sfUploaderCover;
        private SfSpinner? spinner;

        public string uploadedCoverFile { get; set; } = "";


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
        private string alertMessage = "";

        private string busyStatusMessage = "";
        private bool isBusy;
        private System.Threading.CancellationTokenSource? mediaInfoCts;

        [Inject] public MediaInfoProgressClient ProgressClient { get; set; }

        protected bool fileVerified;

        protected override async Task OnInitializedAsync()
        {
            formatList = await formatosService.Listar();
            categoryList = await categoriasService.AllAsync(CategoryType.Media); // true = Publicacoes, false = Media
        }

        protected override void OnParametersSet()
        {
            fileVerified = LocalFileOnDisk();
            if (!fileVerified)
            {
                showAlertMessage = true;
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

                            // Remover espaços dentro do número
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
            var shouldAIBeUsed = SelectedRecord.IdFormato_Media > 1 && SelectedRecord.IdFormato_Media < 6;

            await ProgressClient.StartAsync(NavigationManager.BaseUri.TrimEnd('/'));
            ProgressClient.ProgressReceived -= OnProgressReceived;
            ProgressClient.ProgressReceived += OnProgressReceived;
            if (string.IsNullOrWhiteSpace(SelectedRecord.FileName)
               || string.IsNullOrWhiteSpace(SelectedRecord.Autor))
            {
                showAlertMessage = true;
                alertMessage = "Por favor preencha os campos Autor e Título";
                return;
            }

            isBusy = true;
            busyStatusMessage = "";
            mediaInfoCts?.Cancel();
            mediaInfoCts?.Dispose();
            mediaInfoCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(90));

            if (spinner is not null)
                await spinner.ShowAsync();

            showAlertMessage = false;
            alertMessage = "";

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
                        SelectedRecord.AnoEdicao = dvdInfo.Year;
                        anoEdicaoFilled = true;
                    }
                    else
                    {
                        var albumInfo = await MediaService.GetAlbumInfo(artist, mediaTitle);
                        if (albumInfo != null && !string.IsNullOrEmpty(albumInfo.AnoEdicao))
                        {
                            SelectedRecord.AnoEdicao = albumInfo.AnoEdicao;
                            anoEdicaoFilled = true;
                        }
                    }
                }

                // AI fallback for Notas
                if (!notasFilled || (notasFilled && SelectedRecord.Notas == "N/A"))
                {
                    var ai = await AiMediaClient.GetMediaNotesAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId);
                    if (ai == null)
                    {
                        showAlertMessage = true;
                        alertMessage = L["AINotesServiceFailed"] ?? "O serviço de notas AI não devolveu qualquer resultado.";
                    }
                    else
                    {
                        var notesValue = ai.Notes;
                        if (!string.IsNullOrWhiteSpace(notesValue))
                        {
                            SelectedRecord.Notas = notesValue;
                            if (!string.IsNullOrWhiteSpace(ai.Warnings))
                            {
                                showAlertMessage = true;
                                alertMessage = ai.Warnings;
                            }
                        }
                    }
                }

                // AI fallback for AnoEdicao
                if (!anoEdicaoFilled)
                {
                    var year = await AiMediaClient.GetMediaEditionYearAsync(
                        artist,
                        mediaTitle,
                        uiLanguage,
                        ProgressClient.ConnectionId,
                        mediaInfoCts.Token);

                    if(!string.IsNullOrWhiteSpace(year) && year == "N/A")
                    {
                        showAlertMessage = true;
                        alertMessage = "No available Edition year returned from external API/AI; try to find it in other sources.";
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(year) && !DataFormat.IsNumeric(year))
                    {
                        showAlertMessage = true;
                        alertMessage = $"The Edition year returned from the API is not valid {year}";
                        return;

                    }
                    if (!string.IsNullOrWhiteSpace(year))
                        SelectedRecord.AnoEdicao = year;
                }
            }
            catch (OperationCanceledException)
            {
                showAlertMessage = true;
                alertMessage = "Operação cancelada.";
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
                ProgressClient.ProgressReceived -= OnProgressReceived;
                await ProgressClient.StopAsync();

                // Show UI explanation if no notes were retrieved
                if (string.IsNullOrWhiteSpace(SelectedRecord.Notas))
                {
                    showAlertMessage = true;
                    alertMessage = L["NoNotesFound"] ?? "Nenhuma nota foi encontrada para este media.";
                }

                StateHasChanged();
            }
        }

        private void OnProgressReceived(string message)
        {
            busyStatusMessage = message;
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
                int indexPos = SelectedRecord.FileUrl.IndexOf("v=") + 2;
                string videoId = SelectedRecord.FileUrl.Substring(indexPos);
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
                    showAlertMessage = true;
                    alertMessage = "Api não devolveu qualquer resultado";
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
                showAlertMessage = false;
                alertMessage = "";
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
            ToastTitle = "Download Youtube";
            ToastCssClass = "e-toast-info";
            ToastContent = "Operação de gravação cancelada";
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
            showAlertMessage = false;
            alertMessage = "";
            if (SelectedRecord == null || string.IsNullOrWhiteSpace(SelectedRecord.FileName) || string.IsNullOrWhiteSpace(SelectedRecord.Autor))
            {
                showAlertMessage = true;
                alertMessage = "Por favor preencha os campos Autor e Título";
                StateHasChanged();
                return;
            }

            isBusy = true;
            busyStatusMessage = "A obter metadados via AI...";
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

                // Try OMDb first
                var dvdInfo = await MediaService.GetOMDbInfo(mediaTitle, artist);
                if (dvdInfo != null)
                {
                    if (!string.IsNullOrEmpty(dvdInfo.Runtime))
                        SelectedRecord.Tempo = MediaService.ConvertIOMDbTimeStringAsTime(dvdInfo.Runtime);
                    if (!string.IsNullOrEmpty(dvdInfo.Released))
                        SelectedRecord.DataMov = MediaService.ConvertIOMDbStringAsDate(dvdInfo.Released);
                    if (!string.IsNullOrEmpty(dvdInfo.imdbRating))
                        SelectedRecord.Rating = MediaService.ConvertIMDbRatingToFiveScale(dvdInfo.imdbRating);
                    if (!string.IsNullOrEmpty(dvdInfo.Actors))
                        SelectedRecord.Autor = dvdInfo.Actors;
                    if (!string.IsNullOrEmpty(dvdInfo.Year))
                        SelectedRecord.AnoEdicao = dvdInfo.Year;
                    if (!string.IsNullOrEmpty(dvdInfo.Awards))
                        SelectedRecord.Notas = dvdInfo.Awards;
                    if (!string.IsNullOrEmpty(dvdInfo.Poster))
                        SelectedRecord.CoverFile = dvdInfo.Poster;
                    return;
                }

                // Try AlbumInfo next
                var artirstAndAlbumData = await MediaService.GetAlbumInfo(artist, mediaTitle);
                if (artirstAndAlbumData.Titulo != null)
                {
                    if (!string.IsNullOrEmpty(artirstAndAlbumData.Tempo))
                        SelectedRecord.Tempo = artirstAndAlbumData.Tempo;
                    if (!string.IsNullOrEmpty(artirstAndAlbumData.CoverFile))
                        SelectedRecord.CoverFile = artirstAndAlbumData.CoverFile;
                    if (!string.IsNullOrEmpty(artirstAndAlbumData.Notas))
                        SelectedRecord.Notas = artirstAndAlbumData.Notas;
                    return;
                }

                // If both fail, use AI
                var ai = await AiMediaClient.GetMediaMetadataAsync(artist, mediaTitle, uiLanguage, ProgressClient.ConnectionId, mediaInfoCts.Token);
                if (ai is not null)
                {
                    if (!string.IsNullOrEmpty(ai.Tempo))
                        SelectedRecord.Tempo = ai.Tempo;
                    if (!string.IsNullOrEmpty(ai.CoverFile) && ai.CoverFile != "Unknown poster image")
                        SelectedRecord.CoverFile = ai.CoverFile;
                    if (!string.IsNullOrEmpty(ai.Notes))
                        SelectedRecord.Notas = ai.Notes;
                    if (!string.IsNullOrEmpty(ai.Runtime))
                        SelectedRecord.Tempo = ai.Runtime;
                    if (!string.IsNullOrEmpty(ai.Released))
                    {
                        if (DateTime.TryParse(ai.Released, out var releasedDate))
                            SelectedRecord.DataMov = releasedDate;
                    }
                    if (!string.IsNullOrEmpty(ai.ImdbRating))
                    {
                        if (double.TryParse(ai.ImdbRating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rating))
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
                    if (!string.IsNullOrEmpty(ai.Poster) && ai.Poster != "Unknown poster image")
                        SelectedRecord.CoverFile = ai.Poster;

                    var warningMsg = !string.IsNullOrEmpty(ai.Warnings) ? ai.Warnings : ai.Warning;
                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        showAlertMessage = true;
                        alertMessage = warningMsg;
                    }
                }
                else
                {
                    showAlertMessage = true;
                    alertMessage = "AI não devolveu qualquer resultado";
                }
            }
            catch (OperationCanceledException)
            {
                showAlertMessage = true;
                alertMessage = "Operação cancelada.";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching AI media info for {MediaTitle} by {Artist}", SelectedRecord.FileName, SelectedRecord.Autor);
                showAlertMessage = true;
                alertMessage = "Erro ao obter metadados via AI.";
            }
            finally
            {
                if (spinner is not null)
                    await spinner.HideAsync();
                isBusy = false;
                mediaInfoCts?.Dispose();
                mediaInfoCts = null;
                StateHasChanged();
            }
        }
    }
}