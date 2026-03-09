using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Utilities
{
    public class WebApiExecuter : IWebApiExecuter
    {
        //private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;

        public WebApiExecuter(HttpClient httpClient, IConfiguration Env)
        {
            _httpClient = httpClient;
            //_baseUrl = _httpClient.BaseAddress.AbsoluteUri;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
        }

        public async Task<T> InvokeGet<T>(string uri)
        {
            //await AddTokenHeader();
            return await _httpClient.GetFromJsonAsync<T>(uri);
            //return await _httpClient.GetFromJsonAsync<T>(GetUrl(uri));
        }

        public async Task<T> InvokePost<T>(string uri, T obj)
        {
            //await AddTokenHeader();
            var response = await _httpClient.PostAsJsonAsync(uri, obj);
            await HandleError(response);

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<string> InvokePostReturnString<T>(string uri, T obj)
        {
            //await AddTokenHeader();
            var response = await _httpClient.PostAsJsonAsync(uri, obj);
            await HandleError(response);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task InvokePut<T>(string uri, T obj)
        {
            //await AddTokenHeader();
            var response = await _httpClient.PutAsJsonAsync(uri, obj);
            await HandleError(response);
        }

        public async Task InvokeDelete(string uri)
        {
            //await AddTokenHeader();
            var response = await _httpClient.DeleteAsync(uri);
            await HandleError(response);
        }

        //private string GetUrl(string uri)
        //{
        //    if (_baseUrl.EndsWith("/"))
        //        return $"{_baseUrl}{uri}";
        //    else
        //        return $"{_baseUrl}/{uri}";
        //}

        private async Task HandleError(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }
        }
    }
}
