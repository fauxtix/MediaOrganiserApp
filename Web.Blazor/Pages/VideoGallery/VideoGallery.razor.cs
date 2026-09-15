using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Linq;
using Web.Blazor.Data;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Pages.VideoGallery
{
    public partial class VideoGallery
    {
        [Inject]
        public IMediaServiceWrapper mediaService { get; set; }
        [Inject]
        public IMediaThumbnailsService thumbnailService { get; set; }
        [Inject]
        public ICategoriasService categoriasService { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }
        [Inject]
        IWebHostEnvironment hostingEnvironment { get; set; }
        [Inject] IJSRuntime JS { get; set; }

        protected IEnumerable<VideoCategoryLocations> VideoFiles;
        protected MediaVM SelectedRecord = new MediaVM();
        protected string mediaFilename { get; set; } = "";
        protected string VideoFilesCount { get; set; } = "";
        protected string mediaExtension { get; set; } = "";
        protected string mediaFile { get; set; } = "";
        protected int mediaID { get; set; }

        protected int CatId { get; set; } = 0;

        protected string TitleSearch { get; set; } = string.Empty;

        protected IEnumerable<VideoCategoryLocations> FilteredVideoFiles =>
            string.IsNullOrWhiteSpace(TitleSearch)
                ? (VideoFiles ?? Enumerable.Empty<VideoCategoryLocations>())
                : (VideoFiles ?? Enumerable.Empty<VideoCategoryLocations>())
                    .Where(v => !string.IsNullOrWhiteSpace(v.Autor) &&
                                v.Autor.Contains(TitleSearch, StringComparison.OrdinalIgnoreCase));

        protected string FilteredVideoFilesCount =>
            $"{FilteredVideoFiles.Count()} {localizerApp["Files"]}";

        private List<CategoryVM> categoryList;

        protected bool ErrorVisibility { get; set; } = false;
        protected bool YouTubePlayerDialogVisibility { get; set; } = false;
        protected bool Mp3DialogVisibility { get; set; } = false;
        protected bool ViewDialogVisibility { get; set; } = false;
        protected bool ErrorDialogVisibility { get; set; } = false;
        protected string ErrorMessage { get; set; } = "";

        protected string ErrorTitle { get; set; } = "";
        protected string sErrorMsgs = "";
        protected bool isValidMedia = true;

        public string MediaFilePath { get; set; }

        public DialogEffect Effects = DialogEffect.Fade;
        protected SfSpinner SpinnerObj;
        protected SfToast ToastObj;

        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        protected string target { get; set; } = "#target";

        public bool NoDataVisibility { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            NoDataVisibility = false;

            await GetCategoriesAsync();
            if (categoryList is not null)
            {
                categoryList.Insert(0, new CategoryVM { Id = 0, Descricao = localizerApp["msgAll"] });
                CatId = categoryList.Select(p => p.Id).FirstOrDefault();
                await GetVideosAsync();
            }
            else
            {
                await SpinnerObj?.HideAsync();
                NoDataVisibility = true;
            }
        }

        private async Task GetVideosAsync()
        {
            try
            {
                await Task.Delay(100);
                await SpinnerObj.ShowAsync();
                VideoFiles = (await mediaService.GetVideoLocations(CatId)).ToList() ?? Enumerable.Empty<VideoCategoryLocations>();
                await SpinnerObj.HideAsync();

                if (VideoFiles is null)
                {
                    NoDataVisibility = true;
                    ErrorDialogVisibility = true;
                    ErrorMessage = localizerApp["NoVideosToShow"];
                }
                else
                {
                    if (CatId == 0)
                    {
                        VideoFiles = VideoFiles.OrderBy(p => p.Categoria).ToList();
                    }
                }

                VideoFilesCount = $"{VideoFiles.Count()} {localizerApp["Files"]}";
            }
            catch
            {
                ErrorDialogVisibility = true;
                NoDataVisibility = true;
                ErrorMessage = localizerApp["NoVideosToShow"];
            }
            finally
            {
                await Task.Delay(100);
                await SpinnerObj.HideAsync();
            }
        }

        private async Task GetCategoriesAsync()
        {
            try
            {
                categoryList = (await categoriasService.GetCategoriesWithMediaEntries()).ToList();
            }
            catch
            {
                NoDataVisibility = true;
            }
        }

        private async Task onChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, CategoryVM> args)
        {
            CatId = args.Value;
            await GetVideosAsync();

            StateHasChanged();
        }

        private async Task UpdateCover(int Id)
        {
            var result = await thumbnailService.UpdateMediaCoverRecord(Id);

            ToastTitle = localizerApp["VideoGalleryTitle"];
            ToastCssClass = result.Item2;
            ToastContent = result.Item1;

            StateHasChanged();

            await Task.Delay(200);
            await ToastObj.ShowAsync();

            await GetVideosAsync();
        }

        private async Task PlayVideo(int id)
        {
            mediaID = id;
            SelectedRecord = await mediaService.GetMediaByID(mediaID);

            mediaFile = SelectedRecord.FileUrl;
            if (!string.IsNullOrEmpty(mediaFile))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(mediaFile);
                mediaFilename = fi.Name;
                mediaExtension = ".mp4";
                YouTubePlayerDialogVisibility = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedRecord.FilePath))
                {
                    mediaFile = SelectedRecord.FilePath;
                    System.IO.FileInfo fi = new System.IO.FileInfo(mediaFile);
                    mediaFilename = fi.Name;
                    mediaExtension = fi.Extension.ToLower();
                    if (mediaExtension.ToLower().Contains("3"))
                        Mp3DialogVisibility = true;
                    else
                        YouTubePlayerDialogVisibility = true;
                }
                else
                {
                    Mp3DialogVisibility = false;
                    YouTubePlayerDialogVisibility = false;
                    ErrorVisibility = true;
                    sErrorMsgs = $"{localizerApp["lblTitulo"]} '{SelectedRecord.FileName} ({SelectedRecord.Autor})' {localizerApp["TituloNaoPodeSerReproduzido"]}.";
                    ErrorTitle = $"{localizerApp["TipoFicheiro"]} ({SelectedRecord.FormatoMedia}) {localizerApp["TituloIncompativel"]}";
                    isValidMedia = false;
                }
            }
            StateHasChanged();
        }

        private void CloseMediaPlayer()
        {
            YouTubePlayerDialogVisibility = false;
            SelectedRecord = new MediaVM();
            mediaFilename = "";
        }

        private void CloseMp3Player()
        {
            SelectedRecord = new MediaVM();
            mediaFilename = "";
            Mp3DialogVisibility = false;
        }

        private void CloseErrorDialog()
        {
            ErrorVisibility = false;
        }

        protected void ClearTitleSearch()
        {
            TitleSearch = string.Empty;
        }

        protected async Task ScrollToTop()
        {
            await JS.InvokeVoidAsync(
                "window.scrollTo",
                new { top = 0, behavior = "smooth" });
        }

    }
}