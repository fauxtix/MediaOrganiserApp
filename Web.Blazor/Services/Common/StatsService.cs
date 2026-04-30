using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Shared.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Blazor.Services.Common
{
    public class StatsService : IStatsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _env;
        private readonly ILogger<StatsService> _logger;


        private string _Uri;

        //private readonly int iCount;
        public StatsService(IStatsRepository statRepo,
            HttpClient client, IConfiguration Env, ILogger<StatsService> logger)
        {
            _httpClient = client;
            _env = Env;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/stats";
        }

        public async Task<int> GetCountFromAuthorsWithoutPubs()
        {
            var uri = $"{_Uri}/GetCountFromAuthorsWithoutPubs";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<int>(uri);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (GetCountFromAuthorsWithoutPubs)");
                return 0;
            }
            //var result = await _clientWrapper.Get("GetCountFromAuthorsWithoutPubs");
            //return int.Parse(result.Contagem);
           
        }

        public async Task<int> GetCountFromEditorasWithoutPubs()
        {
            var uri = $"{_Uri}/GetCountFromEditorasWithoutPubs";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromEditorasWithoutPubs");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromFormatosWithoutPubs()
        {
            var uri = $"{_Uri}/GetCountFromFormatosWithoutPubs";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromFormatosWithoutPubs");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromGenerosWithoutPubs()
        {
            var uri = $"{_Uri}/GetCountFromGenerosWithoutPubs";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromGenerosWithoutPubs");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromLocaisArquivoWithoutPubs()
        {
            var uri = $"{_Uri}/GetCountFromLocaisArquivoWithoutPubs";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromLocaisArquivoWithoutPubs");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromPubsWithoutCover()
        {
            var uri = $"{_Uri}/GetCountFromPubsWithoutCover";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromPubsWithoutCover");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromPubsWithoutPages()
        {
            var uri = $"{_Uri}/GetCountFromPubsWithoutPages";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromPubsWithoutPages");
            //return int.Parse(result.Contagem);
        }

        public async Task<int> GetCountFromTableNotClassified(string sTabela, string sCampoGrupo, string sCampoAux, string sToCheck)
        {
            var uri = $"{_Uri}/GetCountFromTableNotClassified";
            var result = await _httpClient.GetFromJsonAsync<int>(uri);
            return result;

            //var result = await _clientWrapper.Get("GetCountFromTableNotClassified");
            //return int.Parse(result.Contagem);
        }

        public async Task<IEnumerable<StatRecord>> GetStatEntities()
        {

            var uri = $"{_Uri}/GetStatEntities";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<StatRecord>>(uri);
            return result;

            //var result = await _clientWrapper.GetAll("GetStatEntities");
            //return result;
        }

        public async Task<IEnumerable<StatRecord>> GetTitulosDuplicados()
        {
            var uri = $"{_Uri}/GetStatEntities";
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<StatRecord>>(uri);
            return result;

            //var result = await _clientWrapper.GetAll("GetTitulosDuplicados");
            //return result;
        }
    }
}
