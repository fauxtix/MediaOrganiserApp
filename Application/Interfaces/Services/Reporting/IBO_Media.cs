using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Media
    {
        Task<List<MediaReportModel>> GetMediaFiles(AppDefinitions.Query_TipoMedia reportOption);
    }
}