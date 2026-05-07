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
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; 
                await context.Response.WriteAsync("Api Key was not provided."); 
                return; 
            } 

            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>(APIKEYNAME); 
            if (string.IsNullOrWhiteSpace(apiKey) || !string.Equals(apiKey, extractedApiKey, StringComparison.Ordinal)) 
            { 
                context.Response.StatusCode = 401; 
                await context.Response.WriteAsync("Unauthorized client."); 
                return;
            } 

            await _next(context);
        }
    }
}
