using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Enums;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediaOrganizerApp.Application.Interfaces.Services.Api
{
    public interface IUserService
    {
        long Insert(User_Info entity);
        void DeleteById(int Id);
        void Update(User_Info entity);
        bool UpdateLastLoginDate(int Id);
        bool UpdateUserProfile(int userID, string userName, string firstName, string email, string mobile, int Ativo, int roleId);
        bool UpdateUserProfile(int userID, string userName, string firstName, string password, string email, string mobile, int Ativo, int roleId);

        bool CheckUserExist(string userName);
        bool IsValidPassword(int Id, string password);

        IEnumerable<User_Info> Query(string where = null);
        User_Info Query_ById(int id);
        bool TableHasData();

        DateTime? GetLastLoginDate(int userID);
        DateTime? GetLastPasswordChange(int userID);

        List<User_Info> GetAllUsers();
        UserVM GetDataVM_ID(int Id);
        List<UserVM> GetDataVM_User(string sUserName);
        User_Info GetData_ID(int Id);
        List<User_Info> GetData_User(string sUserName);
        int GetFirstId();
        RolesDetail GetRoleDetails(int Id);
        User_Info GetUserProfile(int Id);
        AppDefinitions.UserRole GetUserRole(int Id);
        IDataReader GetUsersData(User_Info UserData);
        string GetRoleName(int Id);

        User_Info DecryptUser(User_Info user);
        List<User_Info> DecryptUsers(IEnumerable<User_Info> users);

        bool UserWithPubs(int UserId);
        Task<(string UserId, string Role)> GetAuthenticatedUserDetails(AuthenticationStateProvider authenticationStateProvider);
    }
}