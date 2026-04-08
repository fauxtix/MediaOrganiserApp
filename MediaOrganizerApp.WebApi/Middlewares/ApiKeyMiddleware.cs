using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace MediaOrganizerApp.WebApi.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "ApiKey"; 
        public ApiKeyMiddleware(RequestDelegate next) 
        { 
            _next = next; 
        }
        public async Task InvokeAsync(HttpContext context) 
        {
            // Allow anonymous access to SignalR hub
            if (context.Request.Path.StartsWithSegments("/media-info-progress"))
            {
                await _next(context);
                return;
            }
            if (context.Request.Path.StartsWithSegments("/books-info-progress"))
            {
                await _next(context);
                return;
            }
            if (context.Request.Path.StartsWithSegments("/chat-progress"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; 
                await context.Response.WriteAsync("Api Key was not provided."); 
                return; 
            } 

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>(APIKEYNAME); 
            if (!apiKey.Equals(extractedApiKey)) 
            { 
                context.Response.StatusCode = 401; 
                await context.Response.WriteAsync("Unauthorized client."); 
                return;
            } 

            await _next(context);
        }
    }
}
