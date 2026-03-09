using MediaOrganizerApp.Application.Enums;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IUserAuthentication
    {
        bool IsValidUser(string userName, string password, out int userId, out AppDefinitions.UserRole role);
    }
}