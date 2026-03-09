using System.Threading.Tasks;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.Reporting
{
    public interface IRDLC_ReportService
    {
        Task<string> GetPdfLocation(ReportType reportTypeName, int opcao = 0);
        Task<string> GetMediaPdfLocation(Query_TipoMedia reportOption);
    }
}