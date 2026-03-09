using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Formatos
    {
        Task<List<LookupTableReportModel>> GetFormats();
    }
}