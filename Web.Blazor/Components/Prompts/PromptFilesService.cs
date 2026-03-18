
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Components.Prompts
{
    public class PromptFilesService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PromptFilesService> _logger;
        private readonly IConfiguration _env;
        private readonly string _Uri;

        public PromptFilesService(HttpClient http, ILogger<PromptFilesService> logger, IConfiguration env)
        {
            _http = http;
            _logger = logger;
            _env = env;
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/AI";

        }

        public async Task<List<string>> GetPromptFilesAsync()
        {
            var files = await _http.GetFromJsonAsync<List<string>>($"{_Uri}/prompts");
            return files ?? new List<string>();
        }

        public async Task<string> GetPromptFileContentAsync(string fileName)
        {
            try
            {
                return await _http.GetStringAsync($"{_Uri}/prompts/{fileName}");

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<string> GetReadmeContentAsync()
        {
            return await _http.GetStringAsync($"{_Uri}/prompts/readme");
        }

        public async Task SavePromptFileAsync(string fileName, string content)
        {
            var response = await _http.PutAsJsonAsync($"{_Uri}/prompts/{fileName}", content);
            response.EnsureSuccessStatusCode();
        }
    }
}