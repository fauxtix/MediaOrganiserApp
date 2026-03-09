using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Categorias
    {
        Task<List<CategoryReportModel>> GetBO_Categories(bool CalledFromPubs);
    }
}