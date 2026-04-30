using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IHttpApiService
    {
        string GetApiEndpoint(string apiEndpointSetting);
    }

    public class HttpApiService : IHttpApiService
    {
        public IConfiguration Env { get; }

        public HttpApiService(IConfiguration config)
        {
            Env = config;
        }

        public string GetApiEndpoint(string apiEndpointSetting)
        {
            string WebApiDomain = $"{Env["BaseUrl"]}/api";
            string WebApiKey = Env["ApiKey"];

            //string apiEndpointOld = Env[$"API_Endpoints:{apiEndpointSetting}"];
            //string apiEndpoint = $"{WebApiDomain}/{apiEndpointSetting}?ApiKey={WebApiKey}";
            string apiEndpoint = $"{WebApiDomain}/{apiEndpointSetting}";
            return apiEndpoint;
        }
    }
}
