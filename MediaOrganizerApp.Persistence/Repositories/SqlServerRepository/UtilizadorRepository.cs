using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class UtilizadorRepository :  SQLServerBaseRepository<User_Info>, IUtilizadorRepository
    {
        private readonly IDapperContext _context;

        public UtilizadorRepository(IDapperContext context)
        {
            _context = context;
        }
        public bool CheckUserExist(string User_Name)
        {
            string sProcedName = "usp_User_Info_CheckUserExist";

                using (var connection = _context.CreateConnection())
            {
                bool exists = connection.Query<bool>(sProcedName,
                    new { User_Name }, 
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
                return exists;
            }
        }

        public IDataReader GetData()
        {
            string sProcedName = "usp_User_Info_GetAllData";

            IDataReader dr = _context.CreateConnection().ExecuteReader(sProcedName, 
                commandType: CommandType.StoredProcedure);
            return dr;
        }

        public UserVM GetDataVM_ID(int UserId)
        {
            string sProcedName = "usp_User_Info_GetUsers_ByUserId";

                using (var connection = _context.CreateConnection())
            {
                var result = connection.Query<UserVM>(sProcedName, new { UserId },
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
                return result;
            }
        }

        // ver -- Igual ao método anterior, devolve dados para modelos diferentes... 
        public User_Info GetData_ID(int UserId)
        {
            string sProcedName = "usp_User_Info_GetUsers_ByUserId";

                using (var connection = _context.CreateConnection())
            {
                var result = connection.Query<User_Info>(sProcedName, new { UserId },
                    commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
                return result;
            }
        }

        public List<UserVM> GetDataVM_User(string User_Name)
        {
            StringBuilder sb = new();
            sb.Append(GetSelectQuery());
            if(!string.IsNullOrEmpty(User_Name))
            {
                sb.Append("WHERE User_Name = @userName");
            }

                using (var connection = _context.CreateConnection())
            {
                var list = connection.Query<UserVM>(sb.ToString(), new { User_Name }).ToList();
                return list;
            }
        }


        // este método está duplicado com o de cima...
        public List<User_Info> GetData_User(string userName)
        {
            StringBuilder sb = new();
            sb.Append(GetSelectQuery());
            if (!string.IsNullOrEmpty(userName))
            {
                sb.Append($"WHERE User_Name = @userName");
            }

                using (var connection = _context.CreateConnection())
            {
                var list = connection.Query<User_Info>(sb.ToString(), new { userName }).ToList();
                return list;
            }
        }

        public DateTime? GetLastLoginDate(int UserId)
        {
            string sProcedName = "usp_User_Info_GetLastLoginDate";

                using (var connection = _context.CreateConnection())
            {
                var result = connection.Query<DateTime?>(sProcedName, 
                    new { UserId }, commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
                return result;
            }
        }

        public DateTime? GetLastPasswordChange(int UserId)
        {
            string sProcedName = "usp_User_Info_GetLastPasswordChange";

                using (var connection = _context.CreateConnection())
            {
                var result = connection.Query<DateTime?>(sProcedName,
                    new { UserId }, commandType: CommandType.StoredProcedure)
                    .FirstOrDefault();
                return result;
            }
        }
        public string GetUserFirstLastName(int userID)
        {
            throw new NotImplementedException();
        }

        public string GetUserId(string userName)
        {
            throw new NotImplementedException();
        }

        public string GetUserName(int UserId)
        {
            
           string sProcedName = "usp_User_Info_GetUserName";

                using (var connection = _context.CreateConnection())
            {
                string UName = connection.Query<string>(sProcedName, 
                    new { UserId }, commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
                return UName;
            }
        }


        public bool IsActiveUser(int userID)
        {
            throw new NotImplementedException();
        }

        public bool IsUser_Admin(string sUserName)
        {
            throw new NotImplementedException();
        }

        public bool IsValidPassword(int UserId, string password)
        {
            string sProcedName = "usp_User_Info_IsValidPassword";

                using (var connection = _context.CreateConnection())
            {
                string pwd = connection.Query<string>(sProcedName, new { UserId }, 
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();

                if (pwd.Trim().Equals(password))
                    return true;
                else
                    return false;
            }
        }

        public bool UpdateLastLoginDate(int UserId)
        {
            string sProcedName = "usp_User_Info_UpdateLastLoginDate";
            DateTime LastLoginDate = DateTime.Now;

                using (var connection = _context.CreateConnection())
            {
                connection.Execute(sProcedName, new { LastLoginDate, UserId },
                    commandType: CommandType.StoredProcedure);
                return true;
            }
        }

        public bool UpdateUserDetails(User_Info User)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUserProfile(User_Info userProfile)
        {
            try
            {
                Update(userProfile);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public User_Info GetUserProfile(int UserId)
        {
            string sProcedName = "usp_User_Info_GetUserProfile";

                using (var connection = _context.CreateConnection())
            {
                var result = connection.Query<User_Info>(sProcedName, new { UserId },
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
                return result;
            }
        }

        public bool UpdateUserProfile_Extended(User_Info userProfile)
        {
            try
            {
                Update(userProfile);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public bool UserCanChangeData(string userName)
        {
            throw new NotImplementedException();
        }

        public int UserCount()
        {
            throw new NotImplementedException();
        }


        public RolesDetail GetRoleDetails(int Id)
        {
            string sProcedName = "usp_User_Info_GetRoleDetails";

                using (var connection = _context.CreateConnection())
            {
                RolesDetail roleDetails = connection.Query<RolesDetail>(sProcedName, new { Id }, 
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
                return roleDetails;
            }

        }
        /// <summary>
        /// Returns the UserRole for the specified RoleId
        /// </summary>
        /// <param name="roleId">RoleId</param>
        /// <returns>UserRole</returns>
        public UserRole GetUserRole(int Id)
        {
            string sProcedName = "usp_User_Info_GetRoleDetails";

            UserRole userRole;

                using (var connection = _context.CreateConnection())
            {
                RolesDetail roleDetails = connection.Query<RolesDetail>
                    (sProcedName, new { Id }, commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();

                if (roleDetails != null)
                {
                    string role = roleDetails.Role;
                    userRole = role.ToString().ToUpper() switch
                    {
                        "ADMINISTRATOR" => UserRole.Admin,
                        "USER" => UserRole.GeneralUser,
                        _ => UserRole.GeneralUser,
                    };
                }
                else
                {
                    throw new ApplicationException("Erro ao aceder à tabela 'RoleDetails'");
                }

                return userRole;
            }
        }

        public List<User_Info> GetAll()
        {
            return GetAll();
        }

        private string GetSelectQuery()
        {
            return "SELECT * FROM vwUsers ";
        }

        public bool UserWithPubs(int UserId)
        {
            string sProcedName = "usp_User_Info_UserWithPubs";

                using (var connection = _context.CreateConnection())
            {
                int cnt = connection.Query<int>(sProcedName, new { UserId },
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
                return cnt > 0;
            }
        }
    }
}
