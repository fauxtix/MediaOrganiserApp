using MediaOrganizerApp.Domain.Entities;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories;
    public interface IAppSettingsRepository
{
    Task<ApplicationSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(ApplicationSettings settings);
    Task UpdateLanguageAsync(string language);

}