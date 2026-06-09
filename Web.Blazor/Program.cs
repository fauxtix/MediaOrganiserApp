using MediaOrganizerApp.Application.AppGlobal;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Mappings;
using MediaOrganizerApp.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using Web.Blazor.Areas.Identity.Data;
using Web.Blazor.Services.DocumentHelpers;
using Web.Blazor.Shared;

using OllamaSharp;
using OpenAI;

namespace Web.Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ----------------------
        // Configure Logging
        // ----------------------
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddEventLog(eventLogSettings => eventLogSettings.SourceName = "OrganizeItLogs");
        builder.Logging.AddDebug();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Filter.ByExcluding(ExcludeHttpLog)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Logging.AddSerilog();

        // ----------------------
        // Connection string
        // ----------------------
        var connString = builder.Configuration.GetConnectionString("SqlServerCon");
        if (connString == null)
            throw new ArgumentNullException(nameof(connString), "Connection string cannot be null");

        // ----------------------
        // Services
        // ----------------------
        builder.Services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));
        builder.Services.AddMemoryCache();
        builder.Services.AddSyncfusionBlazor();
        builder.Services.AddScoped<SfDialogService>();

        //builder.Services.AddCors(options =>
        //{
        //    options.AddPolicy("AllowAllOrigins", policy =>
        //        policy.AllowAnyOrigin()
        //              .AllowAnyMethod()
        //              .AllowAnyHeader());
        //});

        #region Localization
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("en"),
                new CultureInfo("fr"),
                new CultureInfo("pt"),
                new CultureInfo("es"),
            };
            options.DefaultRequestCulture = new RequestCulture("pt");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });
        #endregion

        // ----------------------
        // EF Core + Identity
        // ----------------------
        builder.Services.AddDbContext<PublicationsContext>(options =>
            options.UseSqlServer(connString));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddDefaultUI()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<PublicationsContext>();

        builder.Services.AddRazorPages();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
               .AddInteractiveServerComponents(options =>
               {
                   options.DetailedErrors = true;
               });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddAutoMapper(typeof(AppMappings));
        builder.Services.AddHttpClient();

        // 02-2026

        builder.Services.AddHttpClient("SyncfusionLoopback")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                message.RequestUri != null && message.RequestUri.IsLoopback
        });

        builder.Services.AddSingleton<Services.Common.MediaInfoProgressClient>();
        builder.Services.AddSingleton<Services.Common.BooksInfoProgressClient>();

        builder.Services.AddScoped(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient("SyncfusionLoopback");
        });


        builder.Services.AddSingleton<IGlobalLocalizedStrings, GlobalLocalizedStrings>();

        var sqlConnectionConfiguration = new SqlConnectionConfiguration(connString);
        builder.Services.AddSingleton(sqlConnectionConfiguration);

        var conn = builder.Configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
        builder.Services.AddDbConnectionFactory(conn);
        builder.Services.AddServicesInfrastructure();
        builder.Services.AddPersistenceInfrastructure();
        builder.Services.AddValidators();

        builder.Services.AddScoped<PdfGeneratorService>();
        builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();

        IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), "phi4-mini:latest");

        builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
        var app = builder.Build();

        // ----------------------
        // Syncfusion License
        // ----------------------
        Syncfusion.Licensing.SyncfusionLicenseProvider
             .RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fcXRdR2NfUEJ2WkVWYEg=");

        // ----------------------
        // HTTP Pipeline
        // ----------------------
        app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

        if (app.Environment.IsDevelopment())
        {
            var apiBaseUrl = builder.Configuration["BaseUrl"] ??
                throw new InvalidOperationException("Missing Api:BaseUrl configuration.");

            Static.Endpoints.Initialize(apiBaseUrl + "/api");
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
            app.UseHttpsRedirection(); // no dev, porta automática
        }
        else
        {
            Static.Endpoints.Initialize("http://localhost:4100/api");
            app.UseExceptionHandler("/Error");
            app.UseHsts();

            // Produção: IIS já força HTTPS, mas se quiseres usar middleware:
            // app.UseHttpsRedirection(new HttpsRedirectionOptions { HttpsPort = 4100 });
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowAllOrigins");

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.MapRazorPages();
        app.MapControllers();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        app.Run();
    }

    private static bool ExcludeHttpLog(LogEvent logEvent)
    {
        return logEvent.MessageTemplate.Text.IndexOf("HTTP", StringComparison.OrdinalIgnoreCase) >= 0
               && logEvent.MessageTemplate.Text.IndexOf("Error", StringComparison.OrdinalIgnoreCase) < 0;
    }
}
