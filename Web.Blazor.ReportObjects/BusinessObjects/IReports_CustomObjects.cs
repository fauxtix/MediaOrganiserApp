using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IReports_CustomObjects
    {
        Task<List<PublicationReportModel>> GetBooks(AppDefinitions.TipoPesquisa iTipo, int idCodigo, string sTitleQuery = "");
        Task<List<PubsWithDescriptions>> GetBooksByTitle(string sTitulo);
        Task<List<PubsWithDescriptions>> GetBooksData(AppDefinitions.TipoPesquisa iTipo, int idCodigo);
    }
}