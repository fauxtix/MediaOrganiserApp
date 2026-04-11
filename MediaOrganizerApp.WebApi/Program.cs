using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Mappings;
using MediaOrganizerApp.Persistence;
using MediaOrganizerApp.Persistence.Services.Youtube;
using MediaOrganizerApp.WebApi.Middlewares;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.Application;
using MediaOrganizerApp.WebApi.Hubs;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using MediaOrganizerApp.Application.Interfaces.Services.AI;

namespace MediaOrganizerApp.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add a configuration provider that reflects runtime feature flags from FeatureFlagsStore
        ((IConfigurationBuilder)builder.Configuration).Add(new MediaOrganizerApp.WebApi.Configuration.FeatureFlagsConfigurationSource());
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }


        // Get connection string
        var connString = builder.Configuration.GetConnectionString("SqlServerCon");
        if (connString is null)
            throw new ArgumentNullException(nameof(connString), "Connection string cannot be null");

        // Configure Services
        builder.Services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        builder.Services.AddAutoMapper(typeof(AppMappings));

        // Add OpenAPI services (required for Scalar)
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton<IConnectionFactory>(s => new SqlConnectionFactory(connString));
        var conn = builder.Configuration.GetSection("ConnectionStrings").Get<Dictionary<string, string>>();
        builder.Services.AddDbConnectionFactory(conn);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IYouTubeSubscriptionService, YouTubeSubscriptionService>();

        // Google Authentication removido - não necessário para esta API

        builder.Services.AddPersistenceInfrastructure();

        // HttpClient used by AI providers (e.g., Ollama local HTTP API).
        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IPromptTemplateProvider, PromptTemplateProvider>();
        builder.Services.AddSignalR();

        // AI services
        builder.Services.AddHttpClient<OllamaBookSummaryService>();
        builder.Services.AddScoped<IAiBookSummaryService, OllamaBookSummaryService>();
        // Ollama-based books provider (LLM fallback for Google Books lookups)
        builder.Services.AddHttpClient<OllamaBooksService>();
        builder.Services.AddScoped<IAiBooksService, OllamaBooksService>();

        // Feature flags: allow runtime toggling of AI fallback/badge
        builder.Services.Configure<MediaOrganizerApp.Application.FeatureFlags>(builder.Configuration.GetSection("Features"));

        // Initialize FeatureFlagsStore from configuration at startup so the in-memory store matches configured defaults
        try
        {
            var initial = builder.Configuration.GetSection("Features").Get<MediaOrganizerApp.Application.FeatureFlags>() ?? new MediaOrganizerApp.Application.FeatureFlags();
            MediaOrganizerApp.WebApi.Services.FeatureFlagsStore.Set(initial);
        }
        catch
        {
            // ignore any errors reading initial flags
        }

        builder.Services.AddHttpClient<OllamaMediaNotesService>();
        builder.Services.AddScoped<IAiMediaNotesService, OllamaMediaNotesService>();

        builder.Services.AddHttpClient<OllamaMediaMetadataService>();
        builder.Services.AddScoped<IAiMediaMetadataService, OllamaMediaMetadataService>();

        // Register a concrete IAiChatService so AiController can be activated.
        // If you later add a real chat implementation, replace DevAiChatService with that class
        builder.Services.AddHttpClient<OllamaChatService>();
        builder.Services.AddScoped<MediaOrganizerApp.Application.Interfaces.Services.AI.IAiChatService, OllamaChatService>();

        // AI fallback services (server-side). Used only when external metadata APIs fail.
        // Provider selection:
        // - Ai:Provider = "OpenAI" (default)
        // - Ai:Provider = "Ollama" (local)
        var aiProvider = builder.Configuration["Ai:Provider"];
        if (string.Equals(aiProvider, "Ollama", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddScoped<IAiAuthorProfileService, OllamaAuthorProfileService>();
        }
        else
        {
            builder.Services.AddScoped<IAiAuthorProfileService, AiAuthorProfileService>();
        }

        var app = builder.Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Filter.ByExcluding(ExcludeHttpLog)
            .Enrich.FromLogContext()
            .CreateLogger();

        app.Services.GetRequiredService<ILoggerFactory>().AddSerilog();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            // Map OpenAPI endpoint
            app.MapOpenApi();

            // Add Scalar UI for interactive API documentation
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("MediaOrganizer API")
                    .WithTheme(ScalarTheme.Mars)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.UseHttpsRedirection();

        // Serve generated PDFs and other static files from wwwroot
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<ApiKeyMiddleware>();

        app.MapControllers();
        app.MapHub<MediaInfoProgressHub>("/media-info-progress"); // Ensure using directive
        app.MapHub<GoogleBooksProgressHub>("/books-info-progress"); // Ensure using directive

        app.Run();
    }

    private static bool ExcludeHttpLog(LogEvent logEvent)
    {
        return logEvent.MessageTemplate.Text.Contains("HTTP", StringComparison.OrdinalIgnoreCase)
               && logEvent.MessageTemplate.Text.IndexOf("Error", StringComparison.OrdinalIgnoreCase) < 0;
    }
}
