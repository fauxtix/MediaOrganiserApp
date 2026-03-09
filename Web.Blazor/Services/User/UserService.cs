using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services.Api;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using System.Data;
using System.Security.Claims;
using System.Text;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace Web.Blazor.Services.User
{
    public class UserService : IUserService
    {
        private readonly IDataSecurity _securityProvider;
        private readonly IUtilizadorRepository _repoUsers;

        public UserService(IDataSecurity securityProvider, IUtilizadorRepository repoUsers)
        {
            _securityProvider = securityProvider;
            _repoUsers = repoUsers;
        }

        public void DeleteById(int Id)
        {
            _repoUsers.DeleteById(Id);
        }

        public int GetFirstId()
        {
            return _repoUsers.GetFirstId();
        }

        public long Insert(User_Info entity)
        {
            return _repoUsers.Insert(entity);
        }

        public IEnumerable<User_Info> Query(string where = null)
        {
            return _repoUsers.Query(where);
        }

        public User_Info Query_ById(int id)
        {
            return _repoUsers.Query_ById(id);
        }

        public bool TableHasData()
        {
            return _repoUsers.TableHasData();
        }

        public void Update(User_Info entity)
        {
            _repoUsers.Update(entity);
        }

        public List<User_Info> DecryptUsers(IEnumerable<User_Info> users)
        {
            List<User_Info> output = new();
            foreach (var user in users)
            {
                user.Pwd = _securityProvider.Decrypt(user.Pwd);
                user.User_Name = !string.IsNullOrEmpty(user.User_Name) ? _securityProvider.Decrypt(user.User_Name) : "";
                user.EMail = !string.IsNullOrEmpty(user.EMail) ? _securityProvider.Decrypt(user.EMail) : "";
                user.Mobile = !string.IsNullOrEmpty(user.Mobile) ? _securityProvider.Decrypt(user.Mobile) : "";
                output.Add(user);
            }

            return output;
        }

        public User_Info DecryptUser(User_Info user)
        {
            User_Info _user = new()
            {
                Pwd = _securityProvider.Decrypt(user.Pwd),
                User_Name = !string.IsNullOrEmpty(user.User_Name) ? _securityProvider.Decrypt(user.User_Name) : "",
                EMail = !string.IsNullOrEmpty(user.EMail) ? _securityProvider.Decrypt(user.EMail) : "",
                Mobile = !string.IsNullOrEmpty(user.Mobile) ? _securityProvider.Decrypt(user.Mobile) : ""
            };

            return _user;
        }

        public List<User_Info> GetAllUsers()
        {
            return _repoUsers.GetAll();
        }

        public RolesDetail GetRoleDetails(int Id)
        {
            return _repoUsers.GetRoleDetails(Id);
        }

        public UserRole GetUserRole(int Id)
        {
            return _repoUsers.GetUserRole(Id);
        }

        public bool CheckUserExist(string userName)
        {
            string sUserName = _securityProvider.Encrypt(userName);
            return _repoUsers.CheckUserExist(sUserName);
        }

        public User_Info GetUserProfile(int Id)
        {
            return _repoUsers.GetUserProfile(Id);
        }

        public IDataReader GetUsersData(User_Info UserData)
        {
            return _repoUsers.GetData();
            //if (UserData != null)
            //{
            //    if (UserData.User_Name != string.Empty)
            //        sqlCommand.Append(" AND U.User_Name = '" + _securityProvider.Encrypt(UserData.User_Name) + "'");

            //    if (UserData.First_Name != string.Empty)
            //        sqlCommand.Append("AND U.First_Name LIKE '%" + UserData.First_Name + "%' ");

            //    if (UserData.RoleId != -1)
            //        sqlCommand.Append("AND U.RoleId = " + UserData.RoleId.ToString());

            //    if (UserData.EMail != string.Empty)
            //        sqlCommand.Append("AND U.Email = '" + _securityProvider.Encrypt(UserData.EMail) + "'");

            //    if (UserData.Mobile != string.Empty)
            //        sqlCommand.Append("AND U.Mobile = '" + _securityProvider.Encrypt(UserData.Mobile) + "'");
            //}


            //sqlCommand.Append(" ORDER BY U.First_Name");
        }

        public User_Info GetData_ID(int Id)
        {
            return _repoUsers.GetData_ID(Id);
        }

        public UserVM GetDataVM_ID(int Id)
        {
            return _repoUsers.GetDataVM_ID(Id);
        }

        public List<UserVM> GetDataVM_User(string sUserName)
        {
            return _repoUsers.GetDataVM_User(_securityProvider.Encrypt(sUserName));
        }

        public List<User_Info> GetData_User(string sUserName)
        {
            return _repoUsers.GetData_User(_securityProvider.Encrypt(sUserName));
        }


        public bool IsValidPassword(int Id, string password)
        {
            string sPassword = _securityProvider.Encrypt(password);
            return _repoUsers.IsValidPassword(Id, sPassword);
        }

        public bool UpdateLastLoginDate(int Id)
        {
            return _repoUsers.UpdateLastLoginDate(Id);
        }

        public bool UpdateUserProfile(int userID, string userName, string firstName,
            string email, string mobile, int Ativo, int roleId)
        {
            try
            {
                User_Info userProfile = new()
                {
                    Id = userID,
                    User_Name = _securityProvider.Encrypt(userName),
                    First_Name = firstName,
                    EMail = _securityProvider.Encrypt(email),
                    Mobile = _securityProvider.Encrypt(mobile),
                    IsActive = Ativo,
                    RoleId = roleId,
                    Password_Change_Date = DateTime.UtcNow
                };

                _repoUsers.Update(userProfile);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool UpdateUserProfile(int userID, string userName, string firstName,
            string password, string email, string mobile, int Ativo, int roleId)
        {
            try
            {
                DateTime? dLastLogin = _repoUsers.GetLastLoginDate(userID);

                User_Info userProfile = new()
                {
                    Id = userID,
                    User_Name = _securityProvider.Encrypt(userName),
                    First_Name = firstName,
                    Last_Name = "",
                    Pwd = _securityProvider.Encrypt(password),
                    EMail = _securityProvider.Encrypt(email),
                    Mobile = _securityProvider.Encrypt(mobile),
                    IsActive = Ativo,
                    RoleId = roleId,
                    Password_Change_Date = DateTime.UtcNow,
                    Last_Login_Date = dLastLogin
                };

                _repoUsers.UpdateUserProfile_Extended(userProfile);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public string GetRoleName(int Id)
        {
            StringBuilder sbData = new();
            sbData.Clear();
            sbData.Append("SELECT Role ");
            sbData.Append("FROM RolesDetail WHERE Id = @Id");

            using var connection = ConnectionManager.GetConnection();
            string roleName = connection.Query<string>(sbData.ToString(), new { Id }).SingleOrDefault();
            return roleName;
        }

        public DateTime? GetLastLoginDate(int userID)
        {
            return _repoUsers.GetLastLoginDate(userID);
        }

        public DateTime? GetLastPasswordChange(int userID)
        {
            return _repoUsers.GetLastPasswordChange(userID);
        }

        public bool UserWithPubs(int UserId)
        {
            return _repoUsers.UserWithPubs(UserId);
        }

        public async Task<(string UserId, string Role)> GetAuthenticatedUserDetails(AuthenticationStateProvider authenticationStateProvider)
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                return (userId, role);
            }

            return (null, null);
        }
    }
}
