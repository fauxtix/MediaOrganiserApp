using MediaOrganizerApp.Application.AppGlobal;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.Security;
using MediaOrganizerApp.Persistence.Repositories.SqlServerRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Web.Blazor.Services.ApiRelated;
using Web.Blazor.Services.Common;
using Web.Blazor.Services.DocumentHelpers;
using Web.Blazor.Services.Recipes;
using Web.Blazor.Services.Security;

namespace MediaOrganizerApp.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.AddSingleton<IGlobalLocalizedStrings, GlobalLocalizedStrings>();

            services.AddHttpClient<IPublicacaoService, PublicacaoService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<ICategoriasService, CategoriasService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<IEditorasService, EditorasService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<IFormatosService, FormatosService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<ILocaisArquivoService, LocaisArquivoService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<IDashboardService, DashboardService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<IMediaService, MediaService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });
            services.AddHttpClient<IPdfService, PdfService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44327/");
            });


            services.AddScoped<IPublicacaoService, PublicacaoService>();
            services.AddScoped<IAutorService, AutorService>();
            services.AddScoped<ICategoriasService, CategoriasService>();
            services.AddScoped<IEditorasService, EditorasService>();
            services.AddScoped<IFormatosService, FormatosService>();
            services.AddScoped<IFormatos_MediaService, Formatos_MediaService>();
            services.AddScoped<ILocaisArquivoService, LocaisArquivoService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddTransient<IUsersService, UsersService>();

            services.AddScoped<ISQLFormsHelper, SQLFormsHelper>();
            services.AddScoped<IUtilitiesService, UtilitiesService>();
            services.AddScoped<IPdfHelper, PdfHelper>();
            services.AddScoped<IDocxHelper, DocxHelper>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IMostUsedService, MostUsedService>();

            services.AddScoped<ISQLFormsHelper, SQLFormsHelper>();
            services.AddScoped<IDataSecurity, DataSecurity>();

            services.AddScoped<IGetYoutubeVideoMetadata, GetYoutubeVideoMetadata>();

            services.AddScoped<ITastyApiRecipesService, TastyApiRecipesService>();

        }
    }
}
