using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers
{
    public class GetJsonAsyncService<T> : IGetJsonAsyncService<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _env;

        private readonly ILogger<GetJsonAsyncService<T>> _logger;

        public GetJsonAsyncService(ILogger<GetJsonAsyncService<T>> logger,
            IHttpClientFactory httpClientFactory, IConfiguration Env)
        {
            _httpClientFactory = httpClientFactory;

            _logger = logger;
            _httpClient = _httpClientFactory.CreateClient();
            _env = Env;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
        }

        public async Task<X> SendGetRequestAsync<X>(string uri) where X : class
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _httpClient.
                SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserialising

                try
                {
                    return await response.Content.ReadFromJsonAsync<X>();
                }
                catch (NotSupportedException nsExc)
                {
                    _logger.LogError(nsExc, "O tipo de conteúdo não é suportado.");
                }
                catch (JsonException jsonExc)
                {
                    _logger.LogError(jsonExc, "Json inválido.");
                }
                catch (Exception exc)
                {
                    _logger.LogError(exc, "Erro ao enviar request.");
                }
            }

            return null;
        }
    }
}
