using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Web.Blazor.Services.ApiWrappers;
public class WrapperAppSettings : IAppSettingsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _env;
    private readonly ILogger<WrapperAppSettings> _logger;
    private readonly string? _url;
    private readonly string? _updateLanguageUrl;
    private readonly string? _updateSettingsUrl;
    private readonly string? _getSettingsUrl;
    private readonly HttpClientConfigurationService _httpClientConfigService;

    public WrapperAppSettings(HttpClient httpClient, IConfiguration env,
                                    ILogger<WrapperAppSettings> logger, HttpClientConfigurationService httpClientConfigService)
    {
        _env = env;
        _url = $"{_env["BaseUrl"]}/appsettings";
        _getSettingsUrl = $"{_url}/settings";
        _updateLanguageUrl = $"{_url}/updatelanguage";
        _updateSettingsUrl = $"{_url}/updatesettings";
        _logger = logger;

        _httpClient = httpClient;
        _httpClientConfigService = httpClientConfigService;
        _httpClientConfigService.ConfigureHttpClient(_httpClient);
    }

    public async Task<ApplicationSettingsDto> GetSettingsAsync()
    {
        try
        {
            using (HttpResponseMessage response = await _httpClient.GetAsync(_getSettingsUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var output = JsonConvert.DeserializeObject<ApplicationSettingsDto>(data);
                    if (output != null)
                        return output;
                    else
                        return new ApplicationSettingsDto();
                }
                else
                {
                    return new ApplicationSettingsDto();
                }
            }
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "Erro ao pesquisar API (AppSettings)");
            return new ApplicationSettingsDto();
        }
    }

    public async Task UpdateSettingsAsync(ApplicationSettingsDto settings)
    {
        try
        {
            await _httpClient.PutAsJsonAsync(_updateSettingsUrl, settings);

        }
        catch
        {

            throw;
        }
    }
    public async Task<(string, bool)> UpdateLanguageAsync(string language)
    {
        // _updateLanguageUrl = $"{_env["BaseUrl"]}/appsettings/updatelanguage";
        // controller = AppSettingsController

        try
        {
            var content = new StringContent("\"" + language + "\"", Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(_updateLanguageUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return ("Campo atualizado com sucesso!", true);
            }
            else
            {
                return ("Falha ao atualizar o campo.", false);
            }

        }
        catch (HttpRequestException httpEx)
        {
            return (httpEx.Message, false);
        }
        catch (Exception ex)
        {
            return (ex.Message, false);
        }
    }
}