using MediaOrganizerApp.Application.DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services
{
    public interface IHttpClientApi
    {
        string _apiKey { get; }

        Task<T> GetAsync<T>(string uri);
        Task<T> GetFromJsonAsync<T>(string uri);
    }

    public class HttpClientApi : IHttpClientApi
    {
        private readonly HttpClient _client;

        public string _apiKey { get; }

        public HttpClientApi(HttpClient client, ApiConfig config)
        {
            _client = client;
            _apiKey = config.BaseUrl;
            //client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("ApiKey", _apiKey);
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            HttpResponseMessage response = await _client.GetAsync($"{uri}?apikey={_apiKey}");
            string jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }

        public async Task<T> GetFromJsonAsync<T>(string uri)
        {
            T result = await _client.GetFromJsonAsync<T>(uri);
            return result;
        }

    }
}

