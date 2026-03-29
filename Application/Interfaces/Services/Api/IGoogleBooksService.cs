using System.Collections.Generic;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IGoogleBooksService
    {
        Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author, int maxResults = 12, int startIndex = 0);

        // Existing method kept for backward compatibility
        Task<GoogleBook> GetBookAsync(string bookTitle, string author);
    }
}