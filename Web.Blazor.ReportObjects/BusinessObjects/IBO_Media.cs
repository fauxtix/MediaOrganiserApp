using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Media
    {
        Task<List<MediaReportModel>> GetMediaFiles(AppDefinitions.Query_TipoMedia reportOption);
    }
}