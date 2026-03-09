using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiRelated
{
    public class GetJsonAsyncService<T> : IGetJsonAsyncService<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GetJsonAsyncService<T>> _logger;

        public GetJsonAsyncService(ILogger<GetJsonAsyncService<T>> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<T> SendGetRequestAsync<T>(string uri) where T : class
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _httpClient.
                SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserialising

                try
                {
                    _logger.LogInformation($"Envio de pedido ao API");
                    return await response.Content.ReadFromJsonAsync<T>();
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
