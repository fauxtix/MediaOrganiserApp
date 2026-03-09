using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.Reporting
{
    public class RDLC_ReportService : IRDLC_ReportService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _env;
        private readonly HttpClient _httpClient;
        private readonly string _Uri;

        public RDLC_ReportService(HttpClient httpClient,
            ILogger<RDLC_ReportService> logger,
            IConfiguration Env)
        {
            _httpClient = httpClient;
            _logger = logger;
            _env = Env;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}";

        }


        public async Task<string> GetPdfLocation(ReportType reportTypeName, int opcao = 0)
        {
            string reportActionName =
                reportTypeName == ReportType.Categories ? "Categories"
                : reportTypeName == ReportType.Formats ? "Formats"
                : reportTypeName == ReportType.Publications ? "Publications"
                : reportTypeName == ReportType.Authors ? $"Authors/{opcao}"
                : reportTypeName == ReportType.Media ? "Media"
                : reportTypeName == ReportType.Publishers ? "Publishers"
                : "StoragePlaces";

            _logger.LogInformation($"Executando report {reportActionName}");
            try
            {
                var pdfLocation = await _httpClient.GetFromJsonAsync<string>($"{_Uri}/report/{reportActionName}");
                return pdfLocation;

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Erro ao pesquisar API ({reportActionName})");
                return string.Empty;
            }
        }

        public async Task<string> GetMediaPdfLocation(Query_TipoMedia reportOption )
        {
            string reportActionName = $"Media/{reportOption}";
            _logger.LogInformation($"Executando report {reportActionName}");
            try
            {
                var pdfLocation = await _httpClient.GetFromJsonAsync<string>($"{_Uri}/report/{reportActionName}");
                return pdfLocation;

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (Autores)");
                return string.Empty;
            }
        }
    }
}
