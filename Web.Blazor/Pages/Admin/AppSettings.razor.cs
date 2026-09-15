using MediaOrganizerApp.Application.Utilities;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
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
using Web.Blazor.Services.CloudChat;

namespace Web.Blazor.Pages.Admin
{
    public partial class AppSettings
    {

        [Inject] public IAiModelsService? AIModelsSrvice { get; set; }
        [Inject] public IStringLocalizer<App>? L { get; set; }
        [Inject] public IConfiguration? configuration { get; set; }
        [Inject] public ILogger<App>? logger { get; set; }
        [Inject] public IAppSettingsService? AppSettingsService { get; set; }
        [Inject] IWebHostEnvironment _environment { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        [Inject] IJSRuntime JSRuntime { get; set; }

        [Inject] public CloudChatClientService? CloudChatClient { get; set; }
        [Inject] public CurrentUserIdentity? CurrentUser { get; set; }

        private IEnumerable<AIModelVM>? AiModels { get; set; } = [];
        private ApplicationSettingsDto? settings;
        private List<OpenRouterCatalogModel>? OpenRouterModels;
        private string CloudChatDefaultModel { get; set; } = "openrouter/free";
        private string? CloudChatApiKey;
        private bool CloudChatEnableReasoning;
        private bool CloudChatAllowPaidModels;
        private string CloudChatApiKeyStatus = "NotConfigured";
        private string CloudChatModelFilter = string.Empty;
        private bool CloudChatIsAdmin;
        private CloudChatUsageDto? CloudChatUsage;
        private int? CloudChatFreeDailyCap;
        private bool CloudChatAuthorBooksUseCloud = true;

        private static bool EhModeloPago(string? modelId)
            => !string.IsNullOrWhiteSpace(modelId)
               && !string.Equals(modelId, "openrouter/free", StringComparison.OrdinalIgnoreCase)
               && !modelId.EndsWith(":free", StringComparison.OrdinalIgnoreCase);

        private static bool EhModeloRecomendado(string? modelId)
            => RecommendedExtractionModels.IsRecommended(modelId);

        private bool ModeloSelecionadoEhPago => EhModeloPago(CloudChatDefaultModel);

        private bool ChaveExistente
            => !string.IsNullOrWhiteSpace(CloudChatApiKey) || CloudChatApiKeyStatus == "Configured";

        private bool ChaveSemSuportePago
            => CloudChatUsage is not null
               && CloudChatUsage.ChaveConfigurada
               && !CloudChatUsage.PodeUsarModelosPagos;

        private bool AvisoModeloPagoSemChave
            => ModeloSelecionadoEhPago && (!ChaveExistente || ChaveSemSuportePago);

        private string AvisoModeloPagoMensagem
            => !ChaveExistente
                ? L["CloudChatPaidNoKeyWarning"]
                : L["CloudChatPaidUnsupportedWarning"];

        private SfButton? ToggleBtnObj;
        private string IconCss = "fa fa-play";
        private string Content = "Ssl";

        private string? culture;
        private string selectedCulture = Thread.CurrentThread.CurrentCulture.Name;

        private string portugues;
        protected SfToast? ToastObj { get; set; }

        protected string? ToastTitle;
        protected string? ToastMessage;
        protected string? ToastCss;
        protected string? ToastIcon;

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

            //ApiUrl = _environment.IsProduction()
            //    ? "http://localhost:4110/"
            //    : "https://localhost:44327/";
            CultureInfo culture;

            portugues = L["TituloIdiomaPortugues"]; // TODO set languages
            ToastTitle = "";
            ToastMessage = "";
            ToastCss = "";
            ToastIcon = "";
            //ApiFileOperations = Path.Combine(ApiUrl, "api/FileManager/FileOperations");
            //ApiUploadUrl = Path.Combine(ApiUrl, "api/FileManager/Upload");
            //ApiDownloadUrl = Path.Combine(ApiUrl, "api/FileManager/Download");
            //ApiGetImageUrl = Path.Combine(ApiUrl, "api/FileManager/GetImage");

            try
            {
                AiModels = (await AIModelsSrvice.GetAllAsync()).ToList();
                settings = await AppSettingsService.GetSettingsAsync();
            }
            catch
            {
                settings = new();
            }

            try
            {
                var (_, isAdmin) = CurrentUser is null ? ((string?)null, false) : await CurrentUser.GetAsync();
                CloudChatIsAdmin = isAdmin;

                if (isAdmin && CloudChatClient is not null)
                {
                    OpenRouterModels = await CloudChatClient.GetModelsAsync();
                    var ccSettings = await CloudChatClient.GetSettingsAsync();
                    if (ccSettings is not null)
                    {
                        CloudChatDefaultModel = string.IsNullOrWhiteSpace(ccSettings.DefaultModel)
                            ? "openrouter/free"
                            : ccSettings.DefaultModel;
                        CloudChatEnableReasoning = ccSettings.EnableReasoning;
                        CloudChatAllowPaidModels = ccSettings.AllowPaidModels;
                        CloudChatApiKeyStatus = ccSettings.ApiKeyConfigured ? "Configured" : "NotConfigured";
                        CloudChatFreeDailyCap = ccSettings.FreeDailyCap;
                        CloudChatAuthorBooksUseCloud = ccSettings.AuthorBooksUseCloud;
                        CloudChatUsage = await CloudChatClient.GetUsageAsync();
                    }

                    MontarListaModelosCloud();
                }
            }
            catch
            {
                // ignora: definicoes cloud chat indisponiveis (ex.: WebApi sem cloud-chat)
            }

            selectedCulture = settings.Language;
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
                FileManagerOptions.PdfDownloads => L["DlgPdfDownloads"],
                FileManagerOptions.MediaDownloads => L["DlgMediaDownloads"],
                FileManagerOptions.BackupUploads => L["DlgBackupUploads"],
                FileManagerOptions.BackupBD => L["DlgBackupBD"],
                FileManagerOptions.PublicationsFolder => L["DlgPublicationsFolder"],
                FileManagerOptions.ImagesFolder => L["DlgImagesFolder"],
                FileManagerOptions.ReportsFolder => L["DlgReportsFolder"],
                FileManagerOptions.AuthorImagesFolder => L["DlgAuthorImagesFolder"],
                FileManagerOptions.BookImagesFolder => L["DlgBookImagesFolder"],
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
                    { FileManagerOptions.BookImagesFolder, () => settings.PastaImagensCapasLivros = folderPath },
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
                Content = "No Ssl";
                IconCss = "fa fa-pause";
            }
            else
            {
                settings!.UseSsl = false;
                Content = "Ssl";
                IconCss = "fa fa-play";
            }
        }

        private void MontarListaModelosCloud()
        {
            var saved = CloudChatDefaultModel;
            var lista = new List<OpenRouterCatalogModel>
            {
                new OpenRouterCatalogModel { Id = "openrouter/free", Nome = L["CloudChatAutomaticModel"], EhRouter = true }
            };

            if (OpenRouterModels is not null)
            {
                lista.AddRange(OpenRouterModels.Select(m =>
                    new OpenRouterCatalogModel
                    {
                        Id = m.Id,
                        Nome = string.IsNullOrWhiteSpace(m.Nome) ? m.Id : m.Nome,
                        ContextLength = m.ContextLength,
                        EhRouter = m.EhRouter,
                        EhGratuito = m.EhGratuito,
                        Descricao = m.Descricao,
                        ReleaseDate = m.ReleaseDate,
                        IntelligenceIndex = m.IntelligenceIndex,
                        CodingIndex = m.CodingIndex,
                        AgenticIndex = m.AgenticIndex
                    }));
            }

            if (!string.IsNullOrWhiteSpace(saved) &&
                !string.Equals(saved, "openrouter/free", StringComparison.OrdinalIgnoreCase) &&
                !lista.Any(m => string.Equals(m.Id, saved, StringComparison.OrdinalIgnoreCase)))
            {
                lista.Add(new OpenRouterCatalogModel { Id = saved, Nome = saved });
            }

            OpenRouterModels = lista;
        }

        private IEnumerable<OpenRouterCatalogModel> OpenRouterModelsExibidos
        {
            get
            {
                if (OpenRouterModels is null) return Enumerable.Empty<OpenRouterCatalogModel>();

                var filtro = CloudChatModelFilter?.Trim();
                if (string.IsNullOrEmpty(filtro)) return OpenRouterModels;

                return OpenRouterModels.Where(m =>
                    m.Id.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                    m.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void SelectCloudChatModel(string id)
        {
            CloudChatDefaultModel = id;
        }

        private static string FormatarContextoCloud(int ctx) =>
            ctx >= 100_000 ? $"~{ctx / 1_000}K"
            : ctx >= 1_000 ? $"~{Math.Round(ctx / 1_000d, 1):0.#}K"
            : ctx.ToString(CultureInfo.InvariantCulture);

        private async Task UpdateSettings()
        {
            ToastTitle = L["UpdateSettings"];

            try
            {
                await AppSettingsService.UpdateSettingsAsync(settings!);
                await AppSettingsService.UpdateLanguageAsync(selectedCulture);

                if (CloudChatIsAdmin && CloudChatClient is not null)
                {
                    await CloudChatClient.UpdateSettingsAsync(new CloudChatSettingsUpdateDto
                    {
                        ApiKey = string.IsNullOrWhiteSpace(CloudChatApiKey) ? null : CloudChatApiKey.Trim(),
                        DefaultModel = CloudChatDefaultModel,
                        EnableReasoning = CloudChatEnableReasoning,
                        AllowPaidModels = CloudChatAllowPaidModels,
                        FreeDailyCap = CloudChatFreeDailyCap,
                        AuthorBooksUseCloud = CloudChatAuthorBooksUseCloud
                    });
                }

                if (AvisoModeloPagoSemChave)
                {
                    ToastCss = "e-toast-warning";
                    ToastMessage = $"{AvisoModeloPagoMensagem}";
                    ToastIcon = "fas fa-exclamation-triangle";
                }
                else
                {
                    ToastCss = "e-toast-success";
                    ToastMessage = $"{L["SuccessUpdate"]}";
                    ToastIcon = "fas fa-check";
                }
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
