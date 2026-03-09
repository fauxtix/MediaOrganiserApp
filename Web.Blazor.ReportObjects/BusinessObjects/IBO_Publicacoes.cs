using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Publicacoes
    {
        Task<List<PublicationReportModel>> GetPublications(AppDefinitions.TipoPesquisa iTipo, int idId, string sTitleQuery = "");
        Task<List<PubsWithDescriptions>> GetPublicationsByTitle(string sTitulo);
        Task<List<PublicationReportModel>> GetPublicationsData(AppDefinitions.TipoPesquisa iTipo, int idId);
    }
}