using Dapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.DTOs.Reporting;
using MediaOrganizerApp.Application.Factories;
using MediaOrganizerApp.Application.Formatting;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Web.Blazor.Reporting.BusinessObjects
{
    public class BO_Utilizadores : IBO_Utilizadores
    {
        IUtilizadorRepository _repoUsers;
        IDataSecurity _securityProvider;

    

        public BO_Utilizadores(IUtilizadorRepository repoUsers, IDataSecurity securityProvider)
        {
            _repoUsers = repoUsers;
            _securityProvider = securityProvider;
        }

        public List<UserReportModel> GetUsers()
        {
            List<UserVM> utilizadores = _repoUsers.GetDataVM_User(""); // Todos
            List<UserReportModel> ListaUtilizadores = new List<UserReportModel>();
            DateTime? dLastChange = null;
            DateTime? dLogin = null;

            foreach (var item in utilizadores)
            {
                if (DataFormat.IsValidDate(item.Last_Login_Date))
                    dLogin = DateTime.Parse(item.Last_Login_Date.ToString());
                else
                    dLogin = DateTime.Today;

                if (DataFormat.IsValidDate(item.Password_Change_Date))
                    dLastChange = DateTime.Parse(item.Password_Change_Date.ToString());
                else
                    dLastChange = DateTime.Today;

                ListaUtilizadores.Add(new UserReportModel
                {
                    FirstName = _securityProvider.Decrypt(item.User_Name.ToString()),
                    LastChanged = dLastChange,
                    LastLogin = dLogin,
                    Role = GetRoleName(item.RoleId)
                });
            }

            return ListaUtilizadores;
        }

        private string GetRoleName(int Id)
        {
            StringBuilder sbData = new StringBuilder();
            sbData.Clear();
            sbData.Append("SELECT Role ");
            sbData.Append("FROM RoleDetail WHERE Id = @Id");

            using (var connection = ConnectionManager.GetConnection())
            {
                string roleName = connection.Query<string>(sbData.ToString(), new { Id }).SingleOrDefault();
                return roleName;
            }
        }
    }
}
