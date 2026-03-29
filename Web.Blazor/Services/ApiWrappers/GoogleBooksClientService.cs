using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly GoogleBooksService _legacyService;

        public GoogleBooksClientService(HttpClient http, ILogger<GoogleBooksClientService> logger, GoogleBooksService legacyService)
        {
            _http = http;
            _logger = logger;
            _legacyService = legacyService;
        }

        public async Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author, int maxResults = 12, int startIndex = 0)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();
            try
            {
                var q = $"inauthor:\"{Uri.EscapeDataString(author)}\"";
                var url = $"https://www.googleapis.com/books/v1/volumes?q={q}&maxResults={Math.Min(maxResults,40)}&startIndex={startIndex}&fields=items(id,volumeInfo/title,volumeInfo/publishedDate,volumeInfo/imageLinks)";
                var resp = await _http.GetFromJsonAsync<GoogleBooksResponse>(url);
                return resp?.items?.Select(i => new GoogleBookInfo
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
            // Delegate to existing legacy service implementation
            try
            {
                return await _legacyService.GetBookAsync(bookTitle, author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBookAsync fallback");
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
