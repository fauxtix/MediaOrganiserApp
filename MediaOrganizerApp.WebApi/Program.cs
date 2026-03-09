using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Mappings;
using MediaOrganizerApp.Persistence;
using MediaOrganizerApp.Persistence.Services.Youtube;
using MediaOrganizerApp.WebApi.Middlewares;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

namespace MediaOrganizerApp.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
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

        app.Run();
    }

    private static bool ExcludeHttpLog(LogEvent logEvent)
    {
        return logEvent.MessageTemplate.Text.Contains("HTTP", StringComparison.OrdinalIgnoreCase)
               && logEvent.MessageTemplate.Text.IndexOf("Error", StringComparison.OrdinalIgnoreCase) < 0;
    }
}
