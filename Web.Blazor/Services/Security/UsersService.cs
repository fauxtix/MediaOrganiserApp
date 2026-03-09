using MediaOrganizerApp.Application.Interfaces.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Security
{
    public class UsersService : IUsersService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersService> _logger;
        private readonly IConfiguration _env;

        private readonly string _Uri;

        public UsersService(HttpClient httpClient,
            ILogger<UsersService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _env = Env;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/users";
        }

        public async Task<string> GetRoleIdByName(string roleName)
        {
            string uri = $"{_Uri}/getroleidbyname/{roleName}";
            try
            {

                var response = await _httpClient.GetFromJsonAsync<string>(uri);
                return response ?? string.Empty;

                //var _role = await _httpClient
                //    .GetStringAsync(uri);
                //return _role;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<string> GetUserRoleId(string userId)
        {
            string uri = $"{_Uri}/getuserroleId/{userId}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<string>(uri);
                return response ?? string.Empty;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<string> GetUserRoleName(string userId)
        {
            string uri = $"{_Uri}/getuserrolename/{userId}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<string>(uri);
                return response ?? string.Empty;

                //var _role = await _httpClient.GetStringAsync(uri);
                //return _role;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

        public async Task<string> GetUserRoleName_ByName(string roleName)
        {
            string uri = $"{_Uri}/GetUserRoleName_ByName/{roleName}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<string>(uri);
                return response ?? string.Empty;

                //var _role = await _httpClient
                //    .GetStringAsync(uri);
                //return _role;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API");
                throw;
            }
        }

    }
}
