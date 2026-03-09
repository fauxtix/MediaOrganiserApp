using Web.Blazor.Static;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{
    public class PdfService : IPdfService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<PdfService> _logger;
        private string _Uri = Endpoints.PdfEndpoint;

        public PdfService(HttpClient httpClient, ILogger<PdfService> logger)
        {
            this.httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> DisplayPdfInBrowser(string physicalPath)
        {
            var uri = $"{_Uri}/displaypdf/{physicalPath}";
            _logger.LogInformation("Lendo todas os Autores do API");
            try
            {
                var content = await httpClient.GetAsync(uri);
                return content;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Display Pdf In Browser");
                throw;
            }
        }


    }
}
