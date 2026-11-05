using IF.Lastfm.Core.Api;
using MediaOrganizerApp.Application;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Application.Interfaces.Services.LastFM;
using MediaOrganizerApp.Application.Interfaces.Services.WikiData;
using MediaOrganizerApp.Application.Mappings;
using MediaOrganizerApp.Persistence;
using MediaOrganizerApp.Persistence.Services.MusicMetadata;
using MediaOrganizerApp.Persistence.Services.WikiData;
using MediaOrganizerApp.Persistence.Services.Youtube;
using MediaOrganizerApp.WebApi.Hubs;
using MediaOrganizerApp.WebApi.Middlewares;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.WebApi.Services.CloudChat;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Net;
using System.Net.Mail;

namespace MediaOrganizerApp.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ((IConfigurationBuilder)builder.Configuration).Add(new Configuration.FeatureFlagsConfigurationSource());
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        var connString = builder.Configuration.GetConnectionString("SqlServerCon");
        if (connString is null)
            throw new ArgumentNullException(nameof(connString), "Connection string cannot be null");

        // Configura o ConnectionManager para usar a IConfiguration completa (env vars, appsettings.Production, etc.)
        MediaOrganizerApp.Application.Factories.ConnectionManager.Configure(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader());
        });

        builder.Services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        builder.Services.AddAutoMapper(typeof(AppMappings));
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton<IConnectionFactory>(s => new SqlConnectionFactory(connString));
        var conn = builder.Configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
        builder.Services.AddDbConnectionFactory(conn);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IYouTubeSubscriptionService, YouTubeSubscriptionService>();
        builder.Services.AddPersistenceInfrastructure();

        // Cliente genérico mantido apenas para outras integrações genéricas se necessário
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IPromptTemplateProvider, PromptTemplateProvider>();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<IProgressPublisher, SignalRProgressPublisher>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<Services.IAiSettingsProvider, AiSettingsProvider>();
        builder.Services.AddScoped<ISystemPromptService, SystemPromptService>();

        // ----------------------------------------------------------------
        // CONFIGURAÇÃO RESILIENTE DO POOL DE PORTAS TCP PARA O OLLAMA
        // ----------------------------------------------------------------
        // Base URL configurável (Docker: Ollama__BaseUrl=http://ollama:11434).
        // Sem configuração mantém o Ollama local do host.
        var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";

        builder.Services.AddHttpClient("OllamaSharedClient", client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(10); // Evita o Timeout infinito perigoso
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5), // Recicla sockets de forma invisível
            EnableMultipleHttp2Connections = true,               // Permite múltiplas chamadas em paralelo
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),       // Mantém o "túnel" aberto com o Ollama
            KeepAlivePingTimeout = TimeSpan.FromSeconds(15)
        });

        // Configuração dos Serviços do Ollama utilizando o Client gerido acima
        builder.Services.AddScoped<IAiBookSummaryService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaBookSummaryService>(sp, httpClient);
        });

        builder.Services.AddScoped<IAiBooksService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaBooksService>(sp, httpClient);
        });

        builder.Services.AddScoped<IAiMediaNotesService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaMediaNotesService>(sp, httpClient);
        });

        builder.Services.AddScoped<IAiMediaMetadataService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaMediaMetadataService>(sp, httpClient);
        });

        builder.Services.AddScoped<ITranslateService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaTranslateService>(sp, httpClient);
        });

        builder.Services.AddScoped<IAiChatService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaChatService>(sp, httpClient);
        });

        builder.Services.AddScoped<IOllamaModelsService>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("OllamaSharedClient");
            return ActivatorUtilities.CreateInstance<OllamaModelsService>(sp, httpClient);
        });

        // ----------------------------------------------------------------
        // CLOUD CHAT (OPENROUTER)
        // ----------------------------------------------------------------
        builder.Services.AddHttpClient("OpenRouterChat", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            EnableMultipleHttp2Connections = true,
            KeepAlivePingDelay = TimeSpan.FromSeconds(30),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(15)
        });

        builder.Services.AddHttpClient("OpenRouterCatalog", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddScoped<OpenRouterChatService>();
        builder.Services.AddScoped<OpenRouterCatalogService>();
        builder.Services.AddScoped<CloudAuthorBooksService>();
        builder.Services.AddScoped<CloudMediaService>();
        builder.Services.AddScoped<CloudBookSummaryService>();
        builder.Services.AddScoped<CloudTranslateService>();
        builder.Services.AddScoped<CloudAuthorProfileService>();
        builder.Services.AddScoped<ICloudChatUsageService, CloudChatUsageService>();
        builder.Services.AddScoped<AiUsageRecorderService>();
        builder.Services.AddSingleton<IInternetAvailabilityService, InternetAvailability>();

        // ----------------------------------------------------------------

        builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection("Features"));

        try
        {
            var initial = builder.Configuration.GetSection("Features").Get<FeatureFlags>() ?? new FeatureFlags();
            FeatureFlagsStore.Set(initial);
        }
        catch
        {
            // ignore any errors reading initial flags
        }

        builder.Services.AddScoped<IWikidataService, WikidataService>();
        builder.Services.AddAiServices(builder.Configuration);

        builder.Services.AddHttpClient<IMusicMetadataService, MusicMetadataService>();
        builder.Services.AddHttpClient<IAudioDbService, AudioDbService>();

        var lastFmApiKey = builder.Configuration.GetSection("LastFm")["ApiKey"];
        var lastFmApiSecret = builder.Configuration.GetSection("LastFm")["ApiSecret"];

        builder.Services.AddSingleton<ILastFmService, LastFmService>();
        builder.Services.AddSingleton(new LastfmClient(lastFmApiKey, lastFmApiSecret));

        var gmailSender = builder.Configuration.GetSection("Gmail")["Sender"];
        var gmailPassword = builder.Configuration.GetSection("Gmail")["Password"];
        var smtpServer = builder.Configuration.GetSection("Gmail")["SmtpServer"];
        var gmailPort = Convert.ToInt32(builder.Configuration.GetSection("Gmail")["Port"]);

        builder.Services.AddFluentEmail(gmailSender)
            .AddRazorRenderer()
            .AddSmtpSender(new SmtpClient(smtpServer)
            {
                UseDefaultCredentials = false,
                Port = gmailPort,
                Credentials = new NetworkCredential(gmailSender, gmailPassword),
                EnableSsl = true,
            });

        var app = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Filter.ByExcluding(ExcludeHttpLog)
            .Enrich.FromLogContext()
            .CreateLogger();

        app.Services.GetRequiredService<ILoggerFactory>().AddSerilog();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("MediaOrganizer API")
                    .WithTheme(ScalarTheme.Mars)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }
        else
        {
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                app.Logger.LogError(error, "Unhandled exception in the MediaOrganizer API pipeline.");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    title = "An unexpected error occurred.",
                    status = StatusCodes.Status500InternalServerError
                });
            }));
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors("AllowAllOrigins");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ApiKeyMiddleware>();

        app.MapControllers();

        app.MapHub<MediaInfoProgressHub>("/media-info-progress");
        app.MapHub<GoogleBooksProgressHub>("/books-info-progress");
        app.MapHub<GoogleBooksProgressHub>("/chat-progress");

        app.Run();
    }

    private static bool ExcludeHttpLog(LogEvent logEvent)
    {
        return logEvent.MessageTemplate.Text.Contains("HTTP", StringComparison.OrdinalIgnoreCase)
               && logEvent.MessageTemplate.Text.IndexOf("Error", StringComparison.OrdinalIgnoreCase) < 0;
    }
}