using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.DTOs.AI.CloudChat;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.CloudChat;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;
using static MediaOrganizerApp.Application.Enums.QueryEnums;

namespace Web.Blazor.Pages.BaseComponents
{
    public class IndexBase : ComponentBase
    {
        [Inject] public IUserService UserService { get; set; }

        [Inject] public IStringLocalizer<App> localizerApp { get; set; }
        [Inject] public IUtilitiesService utils { get; set; }
        [Inject] public IPublicacaoService publicacoesService { get; set; }
        [Inject] public IAutorService autoresService { get; set; }
        [Inject] public ICategoriasService categoriasService { get; set; }
        [Inject] public IFormatosService formatosService { get; set; }
        [Inject] public IFormatos_MediaService formatosMediaService { get; set; }
        [Inject] public ILocaisArquivoService locaisArquivoService { get; set; }
        [Inject] public IEditorasService editorasService { get; set; }
        [Inject] public IDashboardService dashboardService { get; set; }
        [Inject] public IMediaServiceWrapper mediaService { get; set; }
        [Inject] public ConversationsClientService ConversationsClient { get; set; }
        [Inject] public AiBenchmarkClientService AiBenchmarkClient { get; set; }
        [Inject] public AiUsageClientService AiUsageClient { get; set; }
        [Inject] public CloudChatClientService CloudChatClient { get; set; }
        [Inject] public ILogService LogService { get; set; }

        [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; }


        protected IEnumerable<AutorDTO> authorList;
        protected IEnumerable<FormatoPublicacaoVM> formatList;
        protected IEnumerable<LocalArquivoVM> storageList;
        protected IEnumerable<PublisherVM> editorList;

        protected IEnumerable<CategoryVM> categoryList;
        protected IEnumerable<CategoryVM> categoryMediaList;

        protected IEnumerable<PubsWithDescriptions> pubsList;
        protected IEnumerable<MediaVM> mediaList;

        protected IEnumerable<SummaryDataDTO> categoriasSummary;
        protected IEnumerable<SummaryDataDTO> categoriasSummaryTop10;

        protected IEnumerable<SummaryDataDTO> categoriasMediaSummary;
        protected IEnumerable<SummaryDataDTO> categoriasMediaSummaryTop10;

        protected IEnumerable<SummaryDataDTO> autoresSummary;
        protected IEnumerable<SummaryDataDTO> autoresSummaryTop10;

        protected IEnumerable<SummaryDataDTO> editorasSummary;
        protected IEnumerable<SummaryDataDTO> editorasSummaryTop10;

        protected IEnumerable<SummaryDataDTO> formatosSummary;
        protected IEnumerable<SummaryDataDTO> formatosSummaryTop10;

        protected IEnumerable<SummaryDataDTO> formatosMediaSummary;
        protected IEnumerable<SummaryDataDTO> formatosMediaSummaryTop10;

        protected IEnumerable<SummaryDataDTO> storagePlacesSummary;


        protected int categoriasCount;
        protected int categoriasMediaCount;
        protected int autoresCount;
        protected int editorasCount;
        protected int formatosCount;
        protected int formatosMediaCount;

        protected bool IsAdmin;

        protected DashboardHealthDto HealthStats;
        protected AiBenchmarkSummaryDto BenchmarkSummary;
        protected AiUsageSummaryDto AiUsageSummary;
        protected int ConversationsTotal;
        protected CloudChatUsageDto CloudUsage;
        protected ICollection<AppLogDto> RecentErrorLogs = new List<AppLogDto>();
        protected int ErrorLogsCount;
        protected ICollection<AppLogDto> LoginLogs = new List<AppLogDto>();

        protected override async Task OnInitializedAsync()
        {
            //await utils.SetupSqlServerTables();

            string userId;
            string role;
            var userDetails = await UserService.GetAuthenticatedUserDetails(AuthenticationStateProvider);
            userId = userDetails.UserId;
            role = userDetails.Role;

            pubsList = await publicacoesService.GetAllAsync(userId, role);

            var aut = await autoresService.GetAllAsync();
            authorList = aut?.OrderBy(p => p.Descricao).ToList();

            var cat = await categoriasService.GetAllAsync(CategoryType.Publicacoes);
            categoryList = cat?.OrderBy(p => p.Descricao).ToList();

            var catMedia = await categoriasService.GetAllAsync(CategoryType.Media);
            categoryMediaList = catMedia?.OrderBy(p => p.Descricao).ToList();

            var ed = await editorasService.GetAllAsync();
            editorList = ed?.OrderBy(p => p.Descricao).ToList();

            var fmt = await formatosService.GetAllAsync();
            formatList = fmt?.OrderBy(p => p.Descricao).ToList();

            var med = await mediaService.GetAllMedia_Async(userId, role);
            mediaList = med?.OrderBy(p => p.FileName).ToList();

            var loc = await locaisArquivoService.GetAllAsync();
            storageList = loc?.OrderBy(p => p.Descricao).ToList();


            categoriasSummary = await dashboardService.GetSummaryAsync(QueryOptions.Genero);
            categoriasSummaryTop10 = categoriasSummary.Take(10).ToList();
            categoriasMediaSummary = await dashboardService.GetSummaryAsync(QueryOptions.Genero_Media);
            categoriasMediaSummaryTop10 = categoriasMediaSummary.Take(10).ToList();

            autoresSummary = await dashboardService.GetSummaryAsync(QueryOptions.Autor);
            autoresSummaryTop10 = autoresSummary.Take(10).ToList();

            editorasSummary = await dashboardService.GetSummaryAsync(QueryOptions.Editora);
            editorasSummaryTop10 = editorasSummary.Take(10).ToList();

            formatosSummary = await dashboardService.GetSummaryAsync(QueryOptions.Formato);
            formatosSummaryTop10 = formatosSummary.Take(10);

            formatosMediaSummary = await dashboardService.GetSummaryAsync(QueryOptions.Formato_Media);
            formatosMediaSummaryTop10 = formatosMediaSummary.Take(10);

            storagePlacesSummary = await dashboardService.GetSummaryAsync(QueryOptions.Arquivo);

            categoriasCount = cat is not null ? cat.Count() : 0;
            categoriasMediaCount = catMedia is not null ? catMedia.Count() : 0;
            autoresCount = aut is not null ? aut.Count() : 0;
            editorasCount = ed is not null ? ed.Count() : 0;
            formatosCount = fmt is not null ? fmt.Count() : 0;

            var fmtMed = await formatosMediaService.GetAllAsync();
            formatosMediaCount = fmtMed is not null ? fmtMed.Count() : 0;

            IsAdmin = string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase);

            await LoadOutputsAsync();
        }

        private async Task LoadOutputsAsync()
        {
            HealthStats = await dashboardService.GetHealthAsync() ?? new DashboardHealthDto();

            BenchmarkSummary = await AiBenchmarkClient.GetSummaryAsync() ?? new AiBenchmarkSummaryDto();

            AiUsageSummary = await AiUsageClient.GetSummaryAsync() ?? new AiUsageSummaryDto();

            var conversations = await ConversationsClient.GetAllAsync();
            ConversationsTotal = conversations?.Count ?? 0;

            if (IsAdmin)
            {
                CloudUsage = await CloudChatClient.GetUsageAsync();

                var logs = await LogService.GetAppLogs();
                ErrorLogsCount = logs?.Count() ?? 0;
                RecentErrorLogs = logs?
                        .Where(l => string.Equals(l.Level, "Error", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(l.Level, "Fatal", StringComparison.OrdinalIgnoreCase)
                            || !string.IsNullOrWhiteSpace(l.Exception))
                        .OrderByDescending(l => l.TimeStamp)
                        .Take(5)
                        .ToList()
                    ?? new List<AppLogDto>();

                var logins = await LogService.ViewLogins();
                LoginLogs = logins?.OrderByDescending(l => l.TimeStamp).Take(5).ToList() ?? new List<AppLogDto>();
            }
        }

        protected bool IsAppConfigured()
        {
            bool isAppConfigured = true;
            if (authorList is null || formatList is null || storageList is null ||
                editorList is null || categoryList is null)
            {
                isAppConfigured = false;
            }

            return isAppConfigured;
        }
    }
}
