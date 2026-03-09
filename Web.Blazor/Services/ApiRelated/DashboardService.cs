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

namespace Web.Blazor.Services.ApiRelated
{
    public class DashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AutorService> _logger;
        private string Uri = Endpoints.DashboardEndpoint;

        public DashboardService(HttpClient httpClient, ILogger<AutorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<SummaryDataDTO>> GetSummaryAsync(QueryOptions option)
        {
            var uri = $"{Uri}/{option}";

            try
            {
                var result = await _httpClient
                    .GetFromJsonAsync<IEnumerable<SummaryDataDTO>>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro no Api (Dashboard controller)");
                throw;
            }
        }
    }
}
