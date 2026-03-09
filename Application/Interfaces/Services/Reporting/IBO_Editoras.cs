using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Editoras
    {
        Task<List<PublisherReportModel>> GetPublishers();
        Task<List<PublisherReportModel>> GetPublishersWithoutPubs();
    }
}