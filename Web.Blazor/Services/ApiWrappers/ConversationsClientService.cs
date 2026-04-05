using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Web.Blazor.Services.ApiWrappers
{
    public sealed class ConversationsClientService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _env;

        public ConversationsClientService(HttpClient http, IConfiguration env)
        {
            _http = http;
            _env = env;

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var apiKey = _env["ApiKey"] ?? _env["API_Endpoints:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _http.DefaultRequestHeaders.Add("ApiKey", apiKey);
            }
        }

        private string BaseUrl => _env["BaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("BaseUrl not configured");

        public async Task<List<Conversation>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<List<Conversation>>($"{BaseUrl}/conversations", ct) ?? new List<Conversation>();
            return response;

        }

        public async Task<Conversation?> GetAsync(string id, CancellationToken ct = default) =>
            await _http.GetFromJsonAsync<Conversation>($"{BaseUrl}/conversations/{id}", ct);

        public async Task SaveAsync(Conversation conv, CancellationToken ct = default)
        {
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PutAsJsonAsync(_http, $"{BaseUrl}/conversations/{conv.Id}", conv, ct);
            resp.EnsureSuccessStatusCode();
        }

        public async Task CreateAsync(Conversation conv, CancellationToken ct = default)
        {
            using var resp = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(_http, $"{BaseUrl}/conversations", conv, ct);
            resp.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            using var resp = await _http.DeleteAsync($"{BaseUrl}/conversations/{id}", ct);
            resp.EnsureSuccessStatusCode();
        }
    }
}
