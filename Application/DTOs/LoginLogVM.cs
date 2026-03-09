using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class LoginLogVM
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime LoginDate { get; set; }
        public DateTime? LogoutDate { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
