using System.Collections.Generic;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.DTOs;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IGoogleBooksService
    {
        // Query books for an author. Do not expose a caller-controlled maxResults or startIndex;
        // implementations should apply their own defaults and caps.
        Task<List<GoogleBookInfo>> GetBooksByAuthorAsync(string author);

        // Existing method kept for backward compatibility
        Task<GoogleBook?> GetBookAsync(string bookTitle, string author);
    }
}