using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IDashboardService
    {
        Task<IEnumerable<SummaryDataDTO>> GetSummaryAsync(QueryEnums.QueryOptions option);
    }
}