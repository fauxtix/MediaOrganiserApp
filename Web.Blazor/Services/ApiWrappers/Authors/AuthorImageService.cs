using MediaOrganizerApp.Application.Interfaces.Services;
using System.Net.Http;
using System.Text.Json;

public class AuthorImageService : IAuthorImageService
{
    private readonly HttpClient _http;

    public AuthorImageService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string?> GetAuthorImageAsync(string authorName)
    {
        // 1. Try Open Library
        var openLibrary = await GetFromOpenLibrary(authorName);
        if (openLibrary != null)
            return openLibrary;

        // 2. Fallback to Wikidata
        return await GetFromWikidata(authorName);
    }

    // ------------------------
    // Open Library
    // ------------------------
    private async Task<string?> GetFromOpenLibrary(string authorName)
    {
        try
        {
            var url = $"https://openlibrary.org/search/authors.json?q={Uri.EscapeDataString(authorName)}";

            using var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            if (!doc.RootElement.TryGetProperty("docs", out var docs))
                return null;

            foreach (var author in docs.EnumerateArray())
            {
                if (author.TryGetProperty("key", out var keyProp))
                {
                    var key = keyProp.GetString();

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        var lastSlash = key.LastIndexOf('/');
                        var olid = lastSlash >= 0 ? key[(lastSlash + 1)..] : key;

                        if (!string.IsNullOrWhiteSpace(olid))
                        {
                            var imageUrl =  $"https://covers.openlibrary.org/a/olid/{olid}-L.jpg";
                            var headRequest = new HttpRequestMessage(HttpMethod.Head, imageUrl);
                            var imageOk = await _http.SendAsync(headRequest);
                            var realImage = await IsRealImageAsync(imageUrl);
                            if (realImage)
                            {
                                return imageUrl;
                            }
                        }
                    }
                }
            }

            return null;

        }
        catch (Exception ex)
        {

            throw;
        }
    }

    private async Task<bool> IsRealImageAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return false;

        var length = response.Content.Headers.ContentLength;

        // Tune threshold if needed
        return length.HasValue && length > 5000;
    }
    // ------------------------
    // Wikidata (fallback)
    // ------------------------
    private async Task<string?> GetFromWikidata(string authorName)
    {
        var sparqlQuery = $@"
        SELECT ?image WHERE {{
          ?person rdfs:label ""{authorName}""@en.
          ?person wdt:P31 wd:Q5.
          ?person wdt:P18 ?image.
        }} LIMIT 1";

        var url = "https://query.wikidata.org/sparql?format=json&query=" +
                  Uri.EscapeDataString(sparqlQuery);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "AuthorImageApp/1.0");

        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var results = doc.RootElement
            .GetProperty("results")
            .GetProperty("bindings");

        foreach (var item in results.EnumerateArray())
        {
            var imageUrl = item
                .GetProperty("image")
                .GetProperty("value")
                .GetString();

            if (!string.IsNullOrWhiteSpace(imageUrl))
                return imageUrl;
        }

        return null;
    }
}