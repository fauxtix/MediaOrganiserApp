using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Formatos
    {
        Task<List<LookupTableReportModel>> GetFormats();
    }
}