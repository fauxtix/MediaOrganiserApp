using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System.Data;
using System.Text;

namespace Web.Blazor.Services.User
{
    public class UserAuthentication : IUserAuthentication
    {
        private readonly IDataSecurity _securityProvider;
        private readonly IUtilizadorRepository _userRepo;

        public UserAuthentication(IDataSecurity securityProvider, IUtilizadorRepository usersRepo)
        {
            _securityProvider = securityProvider;
            _userRepo = usersRepo;
        }

        /// <summary>
        /// Autentica��o
        /// </summary>
        /// <param name="userName">Utilizador</param>
        /// <param name="password">Password</param>
        /// <param name="userId">Um par�metro 'out' que devolve o UserId se o login foi bem sucedido</param>
        /// <param name="role">Um par�metro 'out' que devolve o RoleId se o login foi bem sucedido</param>
        /// <returns>true - Se o Utilizador e Password forem v�lidas, sen�o false</returns>
        public bool IsValidUser(string userName, string password,
            out int userId, out AppDefinitions.UserRole role)
        {

            userName = _securityProvider.Encrypt(userName);
            password = _securityProvider.Encrypt(password);


            using IDbConnection connection = ConnectionManager.GetConnection();
            StringBuilder sb = new();
            sb.Append("SELECT * FROM vwUsers ");
            sb.Append("WHERE User_Name=@userName AND Pwd=@password AND IsActive=1");
            string query = sb.ToString();

            var result = connection.Query<UserVM>
                (query, new { userName, password }).FirstOrDefault();

            if (result == null)
            {
                userId = 0;
                role = AppDefinitions.UserRole.GeneralUser;
                return false;
            }
            else
            {
                userId = result.Id;
                role = _userRepo.GetUserRole(result.RoleId);
                return true;
            }
        }
    }
}
