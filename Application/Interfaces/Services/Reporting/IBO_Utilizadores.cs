using MediaOrganizerApp.Application.DTOs.Reporting;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.Interfaces.Services.Reporting
{
    public interface IBO_Utilizadores
    {
        List<UserReportModel> GetUsers();
    }
}