using MediaOrganizerApp.Application.Interfaces.Services.Api;
using System.Text.Json;

namespace Web.Blazor.Services.ApiWrappers
{
    public class GoogleBooksService : IGoogleBooksService
    {
        private readonly ILogger<AutorService> _logger;

        public GoogleBooksService(HttpClient httpClient, ILogger<AutorService> logger, IConfiguration env)
        {
            _logger = logger;
        }

        public async Task<GoogleBook> GetBookAsync(string bookTitle, string author)
        {
            var query = "";

            // Author is required
            var finalAuthorQueryString = "";
            var authorNames = author.Split(" ");
            foreach (var word in authorNames)
            {
                finalAuthorQueryString += word + "+";
            }

            finalAuthorQueryString = finalAuthorQueryString.Remove(finalAuthorQueryString.Length - 1); // remove last '+'

            if (string.IsNullOrEmpty(bookTitle))
            {
                query = $"https://www.googleapis.com/books/v1/volumes?q=inauthor:'{finalAuthorQueryString}'"; // &key={apiKey}";
            }
            else
            {
                var finalTitleQueryString = "";
                var titleNames = bookTitle.Split(" ");
                foreach (var title in titleNames)
                {
                    finalTitleQueryString += title + "+";
                }

                finalTitleQueryString = finalTitleQueryString.Remove(finalTitleQueryString.Length - 1); // remove last '+'

                query = $"https://www.googleapis.com/books/v1/volumes?q=intitle:'{finalTitleQueryString}'+inauthor:''{finalAuthorQueryString}'"; // &key={apiKey}";
            }
            try
            {
                string url = query;
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                using var httpClient = new HttpClient(handler);

                var streamTask = await httpClient.GetStreamAsync(url);

                //var streamTask = await _httpClient.GetStreamAsync(url);
                GoogleBook book = await JsonSerializer.DeserializeAsync<GoogleBook>(streamTask);
                if (book.items is null)
                {
                    return null;
                }
                var items = book.items.Length > 0 ? book.items : null;
                List<string> itemTitles = new();
                foreach (var item in items)
                {
                    itemTitles.Add(item.volumeInfo.title);
                }
                var firstItemWithDescription = book.items.Where(c => c.volumeInfo.description is not null).FirstOrDefault();
                return book;

            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Erro ao pesquisar API (Google books) - {BookTitle} by {Author} => Error: {ErrorMessage}", bookTitle, author, exc.Message);
                return new();
            }
        }

    }
}