using System.Text.Json;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Api;

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

        // Return null on transient failure/cancellation; return list (possibly empty) on success
        public async Task<List<GoogleBookInfo>?> GetBooksByAuthorAsync(string author)
        {
            if (string.IsNullOrWhiteSpace(author)) return new List<GoogleBookInfo>();

            try
            {
                var q = $"inauthor:\"{Uri.EscapeDataString(author)}\"";
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
                    string? retryAfter = resp.Headers.TryGetValues("Retry-After", out var ra)
                        ? string.Join(",", ra)
                        : null;

                    string? server = resp.Headers.TryGetValues("Server", out var sv)
                        ? string.Join(",", sv)
                        : null;

                    string? via = resp.Headers.TryGetValues("Via", out var viaHdr)
                        ? string.Join(",", viaHdr)
                        : null;

                    string? cfRay = resp.Headers.TryGetValues("CF-RAY", out var cfr)
                        ? string.Join(",", cfr)
                        : null;

                    string? bodyPreview = null;
                    try
                    {
                        var raw = await resp.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(raw))
                            bodyPreview = raw.Substring(0, Math.Min(raw.Length, 800));
                    }
                    catch (Exception ex)
                    {
                        bodyPreview = $"<failed to read body: {ex.GetType().Name}: {ex.Message}>";
                    }

                    _logger.LogWarning(
                        "Google Books GetBooksByAuthorAsync non-success. Status={StatusCode} Author={Author} Url={Url} RetryAfter={RetryAfter} Server={Server} Via={Via} CF-RAY={CFRay} BodyPreview={BodyPreview}",
                        resp.StatusCode,
                        author,
                        url,
                        retryAfter ?? "<none>",
                        server ?? "<none>",
                        via ?? "<none>",
                        cfRay ?? "<none>",
                        bodyPreview ?? "<empty>");

                    return null;
                }
                try
                {
                    var respObj = await resp.Content.ReadFromJsonAsync<GoogleBooksResponse>();
                    var list = respObj?.items?.Select(i => new GoogleBookInfo
                    {
                        Id = i.id,
                        Title = i.volumeInfo?.title,
                        PublishedDate = i.volumeInfo?.publishedDate,
                        Thumbnail = i.volumeInfo?.imageLinks?.thumbnail ?? i.volumeInfo?.imageLinks?.smallThumbnail
                    }).ToList() ?? new List<GoogleBookInfo>();

                    _logger.LogDebug("Google Books returned {Count} items for {Author}", list.Count, author);
                    return list;
                }
                catch (Exception ex)
                {
                    var raw = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning(ex, "Failed to deserialize Google Books response for {Author}. Raw preview: {Raw}", author, raw?.Substring(0, Math.Min(512, raw?.Length ?? 0)));
                    return null;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw; // re-throw so callers can detect 429 specifically
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GoogleBooks request cancelled for author {Author}", author);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GoogleBooks lookup failed for {Author}", author);
                return null;
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

                // Surface 429 so callers can detect and fallback to AI
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Google Books API rate limited for query {Query}", query);
                    throw new HttpRequestException("Google Books API rate limited", null, resp.StatusCode);
                }

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
        private class Imagelinks { public string? thumbnail { get; set; } public string? smallThumbnail { get; set; } }
    }
}