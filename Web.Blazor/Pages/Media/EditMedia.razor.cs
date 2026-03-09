using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using Web.Blazor.Services.Common;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Web.Blazor.Services.ApiWrappers;

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

        [Inject] public IFormatos_MediaService formatosService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] Microsoft.Extensions.Localization.IStringLocalizer<App> L { get; set; }

        [Inject] IWebHostEnvironment Env { get; set; }

        protected SfUploader sfUploaderCover;
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
        protected string mediaType { get; set; }

        private IEnumerable<FormatoMediaVM> formatList;
        private IEnumerable<CategoryVM> categoryList;

        protected YouTubeVideoDetails VideoDetail = new();

        private bool showAlertMessage = false;
        private string alertMessage = "";

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
                mediaType = isMp3 ? "MP3" : isMp4 ? "MP4" : "";

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
                    mediaType = ""; // mediaFormat;
                }
                else
                {
                    ext = ".mp4";
                    isMp4 = true;
                    mediaType = "MP4";
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
                    mediaType = "Outro";

                }
                else if (mediaFormat.ToLower() == "cd" || mediaFormat.ToLower() == "lp")
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;
                    mediaType = mediaFormat;
                }
                else
                {
                    ext = "";
                    isMp3 = false;
                    isMp4 = false;
                    mediaType = "Outro";
                }
            }

            StateHasChanged();
        }

        protected async Task GetMediaInfo()
        {
            if (string.IsNullOrWhiteSpace(SelectedRecord.FileName)
               || string.IsNullOrWhiteSpace(SelectedRecord.Autor))
            {
                showAlertMessage = true;
                alertMessage = "Por favor preencha os campos Autor e Título";
                return;
            }

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

                if (File.Exists(uploadedCoverFile))
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
    }

}
