namespace Web.Blazor.Services.ApiWrappers
{
    public class PdfService : IPdfService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration _env;

        private readonly ILogger<PdfService> _logger;
        private string _Uri;

        public PdfService(HttpClient client,
            ILogger<PdfService> logger,
           IConfiguration Env)
        {
            httpClient = client;
            _logger = logger;
            _env = Env;
            httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/pdf";

        }

        public async Task<HttpResponseMessage> DisplayPdfInBrowser(string physicalPath)
        {
            var uri = $"{_Uri}/displaypdf/{physicalPath}";
            try
            {
                using (var content = await httpClient.GetAsync(uri))
                {
                    return content;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Error -Display Pdf In Browser ({exc.Message})");
                return null;
            }
        }
    }
}
