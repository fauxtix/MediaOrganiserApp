using System;

namespace MediaOrganizerApp.Application.DTOs
{
    public class LoginLog
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime LoginDate { get; set; }
        public DateTime LogoutDate { get; set; }
    }
}
