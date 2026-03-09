using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public interface IBO_Utilizadores
    {
        List<UserReportModel> GetUsers();
    }
}