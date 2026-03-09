using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.MySqlRepository
{
    public class UsersRepository : IUsers
    {
        private readonly IDbConnection sqlConnection;

        public UsersRepository(IDbConnectionFactory dbFactory)
        {
            sqlConnection = dbFactory.CreateMySqlConnection("MySqlCon");
        }

        public async Task<string> GetRoleIdByName(string roleName)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@RoleName", roleName);

            StringBuilder sb = new();
            sb.Append("SELECT R.Id RoleId ");
            sb.Append("FROM AspNetRoles R ");
            sb.Append("WHERE R.Name = @RoleName");

            try
            {
                using IDbConnection connection = sqlConnection;
                return await connection.QuerySingleAsync<string>
                    (sb.ToString(), paramCollection);
            }
            catch (Exception exc)
            {
                throw new ApplicationException(exc.Message);
            }
        }


        public async Task<string> GetUserRoleName_ByName(string sRoleName)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@RoleName", sRoleName);

            StringBuilder sb = new();
            sb.Append("SELECT DISTINCT R.Id RoleId ");
            sb.Append("FROM AspNetUsers U ");
            sb.Append("INNER JOIN AspNetUserRoles UR ON U.Id = UR.UserId ");
            sb.Append("INNER JOIN AspNetRoles R ON UR.RoleId = R.Id ");
            sb.Append("WHERE R.Name = @RoleName");

            try
            {
                using var connection = sqlConnection;
                var roleName = await connection.QuerySingleAsync<string>
                    (sb.ToString(), paramCollection);

                return roleName;
            }
            catch (Exception exc)
            {
                throw new ApplicationException(exc.Message);
            }

        }

        public async Task<string> GetUserRoleId(string sUserID)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@UserId", sUserID);
            StringBuilder sb = new();
            sb.Append("SELECT a1.Id RoleId ");
            sb.Append("FROM AspNetUsers a ");
            sb.Append("INNER JOIN AspNetUserRoles a0 ON a.Id = a0.UserId ");
            sb.Append("INNER JOIN AspNetRoles a1 ON a0.RoleId = a1.Id ");
            sb.Append("WHERE UserId = @UserId");

            try
            {
                using IDbConnection connection = sqlConnection;
                string output = await connection.QuerySingleAsync<string>
                    (sb.ToString(), paramCollection);
                return output;
            }
            catch (Exception exc)
            {
                throw new ApplicationException(exc.Message);
            }

        }

        public async Task<string> GetUserRoleName(string sUserID)
        {
            DynamicParameters paramCollection = new();
            paramCollection.Add("@UserId", sUserID);
            StringBuilder sb = new();
            sb.Append("SELECT a1.Name RoleName ");
            sb.Append("FROM AspNetUsers a ");
            sb.Append("INNER JOIN AspNetUserRoles a0 ON a.Id = a0.UserId ");
            sb.Append("INNER JOIN AspNetRoles a1 ON a0.RoleId = a1.Id ");
            sb.Append("WHERE UserId = @UserId");

            try
            {
                using IDbConnection connection = sqlConnection;
                return await connection.QuerySingleAsync<string>
                    (sb.ToString(), paramCollection);
            }
            catch (Exception exc)
            {
                throw new ApplicationException(exc.Message);
            }
        }
    }
}
