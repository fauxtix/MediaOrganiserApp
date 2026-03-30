using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using Microsoft.Extensions.Logging;

namespace Web.Blazor.Services.ApiWrappers
{
    public class GoogleBooksClientService : IGoogleBooksService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GoogleBooksClientService> _logger;

        public GoogleBooksClientService(HttpClient http, ILogger<GoogleBooksClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();
            try
            {
                var q = $"inauthor:\"{Uri.EscapeDataString(author)}\"";
                // Use an internal default cap for maxResults to avoid exposing it to callers
                var defaultMax = 20;
                var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&maxResults={defaultMax}&fields=items(id,volumeInfo/title,volumeInfo/publishedDate,volumeInfo/imageLinks)";

                using var resp = await _http.GetAsync(url);

                // Surface rate-limit to callers by throwing so callers can detect 429 specifically
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Google Books API rate limited for author {Author}", author);
                    throw new HttpRequestException("Google Books API rate limited", null, resp.StatusCode);
                }

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google Books API request failed with status {Status} for author {Author}", resp.StatusCode, author);
                    return new List<GoogleBookInfo>();
                }

                var respObj = await resp.Content.ReadFromJsonAsync<GoogleBooksResponse>();
                return respObj?.items?.Select(i => new GoogleBookInfo
                {
                    Id = i.id,
                    Title = i.volumeInfo?.title,
                    PublishedDate = i.volumeInfo?.publishedDate,
                    Thumbnail = i.volumeInfo?.imageLinks?.thumbnail
                }).ToList() ?? new List<GoogleBookInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GoogleBooks lookup failed for {Author}", author);
                return new List<GoogleBookInfo>();
            }
        }

        // Keep original method for backward compatibility used elsewhere in codebase
        public async Task<GoogleBook> GetBookAsync(string bookTitle, string author)
        {
            try
            {
                var query = "";

                // Author is required
                var finalAuthorQueryString = "";
                var authorNames = author?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                foreach (var word in authorNames)
                {
                    finalAuthorQueryString += word + "+";
                }

                if (finalAuthorQueryString.Length > 0)
                    finalAuthorQueryString = finalAuthorQueryString.Remove(finalAuthorQueryString.Length - 1); // remove last '+'

                if (string.IsNullOrEmpty(bookTitle))
                {
                    query = $"https://www.googleapis.com/books/v1/volumes?q=inauthor:'{finalAuthorQueryString}'";
                }
                else
                {
                    var finalTitleQueryString = "";
                    var titleNames = bookTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var title in titleNames)
                    {
                        finalTitleQueryString += title + "+";
                    }

                    if (finalTitleQueryString.Length > 0)
                        finalTitleQueryString = finalTitleQueryString.Remove(finalTitleQueryString.Length - 1);

                    query = $"https://www.googleapis.com/books/v1/volumes?q=intitle:'{finalTitleQueryString}'+inauthor:'{finalAuthorQueryString}'";
                }

                using var resp = await _http.GetAsync(query);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google Books API GetBookAsync failed with status {Status} for query {Query}", resp.StatusCode, query);
                    return new GoogleBook();
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                var book = await JsonSerializer.DeserializeAsync<GoogleBook>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return book ?? new GoogleBook();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error in GetBookAsync");
                return new GoogleBook();
            }
        }

        // Minimal DTOs for deserialization - reuse existing Application DTOs shape
        private class GoogleBooksResponse { public Item[]? items { get; set; } }
        private class Item { public string? id { get; set; } public Volumeinfo? volumeInfo { get; set; } }
        private class Volumeinfo { public string? title { get; set; } public string? publishedDate { get; set; } public Imagelinks? imageLinks { get; set; } }
        private class Imagelinks { public string? thumbnail { get; set; } }
    }
}
