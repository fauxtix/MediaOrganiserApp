using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.FileManager;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.SplitButtons;
using System.Globalization;

namespace Web.Blazor.Pages.Admin
{
    public partial class AppSettings2
    {
        [Inject] public IStringLocalizer<App> L { get; set; }
        [Inject] public ILogger<App> logger { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] IWebHostEnvironment _environment { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        [Inject] IJSRuntime JSRuntime { get; set; }

        private ApplicationSettingsDto? settings;
        private SfButton? ToggleBtnObj;

        private string? culture;
        private string selectedCulture = Thread.CurrentThread.CurrentCulture.Name;

        private string portugues;
        protected SfToast ToastObj { get; set; }

        protected string ToastTitle;
        protected string ToastMessage;
        protected string ToastCss;
        protected string ToastIcon;


        private bool IsFileManagerVisible = false;
        protected DialogEffect Effects = DialogEffect.SlideTop;
        protected string dialogTitle = string.Empty;

        SfFileManager<FileManagerDirectoryContent>? FileManager;
        protected string ApiFileOperations { get; set; }
        protected string ApiUrl { get; set; }
        protected string ApiUploadUrl { get; set; }
        protected string ApiDownloadUrl { get; set; }
        protected string ApiGetImageUrl { get; set; }
        protected FileManagerOptions fileManagerOption { get; set; }
        protected bool IsDirectoryUpload = true;

        private Syncfusion.Blazor.AnimationEffect PreviousEffect = Syncfusion.Blazor.AnimationEffect.SlideLeftIn;
        private Syncfusion.Blazor.AnimationEffect NextEffect = Syncfusion.Blazor.AnimationEffect.SlideRightIn;

        protected override async Task OnInitializedAsync()
        {
            ApiUrl = _environment.IsProduction()
                ? "http://localhost:4100/"
                : "https://localhost:44327";

            CultureInfo culture;

            portugues = L["TituloIdiomaPortugues"]; // TODO set languages
            ToastTitle = "";
            ToastMessage = "";
            ToastCss = "";
            ToastIcon = "";
            ApiFileOperations = Path.Combine(ApiUrl, "api/FileManager/FileOperations");
            ApiUploadUrl = Path.Combine(ApiUrl, "api/FileManager/Upload");
            ApiDownloadUrl = Path.Combine(ApiUrl, "api/FileManager/Download");
            ApiGetImageUrl = Path.Combine(ApiUrl, "api/FileManager/GetImage");

            try
            {
                settings = await AppSettingsService.GetSettingsAsync();
            }
            catch
            {
                settings = new();
            }

            selectedCulture = settings.DefaultLanguage;
            if (!string.IsNullOrEmpty(selectedCulture))
            {
                culture = new CultureInfo(selectedCulture);
            }
            else
            {
                culture = new CultureInfo("pt-PT");

            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

        }

        public async void ItemSelected(MenuEventArgs args)
        {
            if (args.Item.Id == "Folder")
            {
                IsDirectoryUpload = true;
            }
            else
            {
                IsDirectoryUpload = false;
            }
            await Task.Delay(100);
            await JSRuntime.InvokeVoidAsync("uploadClick");
        }

        private void OpenFileManager(FileManagerOptions option)
        {
            fileManagerOption = option;
            SetDialogTitle(option);
            IsFileManagerVisible = true;
            StateHasChanged();
        }

        private void SetDialogTitle(FileManagerOptions option)
        {
            dialogTitle = option switch
            {
                FileManagerOptions.PdfDownloads => "Escolha pasta para os downloads de pdf's",
                FileManagerOptions.MediaDownloads => "Escolha pasta para os downloads de ficheiros Média",
                FileManagerOptions.BackupUploads => "Escolha pasta para o backup de ficheiros Média",
                FileManagerOptions.BackupBD => "Escolha pasta para o backup da base de dados",
                FileManagerOptions.PublicationsFolder => "Escolha pasta origem de Publicações",
                FileManagerOptions.ImagesFolder => "Escolha pasta origem das Imagens da aplicação",
                FileManagerOptions.ReportsFolder => "Escolha pasta origem dos Reports",
                FileManagerOptions.AuthorImagesFolder => "Escolha pasta origem das imagens dos Autores",
                _ => string.Empty
            };
        }

        public void ItemClicked(ToolbarClickEventArgs<FileManagerDirectoryContent> args)
        {
            if (args.Item.Text == "Upload")
            {
                args.Cancel = true;
            }
        }

        public void FileSelection(FileSelectionEventArgs<FileManagerDirectoryContent> args)
        {
            var selectedFile = args.FileDetails;
            if (selectedFile != null && !selectedFile.IsFile)
            {
                var folderPath = Path.Combine(selectedFile.FilterPath, selectedFile.Name);

                var optionsMap = new Dictionary<FileManagerOptions, Action>
                {
                    { FileManagerOptions.PdfDownloads, () => settings.PastaDownloadsPdf = folderPath },
                    { FileManagerOptions.MediaDownloads, () => settings.PastaDownloadsMedia = folderPath },
                    { FileManagerOptions.BackupUploads, () => settings.PastaBackupUploads = folderPath },
                    { FileManagerOptions.BackupBD, () => settings.PastaBackupBD = folderPath },
                    { FileManagerOptions.PublicationsFolder, () => settings.PastaPublicacoes = folderPath },
                    { FileManagerOptions.ImagesFolder, () => settings.PastaImagens = folderPath },
                    { FileManagerOptions.ReportsFolder, () => settings.PastaListagens = folderPath },
                    { FileManagerOptions.AuthorImagesFolder, () => settings.PastaImagensAutores = folderPath }
                };

                if (optionsMap.ContainsKey(fileManagerOption))
                {
                    optionsMap[fileManagerOption].Invoke();
                }

                StateHasChanged();
            }
        }

        public void OnToggleClick()
        {
            if (ToggleBtnObj?.Content == "Ssl")
            {
                settings!.UseSsl = true; // Hotmail
            }
            else
            {
                settings!.UseSsl = false;
            }
        }

        private async Task UpdateSettings()
        {
            ToastTitle = "Update settings";

            try
            {
                await AppSettingsService.UpdateSettingsAsync(settings!);
                await AppSettingsService.UpdateLanguageAsync(selectedCulture);
                ToastCss = "e-toast-success";
                ToastMessage = $"{L["SuccessUpdate"]}";
                ToastIcon = "fas fa-check";
                await ShowToastMessage();
            }
            catch (Exception ex)
            {
                ToastCss = "e-toast-danger";
                ToastMessage = $"{L["MSG_ApiError"]}";
                ToastIcon = "fas fa-times";
                await ShowToastMessage();
                logger?.LogError(ex.Message);
            }
        }

        private async void OnSelected(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, CountryModel> e)
        {
            if (e.Value != null)
            {
                culture = e.Value.ToString();
                selectedCulture = culture;

                settings!.DefaultLanguage = culture;

                var uri = new Uri(NavigationManager.Uri)
                    .GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                var query = $"?culture={Uri.EscapeDataString(culture)}&" +
                    $"redirectUri={Uri.EscapeDataString(uri)}";

                var resp = await AppSettingsService!.UpdateLanguageAsync(selectedCulture); // 11/03/2024
                var success = resp.Item2 == true;

                if (success)
                    try
                    {
                        NavigationManager.NavigateTo("/Culture/SetCulture" + query, forceLoad: true);
                    }
                    catch (Exception)
                    {
                    }
            }
        }

        private async Task ShowToastMessage()
        {
            StateHasChanged();
            await Task.Delay(100);
            await ToastObj!.ShowAsync();
        }

        protected async Task HideToast()
        {
            await ToastObj!.HideAsync();
        }

        public List<ToolBarItemModel> Items = new List<ToolBarItemModel>()
    {
        new ToolBarItemModel() { Name = "NewFolder" },
        new ToolBarItemModel() { Name = "Upload" },
        new ToolBarItemModel() { Name = "Cut" },
        new ToolBarItemModel() { Name = "Copy" },
        new ToolBarItemModel() { Name = "Paste" },
        new ToolBarItemModel() { Name = "Delete" },
        new ToolBarItemModel() { Name = "Download" },
        new ToolBarItemModel() { Name = "Rename" },
        new ToolBarItemModel() { Name = "SortBy" },
        new ToolBarItemModel() { Name = "Refresh" },
        new ToolBarItemModel() { Name = "Selection" },
        new ToolBarItemModel() { Name = "View" },
        new ToolBarItemModel() { Name = "Details" },
    };

        public List<CountryModel> CountryData = new List<CountryModel>
        {
            new CountryModel { ID ="pt-PT", Text = "TituloIdiomaPortugues", Pic = "portugal" },
            new CountryModel { ID = "es", Text = "TituloIdiomaEspanhol", Pic = "espanha" },
            new CountryModel { ID ="en-US", Text = "TituloIdiomaIngles", Pic = "american_flag" },
            new CountryModel { ID ="fr", Text = "TituloIdiomaFrances", Pic = "franca" }
        };

        public class CountryModel
        {
            public string ID { get; set; } = string.Empty;
            public string? Text { get; set; }
            public string? Pic { get; set; }
        }

    }
}
