using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Web.Blazor.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.QueryEnums;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.ApiWrappers
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DashboardService> _logger;
        private readonly IConfiguration _env;
        private readonly string _uri;

        public DashboardService(HttpClient client,
            ILogger<DashboardService> logger, 
            IConfiguration Env)
        {
            //_httpClientFactory = httpClientFactory;
            _httpClient = client;

            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _uri = $"{_env["BaseUrl"]}/api/dashboard";
        }

        public async Task<IEnumerable<SummaryDataDTO>> GetSummaryAsync(QueryOptions option)
        {
            var uri = $"{_uri}/{option}";

            try
            {
                var result = await _httpClient
                    .GetFromJsonAsync<IEnumerable<SummaryDataDTO>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao aceder ao serviço (Dashboard - GetSummaryAsync)");
                return Enumerable.Empty<SummaryDataDTO>();
            }
        }
    }
}
