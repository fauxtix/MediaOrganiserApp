using MediaOrganiserApp.Persistence.Repositories.SqlServerRepository;
using MediaOrganizerApp.API.Filters;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.LastFM;
using MediaOrganizerApp.Application.Security;
using MediaOrganizerApp.Persistence.Repositories.SqlServerRepository;
using MediaOrganizerApp.Persistence.Services.Chat;
using MediaOrganizerApp.Persistence.Services.MusicMetadata;
using MediaOrganizerApp.Persistence.Services.Playlists;
using MediaOrganizerApp.Persistence.Services.Youtube;
using Microsoft.Extensions.DependencyInjection;

namespace MediaOrganizerApp.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPublicacoesRepository, PublicacoesRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();


            // playlists
            // repositórios
            services.AddScoped<IPlaylistRepository, PlaylistRepository>();
            services.AddScoped<IPlaylistItemRepository, PlaylistItemRepository>();

            // serviços de domínio
            services.AddScoped<IPlaylistService, PlaylistService>();
            services.AddScoped<IPlaylistItemService, PlaylistItemService>();

            services.AddScoped<IAutoresRepository, AutoresRepository>();
            services.AddScoped<IEditorasRepository, EditorasRepository>();
            services.AddScoped<ICategoriasRepository, CategoriasRepository>();
            services.AddScoped<IFormatos_MediaRepository, Formatos_MediaRepository>();
            services.AddScoped<IFormatosRepository, FormatosRepository>();
            services.AddScoped<IHelpManagerRepository, HelpManagerRepository>();
            services.AddScoped<ILocalArquivoRepository, LocaisArquivoRepository>();
            services.AddScoped<IStatsRepository, StatsRepository>();
            services.AddScoped<ITipoCategoriaRepository, TipoCategoriaRepository>();

            services.AddScoped<IMostUsedRepository, MostUsedRepository>();
            services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
            //services.AddScoped<ILookupTableRepository, LookupTableRepository>();

            services.AddScoped<IFormsHelper, FormsHelper>();
            services.AddScoped<IDataSecurity, DataSecurity>();
            services.AddScoped<IUtilizadorRepository, UtilizadorRepository>();

            services.AddScoped<IUsers, UsersRepository>();
            services.AddScoped<ILogRepository, LogRepository>();

            services.AddScoped<IDapperContext, DapperContext>();

            // Conversations (chat)
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IConversationService, ConversationService>();

            //services.AddSingleton<ILastFmService, LastFmService>();
            services.AddMemoryCache();

        }
    }
}
