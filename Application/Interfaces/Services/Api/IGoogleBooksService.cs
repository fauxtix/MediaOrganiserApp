using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IGoogleBooksService
    {
        //Task<GoogleBook> GetBookAsync(string bookTitle);
        Task<GoogleBook> GetBookAsync(string bookTitle, string author);
    }
}