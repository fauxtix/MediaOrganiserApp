using MediaOrganizerApp.Application.Interfaces.Services.AI;
using MediaOrganizerApp.WebApi.Services;

namespace MediaOrganizerApp.WebApi
{
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers AI-related services for the Web API. Uses AddHttpClient for Ollama-backed implementations
        /// and regular scoped registration for hosted SDK-based implementations.
        /// </summary>
        public static void AddAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            var aiProvider = configuration["Ai:Provider"];
            if (string.Equals(aiProvider, "Ollama", System.StringComparison.OrdinalIgnoreCase))
            {
                // Ollama implementation requires an HttpClient in the constructor; register as a typed client.
                services.AddHttpClient<IAiAuthorProfileService, OllamaAuthorProfileService>();
            }
            else
            {
                // Hosted/OpenAI-style implementation does not use HttpClient in the constructor.
                services.AddScoped<IAiAuthorProfileService, AiAuthorProfileService>();
            }
        }
    }
}
