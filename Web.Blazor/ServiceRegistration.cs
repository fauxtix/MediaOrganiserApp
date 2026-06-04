using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Security;
using MediaOrganizerApp.Application.Interfaces.Services.Validators;
using MediaOrganizerApp.Application.Validators;
using Web.Blazor.Components.Prompts;
using Web.Blazor.Data;
using Web.Blazor.Reporting.BusinessObjects;
using Web.Blazor.Reporting.Helpers;
using Web.Blazor.Reporting.Helpers.Reporting;
using Web.Blazor.Services.ApiWrappers;
using Web.Blazor.Services.ApiWrappers.Authors;
using Web.Blazor.Services.Common;
using Web.Blazor.Services.DocumentHelpers;
using Web.Blazor.Services.EmailServices;
using Web.Blazor.Services.Recipes;
using Web.Blazor.Services.Reporting;
using Web.Blazor.Services.Security;
using Web.Blazor.Services.User;
using Web.Blazor.Services.YouTube;


namespace Web.Blazor
{
    public static class ServiceRegistration
    {
        public static void AddServicesInfrastructure(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            //services.AddHttpClient<IWebApiExecuter, WebApiExecuter>();

            services.AddHttpClient<IRDLC_ReportService, RDLC_ReportService>();

            services.AddHttpClient<IDashboardService, DashboardService>();

            services.AddHttpClient<IPublicacaoService, PublicacaoService>();

            services.AddHttpClient<IMediaServiceWrapper, MediaServiceWrapper>();

            // 17-04-2026 - Added LastFmApiClient API client for music metadata retrieval,
            // used in the Author profile page to fetch artist releases.

            services.AddHttpClient<IMusicApiClient, LastFmApiClient>();

            // 02/ 26 / 2024 - Added new services for Playlists, but not using the interface for now,
            // as it's only used in one place and doesn't have any complex logic that would require
            // an interface abstraction at this point.
            // If we find that we need to use it in multiple places or if it becomes more complex,
            // we can easily refactor it to implement an interface later on.
            services.AddHttpClient<PlaylistsClientService>();

            services.AddHttpClient<IAutorService, AutorService>();

            // Translate client for generic text translation
            services.AddHttpClient<TranslateClientService>();

            // AI wrapper used as fallback only when external metadata APIs fail (e.g., author profile).
            services.AddHttpClient<AiClientService>();

            // AI wrapper for publication fallback (e.g., missing/resume/synopsis).
            services.AddHttpClient<AiBookClientService>();

            services.AddHttpClient<IAuthorImageService, AuthorImageService>();

            // AI wrapper for media fallback (e.g., missing awards/liner notes).
            services.AddHttpClient<AiMediaClientService>();

            // YoutubeService para integração com playlists do YouTube
            services.AddHttpClient("YouTube");
            services.AddScoped<Web.Blazor.Services.YouTube.YoutubeService>(sp =>
            {
                var apiKey = Web.Blazor.Static.Keys.YouTubeApiKey;
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var logger = sp.GetRequiredService<ILogger<YoutubeService>>();
                var httpClient = httpClientFactory.CreateClient("YouTube");
                return new Web.Blazor.Services.YouTube.YoutubeService(httpClient, apiKey, logger);
            });

            services.AddHttpClient<ICategoriasService, CategoriasService>();
            services.AddHttpClient<IEditorasService, EditorasService>();
            services.AddHttpClient<IFormatosService, FormatosService>();
            services.AddHttpClient<IFormatos_MediaService, Formatos_MediaService>();
            services.AddHttpClient<ILocaisArquivoService, LocaisArquivoService>();
            services.AddHttpClient<IMostUsedService, MostUsedService>();
            services.AddHttpClient<IUsersService, UsersService>();
            services.AddHttpClient<IPdfService, PdfService>();
            services.AddHttpClient<IMediaThumbnailsService, MediaThumbnailsService>();

            services.AddHttpClient<PromptFilesService>();

            // Conversations client for server-side persisted chat
            services.AddHttpClient<Web.Blazor.Services.ApiWrappers.ConversationsClientService>();

            // Chat-specific SignalR client removed: chat now uses HTTP streaming via AiClientService

            services.AddHttpClient<ILogService, WrapperAppLogs>();

            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IUtilitiesService, UtilitiesService>();
            services.AddScoped<IPdfHelper, PdfHelper>();
            services.AddScoped<IDocxHelper, DocxHelper>();
            services.AddScoped<IOutlookMsgHelper, OutlookMsgHelper>();
            services.AddScoped<IFileService, FileService>();

            services.AddHttpClient<IGoogleBooksService, GoogleBooksClientService>();
            services.AddHttpClient<IWikidataClientService, WikidataClientService>();
            services.AddHttpClient<IAudioDbServiceWrapper, AudioDbServiceWrapper>();

            services.AddScoped<IGetYoutubeVideoMetadata, GetYoutubeVideoMetadata>();

            services.AddScoped<ITastyApiRecipesService, TastyApiRecipesService>();
            services.AddScoped<IAppSettingsService, WrapperAppSettings>();

            services.AddHttpClient<InternetConnectivityService>();

            services.AddScoped<IUserService, UserService>();

            services.AddTransient<HttpClientConfigurationService>();

            // email template service - 05/2026 

            services.AddHttpClient<EmailTemplateService>();


            // Reporting

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportLists, ReportLists>();

            services.AddScoped<IBO_Autores, BO_Autores>();
            services.AddScoped<IBO_Categorias, BO_Categorias>();
            services.AddScoped<IBO_Formatos, BO_Formatos>();
            services.AddScoped<IBO_Editoras, BO_Editoras>();
            services.AddScoped<IBO_LocalArquivo, BO_LocalArquivo>();
            services.AddScoped<IBO_Media, BO_Media>();
            services.AddScoped<IBO_Publicacoes, BO_Publicacoes>();
            services.AddScoped<IBO_Utilizadores, BO_Utilizadores>();
            services.AddScoped<IReports_CustomObjects, Reports_CustomObjects>();

        }

        public static void AddCorsInfrastructure(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
        }

        public static void AddValidators(this IServiceCollection services)
        {
            // Register individual validators
            services.AddTransient<IValidator<AutorDTO>, AutorValidator>();
            services.AddTransient<IValidator<CategoryVM>, CategoriaValidator>();
            services.AddTransient<IValidator<PublisherVM>, EditoraValidator>();
            services.AddTransient<IValidator<FormatoMediaVM>, FormatoMediaValidator>();
            services.AddTransient<IValidator<FormatoPublicacaoVM>, FormatoValidator>();
            services.AddTransient<IValidator<MediaVM>, MediaValidator>();
            services.AddTransient<IValidator<PubsWithDescriptions>, PublicacaoValidator>();

            // Register the generic validator
            services.AddTransient(typeof(IValidator<>), typeof(TabAuxGenericValidator<>));

            // Register the generic validation service
            services.AddScoped<IValidationService, ValidationService>();
        }
    }
}
