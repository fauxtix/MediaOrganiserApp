using Microsoft.AspNetCore.Identity;

namespace MediaOrganizerApp.Domain.Entities
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
