using System;

namespace MediaOrganizerApp.Application.DTOs.Reporting
{
    public class UserReportModel
    {
        public string FirstName { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastChanged { get; set; }
        public string Role { get; set; }
    }
}
