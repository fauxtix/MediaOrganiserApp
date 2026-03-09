using MediaOrganizerApp.Application.Enums;

namespace Web.Blazor.Reporting.Helpers.Reporting
{
    public interface IReportService
    {
        byte[] GenerateAuthorsReportAsync(int opcao, string reportType, object reportList = null);
        byte[] GenerateCategoriesReportAsync(string reportType, object reportList = null);
        byte[] GenerateLookupTablesReportAsync(string rdlcFile, string reportType, object reportList = null);
        byte[] GeneratePublishersReportAsync(string reportType, object reportList = null);
        byte[] GeneratePublicationsReportAsync(string reportType, object reportList = null);
        byte[] GenerateMediaFilesReportAsync(AppDefinitions.Query_TipoMedia reportOption, string reportType, object reportList = null);
        byte[] GenerateReportAsync(string reportName, string reportType, string dsName = null, object reportList = null);
    }
}
