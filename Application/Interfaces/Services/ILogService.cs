using MediaOrganizerApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services
{
    public interface ILogService
    {
        Task<IEnumerable<AppLogDto>> GetAppLogs();
        Task<AppLogDto> GetLog_ById(int id);
        Task<IEnumerable<AppLogDto>> ViewLogins();
        Task DeleteAll();
        Task DeleteById(int Id);
        Task DeleteFilteredRecords(IEnumerable<AppLogDto> FilteredRecords);
    }
}
