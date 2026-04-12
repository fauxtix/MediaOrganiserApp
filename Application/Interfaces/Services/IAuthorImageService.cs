using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services;


public interface IAuthorImageService
{
    Task<string> GetAuthorImageAsync(string authorName);
}