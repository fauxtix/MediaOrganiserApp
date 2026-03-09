using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Editoras
    {
        Task<List<PublisherReportModel>> GetPublishers();
        Task<List<PublisherReportModel>> GetPublishersWithoutPubs();
    }
}