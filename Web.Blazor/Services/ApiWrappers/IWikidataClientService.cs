using MediaOrganizerApp.Application.DTOs.Api.WikiData;

namespace Web.Blazor.Services.ApiWrappers
{
    public interface IWikidataClientService
    {
        Task<List<BookResult>> GetBooksByAuthorAsync(string author, string lang = "en");
        Task<(string bio, string photo, string birthDate)> GetWikiDataAuthorDetails(string author);
    }
}