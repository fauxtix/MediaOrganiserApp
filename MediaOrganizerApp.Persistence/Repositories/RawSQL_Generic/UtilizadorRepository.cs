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

namespace MediaOrganizerApp.Persistence.Repositories.RawSQLRepository
{
    public class UtilizadorRepository : BaseRepository<User_Info>, IUtilizadorRepository
    {
        private readonly IDbConnection sqlConnection;

        public UtilizadorRepository(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateSqlConnection("SqlServerCon");
        }

        public bool CheckUserExist(string userName)
        {
            var query = "SELECT COUNT(1) FROM User_Info where User_Name=@user_Name AND IsActive=@isActive";

            using (IDbConnection connection = sqlConnection)
            {
                bool exists = connection.ExecuteScalar<bool>(query, new { user_Name = userName, isActive = 1 });
                return exists;
            }
        }

        public IDataReader GetData()
        {
            StringBuilder sb = new();
            sb.Append(GetSelectQuery());
            sb.Append("ORDER BY U.First_Name");

            IDbConnection connection = ConnectionManager.GetConnection();

            IDataReader dr = connection.ExecuteReader(sb.ToString());
            return dr;
        }

        public UserVM GetDataVM_ID(int Id)
        {
            StringBuilder sb = new();

            using (var connection = ConnectionManager.GetConnection())
            {
                sb.Append(GetSelectQuery());
                sb.Append($"WHERE U.Id = @Id");

                var result = connection.Query<UserVM>(sb.ToString(), new { Id }).FirstOrDefault();
                return result;
            }
        }

        public User_Info GetData_ID(int Id)
        {
            StringBuilder sb = new();

            using (var connection = ConnectionManager.GetConnection())
            {
                sb.Append(GetSelectQuery());
                sb.Append($"WHERE U.Id = @Id");

                var result = connection.Query<User_Info>(sb.ToString(), new { Id }).FirstOrDefault();
                return result;
            }
        }

        public List<UserVM> GetDataVM_User(string userName)
        {
            StringBuilder sb = new();
            sb.Append(GetSelectQuery());
            if(!string.IsNullOrEmpty(userName))
            {
                sb.Append($"WHERE User_Name = @userName");
            }

            using (IDbConnection connection = sqlConnection)
            {
                var list = connection.Query<UserVM>(sb.ToString(), new { userName }).ToList();
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

            using (IDbConnection connection = sqlConnection)
            {
                var list = connection.Query<User_Info>(sb.ToString(), new { userName }).ToList();
                return list;
            }
        }

        public DateTime? GetLastLoginDate(int userID)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = "SELECT Last_Login_Date FROM User_Info WHERE Id = @userId";

                var result = connection.Query<DateTime?>(sql, new { userID }).FirstOrDefault();
                return result;
            }
        }

        public DateTime? GetLastPasswordChange(int userID)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                string sql = "SELECT Password_Change_Date FROM User_Info WHERE Id = @userId";

                var result = connection.Query<DateTime?>(sql, new { userID }).FirstOrDefault();
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

        public string GetUserName(int userID)
        {
            string Query = "SELECT User_Name FROM User_Info WHERE Id=@userID"; // User_Id = Id ... changed in 2020...

            using (IDbConnection connection = sqlConnection)
            {
                string UName = connection.Query<string>(Query, new { userID }).SingleOrDefault();
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

        public bool IsValidPassword(int Id, string password)
        {
            string query = "SELECT Pwd FROM User_Info WHERE Id=@Id";
            using (var connection = ConnectionManager.GetConnection())
            {
                string pwd = connection.Query<string>(query, new { Id }).FirstOrDefault();
                if (pwd.Trim().Equals(password))
                    return true;
                else
                    return false;
            }
        }

        public bool UpdateLastLoginDate(int Id)
        {
            DateTime dLastLogin = System.DateTime.Now;

            string query = "UPDATE User_Info SET Last_Login_Date=@lastLoginDate WHERE Id=@Id";

            using (IDbConnection connection = sqlConnection)
            {
                connection.Execute(query, new { lastLoginDate = dLastLogin, Id });
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

        public User_Info GetUserProfile(int Id)
        {
            using (var connection = ConnectionManager.GetConnection())
            {
                StringBuilder sb = new();
                sb.Append("SELECT * ");
                sb.Append("FROM User_Info ");
                sb.Append("WHERE Id = @Id");

                string query = sb.ToString();
                var result = connection.Query<User_Info>(query, new { Id }).FirstOrDefault();
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
            string query = "SELECT Descricao from RolesDetail WHERE Id=@Id";

            using (IDbConnection connection = sqlConnection)
            {
                RolesDetail roleDetails = connection.Query<RolesDetail>(query, new { Id }).FirstOrDefault();
                return roleDetails;
            }

        }
        /// <summary>
        /// Returns the UserRole for the specified RoleId
        /// </summary>
        /// <param name="roleId">RoleId</param>
        /// <returns>UserRole</returns>
        public UserRole GetUserRole(int roleId)
        {
            UserRole userRole = new();
            string query = "SELECT Id, Role FROM RolesDetail WHERE Id = @RoleId";

            using (IDbConnection connection = sqlConnection)
            {
                RolesDetail roleDetails = connection.Query<RolesDetail>
                    (query, new { RoleId = roleId })
                    .FirstOrDefault();

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
            StringBuilder sb = new();
            sb.Append("SELECT * FROM User_Info ");

            using (var connection = ConnectionManager.GetConnection())
            {
                List<User_Info> userList = connection.Query<User_Info>(sb.ToString()).ToList();
                return userList;
            }
        }

        private string GetSelectQuery()
        {
            StringBuilder sb = new();
            sb.Append("SELECT U.Id, U.User_Name, U.Pwd, U.RoleId, ");
            sb.Append("R.Descricao, U.Last_Login_Date, U.Password_Change_Date, ");
            sb.Append("U.First_Name, U.EMail, U.Mobile, U.IsActive ");
            sb.Append("FROM User_Info U INNER JOIN ");
            sb.Append("RolesDetail R ON U.RoleId = R.Id ");

            return sb.ToString();
        }

        public bool UserWithPubs(int UserId)
        {
            StringBuilder sb = new();
            sb.Append("SELECT COUNT(1) ");
            sb.Append("from publicacao ");
            sb.Append("WHERE UserId = @UserId");

            using (var connection = ConnectionManager.GetConnection())
            {
               int cnt = connection.Query<int>(sb.ToString(), new { UserId }).SingleOrDefault();
                return cnt > 0;
            }
        }
    }
}
