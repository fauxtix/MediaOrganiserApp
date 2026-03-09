namespace MediaOrganizerApp.Application.DTOs
{
    public interface IApiConfig
    {
        string ApiKey { get; }
        string BaseUrl { get; }
    }

    public class ApiConfig : IApiConfig
    {
        public string ApiKey { get; }
        public string BaseUrl { get; }

        public ApiConfig(string apiKey, string baseUrl)
        {
            ApiKey = apiKey;
            BaseUrl = baseUrl;
        }
    }
}
