using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    public interface IUtilizadorRepository : IBaseRepository<User_Info>
    {
        bool CheckUserExist(string userName);

        IDataReader GetData();  // Utilizador UserData
        UserVM GetDataVM_ID(int ID);
        User_Info GetData_ID(int ID);
        List<UserVM> GetDataVM_User(string sUserName);
        List<User_Info> GetData_User(string sUserName);
        DateTime? GetLastLoginDate(int userID);
        DateTime? GetLastPasswordChange(int userID);
        string GetUserFirstLastName(int userID);
        string GetUserId(string userName);
        string GetUserName(int userID);
        User_Info GetUserProfile(int userID);
        UserRole GetUserRole(int roleId);
        RolesDetail GetRoleDetails(int Id);
        List<User_Info> GetAll();

        bool IsActiveUser(int userID);
        bool IsUser_Admin(string sUserName);
        bool IsValidPassword(int userID, string password);

        bool UpdateLastLoginDate(int userID);
        bool UpdateUserDetails(User_Info User);
        bool UpdateUserProfile(User_Info userProfile);
        bool UpdateUserProfile_Extended(User_Info userProfile);

        bool UserCanChangeData(string userName);
        int UserCount();

        bool UserWithPubs(int UserId);
    }
}