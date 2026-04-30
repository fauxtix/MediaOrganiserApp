using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

public interface IHTTPClientWrapper<T> where T : class
{
    Task<T> Get(string endpoint);
    Task<IEnumerable<T>> GetAll(string endpoint);
    Task<T> PostRequest(T postObject);
    Task PutRequest(T putObject);
}



/// <summary>
/// A generic wrapper class to REST API calls
/// </summary>
/// <typeparam name="T"></typeparam>
public class HTTPClientWrapper<T> : IHTTPClientWrapper<T> where T : class
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public HTTPClientWrapper(IHttpClientFactory clientFactory, IConfiguration config)
    {
        _clientFactory = clientFactory;
        _config = config;
        _apiUrl = $"{_config["BaseUrl"]}/api";
        _httpClient = _clientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("ApiKey", _config["BaseUrl"]);
    }
    /// <summary>
    /// For getting the resources from a web api
    /// </summary>
    /// <param name="url">API Url</param>
    /// <returns>A Task with result object of type T</returns>
    public async Task<T> Get(string endpoint)
    {
        T result = null;

        var response = _httpClient.GetAsync(new Uri($"{_apiUrl}/{endpoint}")).Result;

        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
        {
            if (x.IsFaulted)
                throw x.Exception;

            result = JsonConvert.DeserializeObject<T>(x.Result);
        });

        return result;
    }

    /// <summary>
    /// For getting the resources from a web api
    /// </summary>
    /// <param name="url">API Url</param>
    /// <returns>A Task with result object of type T</returns>
    public async Task<IEnumerable<T>> GetAll(string endpoint)
    {
        IEnumerable<T> result = null;

        var response = _httpClient.GetAsync(new Uri($"{_apiUrl}/{endpoint}")).Result;

        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
        {
            if (x.IsFaulted)
                throw x.Exception;

            result = JsonConvert.DeserializeObject<IEnumerable<T>>(x.Result);
        });

        return result;
    }

    /// <summary>
    /// For creating a new item over a web api using POST
    /// </summary>
    /// <param name="apiUrl">API Url</param>
    /// <param name="postObject">The object to be created</param>
    /// <returns>A Task with created item</returns>
    public async Task<T> PostRequest(T postObject)
    {
        T result = null;

        var response = await _httpClient.PostAsync(_apiUrl, postObject, new JsonMediaTypeFormatter()).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await response.Content.ReadAsStringAsync().ContinueWith((Task<string> x) =>
        {
            if (x.IsFaulted)
                throw x.Exception;

            result = JsonConvert.DeserializeObject<T>(x.Result);

        });

        return result;
    }

    /// <summary>
    /// For updating an existing item over a web api using PUT
    /// </summary>
    /// <param name="apiUrl">API Url</param>
    /// <param name="putObject">The object to be edited</param>
    public async Task PutRequest(T putObject)
    {
        var response = await _httpClient.PutAsync(_apiUrl, putObject, new JsonMediaTypeFormatter()).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }
}