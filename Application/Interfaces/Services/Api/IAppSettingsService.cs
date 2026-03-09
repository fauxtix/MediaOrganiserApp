using MediaOrganizerApp.Application.DTOs;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api;
public interface IAppSettingsService
{
    Task<ApplicationSettingsDto> GetSettingsAsync();
    Task UpdateSettingsAsync(ApplicationSettingsDto settings);
    Task<(string, bool)> UpdateLanguageAsync(string language);
}