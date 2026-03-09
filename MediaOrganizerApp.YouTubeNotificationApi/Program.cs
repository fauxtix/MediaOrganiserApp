using MediaOrganizerApp.YouTubeNotificationApi.Data;
using MediaOrganizerApp.YouTubeNotificationApi.Services;
using MediaOrganizerApp.YouTubeNotificationApi.Settings;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServerCon");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddSingleton<IMyYouTubeService>(provider =>
{
    var userCredentialTask = OAuthService.GetUserCredentialAsync();
    userCredentialTask.Wait();  // Blocking call
    var userCredential = userCredentialTask.Result;

    return new MyYouTubeService(userCredential);
});

builder.Services.AddAuthentication(
        v =>
        {
            v.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
            v.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        }).AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = Keys.ClientId;
            googleOptions.ClientSecret = Keys.ClientSecret;
            googleOptions.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/api/signin-google");
        });

builder.Services.AddHostedService<YouTubeNotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();


// Enable CORS
app.UseCors("AllowAllOrigins");

// Start the application
app.Run();
