using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Web.Blazor.Services.ApiWrappers;

namespace Web.Blazor.Services.Common
{
    public class MostUsedService : IMostUsedService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<MostUsedService> _logger;
        private readonly IConfiguration _env;
        private string _Uri;

        public MostUsedService(HttpClient client,
            ILogger<MostUsedService> logger,
            IConfiguration Env)
        {
            httpClient = client;
            _logger = logger;
            _env = Env;

            httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/mostused";

        }
        public async Task<string> GetOpcaoMaisUsada(string field, string table, string key, string description, string tabelaAUsar = "publicacao")
        {

            QueryMostUsed query = new()
            {
                Campo = field,
                Tabela = table,
                Chave = key,
                Descricao = description, 
                TabelaAusar = tabelaAUsar
            };

            var uri = $"{_Uri}/getmostused/{query}";

            string result;
            using (var response = await httpClient.PostAsJsonAsync(uri, query))
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        public string GetEditoraTop_CategoriaTop()
        {
            return ""; // can we use it? (GetEditoraTop_CategoriaTop)
        }
    }
}
