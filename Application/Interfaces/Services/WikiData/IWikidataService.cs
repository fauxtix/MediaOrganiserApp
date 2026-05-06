using MediaOrganizerApp.Application.DTOs.Api.WikiData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.WikiData
{
    public interface IWikidataService
    {
        Task<List<BookResult>> GetBooksByAuthorAsync(string author, string lang);
        Task<(string bio, string photo, string birthDate)> GetWikiDataAuthorDetails(string authorName);
    }
}