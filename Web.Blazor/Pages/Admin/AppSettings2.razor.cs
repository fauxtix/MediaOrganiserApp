using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.FileManager;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using Web.Blazor.Services.CloudChat;

namespace Web.Blazor.Pages.Admin
{
    public partial class AppSettings2
    {
        [Inject] public IAiModelsService? AIModelsSrvice { get; set; }
        [Inject] public IStringLocalizer<App>? L { get; set; }
        [Inject] public ILogger<App>? logger { get; set; }
        [Inject] public IAppSettingsService? AppSettingsService { get; set; }
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

        private string? culture;
        private string selectedCulture = Thread.CurrentThread.CurrentCulture.Name;

        protected SfToast? ToastObj { get; set; }
        protected string? ToastTitle;
        protected string? ToastMessage;
        protected string? ToastCss;
        protected string? ToastIcon;

        private bool IsFileManagerVisible = false;
        protected DialogEffect Effects = DialogEffect.SlideTop;
        protected string dialogTitle = string.Empty;

        SfFileManager<FileManagerDirectoryContent>? FileManager;
        protected FileManagerOptions fileManagerOption { get; set; }
        protected bool IsDirectoryUpload = true;

        private List<SettingsGroup> SettingsGroups = new();
        private string SelectedNode = "downloads";
        private readonly HashSet<string> ExpandedGroups = new() { "pastas" };

        protected override async Task OnInitializedAsync()
        {
            ToastTitle = "";
            ToastMessage = "";
            ToastCss = "";
            ToastIcon = "";

            try
            {
                AiModels = (await AIModelsSrvice.GetAllAsync()).ToList();
                settings = await AppSettingsService.GetSettingsAsync();
            }
            catch
            {
                settings = new();
            }

            CultureInfo culture;

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

            SettingsGroups = new List<SettingsGroup>
            {
                new("pastas", "TituloPastas", "fa-folder-open", new List<SettingsLeaf>
                {
                    new("downloads", "Downloads", "fa-download"),
                    new("backups", "TituloBackups", "fa-hdd-o"),
                    new("armazenamento", "TituloArmazenamento", "fa-archive"),
                    new("imagens", "lblImagens", "fa-picture-o")
                }),
                new("outras", "TituloOtherSettings", "fa-cog", new List<SettingsLeaf>
                {
                    new("contactos", "TituloContactos", "fa-address-book"),
                    new("definicoes", "TituloOutrasDefinicoes", "fa-sliders"),
                    new("idioma", "TituloDefaultLanguage", "fa-language")
                }),
                new("apis", "TituloApisUrls", "fa-plug", new List<SettingsLeaf>
                {
                    new("webapi", "TituloWebApi", "fa-server"),
                    new("servicos", "TituloServicos", "fa-key"),
                    new("basedados", "TituloBaseDados", "fa-database")
                }),
                new("ia", "TituloGrupoIA", "fa-microchip", new List<SettingsLeaf>
                {
                    new("iacomum", "TituloIAComum", "fa-cog"),
                    new("iaopenrouter", "TituloIAOpenRouter", "fa-cloud"),
                    new("iaollama", "TituloIAOllama", "fa-microchip")
                })
            };
        }

        private void ToggleGroup(string id)
        {
            if (!ExpandedGroups.Remove(id))
            {
                ExpandedGroups.Add(id);
            }
        }

        private void SelectNode(string id)
        {
            SelectedNode = id;
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

        private sealed class SettingsGroup
        {
            public SettingsGroup(string id, string resxKey, string icon, List<SettingsLeaf> leaves)
            {
                Id = id;
                ResxKey = resxKey;
                Icon = icon;
                Leaves = leaves;
            }

            public string Id { get; }
            public string ResxKey { get; }
            public string Icon { get; }
            public List<SettingsLeaf> Leaves { get; }
        }

        private sealed class SettingsLeaf
        {
            public SettingsLeaf(string id, string resxKey, string icon)
            {
                Id = id;
                ResxKey = resxKey;
                Icon = icon;
            }

            public string Id { get; }
            public string ResxKey { get; }
            public string Icon { get; }
        }
    }
}