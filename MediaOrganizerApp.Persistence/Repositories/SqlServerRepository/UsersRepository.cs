using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class UsersRepository : IUsers
    {
        private readonly IDapperContext _context;

        public UsersRepository(IDapperContext context)
        {
            _context = context;
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
                using (var connection = _context.CreateConnection())
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
                using (var connection = _context.CreateConnection())
                {
                    var roleName = await connection.QuerySingleAsync<string>
                        (sb.ToString(), paramCollection);

                    return roleName;

                }
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
                using (var connection = _context.CreateConnection())
                {
                    string output = await connection.QuerySingleAsync<string>
                        (sb.ToString(), paramCollection);
                    return output;

                }
            }
            catch (Exception exc)
            {
                // sem role atribuído

                StringBuilder sbRole = new();
                sbRole.Append("INSERT INTO AspNetUserRoles( ");
                sbRole.Append("UserId, RoleId) ");
                sbRole.Append("VALUES( ");
                sbRole.Append("@UserId, ");
                sbRole.Append("(SELECT Id FROM AspNetRoles WHERE Name = 'User'))");
                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(sbRole.ToString(), paramCollection);
                }

                using (var connection = _context.CreateConnection())
                {
                    string output = await connection.QuerySingleAsync<string>
                        (sb.ToString(), paramCollection);
                    return output;
                }
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
                using (var connection = _context.CreateConnection())
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
