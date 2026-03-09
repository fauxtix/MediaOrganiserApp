using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Autores
    {
        Task<List<AuthorReportModel>> GetBO_AuthorsPubs();
        List<AuthorReportModel> GetBO_AuthorsBooksAndPubs();
        Task<List<AuthorReportModel>> GetBO_AuthorsWithoutPubs();
    }
}