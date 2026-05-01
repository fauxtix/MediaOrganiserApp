using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface ILogRepository
    {
        Task DeleteAll();
        Task DeleteById(int id);
        Task<IEnumerable<AppLog>> GetAppLogs();
        Task<AppLog> GetAppLog_ById(int Id);
        Task<IEnumerable<LoginLogVM>> GetLogins();
        Task<LoginLogVM> GeUserNameAndEmail(string userId);
        void LogCRUD(LogCRUDModel model);
        void LogLogin(string userId, OpcaoCRUD Operacao, string sessionId);
        Task<IEnumerable<AppLog>> FilterLogins();
        Task DeleteByFilter(IEnumerable<AppLogDto> filteredLogs);
        Task SendEmail(object payload);

    }
}
