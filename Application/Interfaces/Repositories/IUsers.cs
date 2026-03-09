using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IUsers
    {
        Task<string> GetUserRoleId(string sUserID);
        Task<string> GetUserRoleName(string sUserID);
        Task<string> GetUserRoleName_ByName(string sRoleName);
        Task<string> GetRoleIdByName(string roleName);
    }
}