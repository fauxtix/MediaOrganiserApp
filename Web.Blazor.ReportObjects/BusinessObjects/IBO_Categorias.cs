using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Categorias
    {
        Task<List<CategoryReportModel>> GetBO_Categories(bool CalledFromPubs);
    }
}