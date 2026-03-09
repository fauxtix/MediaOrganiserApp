using MediaOrganizerApp.Application.Enums;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IReportLists
    {
        Task<object> GetAuthorsList(int opcao);
        Task<object> GetCategoriesList();
        Task<object> GetFormatsList();
        Task<object> GetStoragePlacesList();
        Task<object> GetPublishersList();
        Task<object> GetPublicationsList();
        Task<object> GetMediaFilesList(AppDefinitions.Query_TipoMedia reportOption);        Task<object> GetReportList(string reportName);
    }
}