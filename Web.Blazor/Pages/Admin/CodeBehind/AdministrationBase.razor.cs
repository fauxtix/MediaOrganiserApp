using MediaOrganizerApp.Application.Interfaces.Services.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Notifications;
using Syncfusion.Blazor.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Web.Blazor.Pages.Admin.CodeBehind
{
    public class AdministrationBase : ComponentBase
    {
        [Inject] public IStringLocalizer<App> LocalizerApp { get; set; }
        [Inject] public UserManager<IdentityUser> UserManager { get; set; }
        [Inject] public RoleManager<IdentityRole> RoleManager { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IUsersService UserService { get; set; }

        protected SfGrid<IdentityUser> gridObj;
        protected SfToast ToastObj;
        public DialogEffect Effects = DialogEffect.Zoom;
        protected IdentityUser User = new();
        protected bool EditRecord = true;

        // Dialogs visibility flags
        public bool ErrorVisibility { get; set; } = false;
        public bool EditUserDialogVisibility { get; set; } = false;
        public bool DeleteRecordConfirmVisibility { get; set; } = false;
        public bool DeleteUserConfirmVisibility { get; set; } = false;
        public bool ValidationErrorsVisibility { get; set; } = false;

        protected List<string> ErrorMsgsList = new();
        public string userID { get; set; }

        protected SfDropDownList<string, IdentityRole> ddlRoles { get; set; }
        protected string ToastTitle = "";
        protected string ToastContent = "";
        protected string ToastCssClass = "";

        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; }
        protected string ADMINISTRATION_ROLE = "Administrator";
        protected System.Security.Claims.ClaimsPrincipal CurrentUser;

        // Property used to add or edit the currently selected user
        protected IdentityUser objUser = new();
        // Tracks the selected role for the currently selected user
        protected string CurrentUserRole { get; set; } = "User";
        // Collection to display the existing users
        protected List<IdentityUser> ColUsers = new();
        // Options to display in the roles dropdown when editing a user
        protected List<string> Options = new() { "User", "Administrator" };
        // To hold any possible errors
        protected string strError = "";
        // To enable showing the Popup
        protected bool ShowPopup = false;

        protected List<IdentityRole> userRoles;
        protected string userId;
        protected string userRoleId;

        protected bool IsUserDoctor { get; set; } = false;
        protected int doctorID;

        protected string sEditUserMsg;
        protected string sCreateUserMsg;
        IdentityUser user = new();

        protected override async Task OnInitializedAsync()
        {
            sEditUserMsg = $"{LocalizerApp["editionMsg"]} {LocalizerApp["lblUtilizador"]}";
            sCreateUserMsg = $"{LocalizerApp["creationMsg"]} {LocalizerApp["lblUtilizador"]}";

            // Get the current logged in user
            if (CurrentUser is null)
                CurrentUser = (await AuthenticationStateTask).User;
            var user = await UserManager.FindByNameAsync(CurrentUser.Identity.Name);

            IdentityUser RoleResult = await UserManager.FindByNameAsync(CurrentUser.Identity.Name);
            if (RoleResult is null) return;

            string sUserId = RoleResult.Id;
            userRoleId = await UserService.GetUserRoleId(sUserId);

            userRoles = RoleManager.Roles.ToList();

            // Get the users to fill grid
            GetUsers();
        }

        public async Task onUserDoubleClick(RecordDoubleClickEventArgs<IdentityUser> args)
        {
            User = args.RowData;
            userID = User.Id;
            userRoleId = await UserService.GetUserRoleId(userID);

            EditUserDialogVisibility = true;
        }

        public async Task OnUserCommandClicked(CommandClickEventArgs<IdentityUser> args)
        {
            User = args.RowData;
            User.Id = args.RowData.Id;
            userID = User.Id;

            userRoleId = await UserService.GetUserRoleId(userID);

            if (args.CommandColumn.Type == CommandButtonType.Edit)
            {
                EditUserDialogVisibility = true;
            }

            if (args.CommandColumn.Type == CommandButtonType.Delete)
            {
                DeleteUserConfirmVisibility = true;
            }
        }

        public void Voltar()
        {
            EditUserDialogVisibility = false;
        }

        public void CloseErrorBox()
        {
            ErrorVisibility = false;
            EditUserDialogVisibility = true;
        }

        public async Task ConfirmDeleteYes()
        {
            await DeleteUser();
            // Close the Popup
            DeleteUserConfirmVisibility = false;

            // Refresh Users
            GetUsers();
        }

        public void ConfirmDeleteNo()
        {
            DeleteUserConfirmVisibility = false;
        }

        public async void OnUserToolbarClick(Syncfusion.Blazor.Navigations.ClickEventArgs args)
        {
            if (args.Item.Id == "Clear")
            {
                await gridObj.SearchAsync("");
            }
        }

        protected void ClosePopup()
        {
            // Close the Popup
            EditUserDialogVisibility = false;
        }

        protected void GetUsers()
        {
            strError = "";
            ColUsers = new List<IdentityUser>();
            try
            {
                var user = UserManager.Users.Select(usr => new IdentityUser
                {
                    Id = usr.Id,
                    UserName = usr.UserName,
                    Email = usr.Email,
                    PasswordHash = "*****"
                });
                foreach (var item in user)
                {
                    ColUsers.Add(item);
                }

            }
            catch (Exception ex)
            {
                var er = ex.Message;
                throw;
            }
        }

        public async Task<string> GetUserRole(IdentityUser user)
        {
            return (await UserManager.GetRolesAsync(user)).SingleOrDefault();
        }

        protected async Task AddNewUser()
        {
            // Make new user
            objUser = new();
            objUser.PasswordHash = "*****";
            objUser.PhoneNumber = "";
            objUser.PhoneNumberConfirmed = true;
            objUser.UserName = "";
            objUser.EmailConfirmed = true;
            // Set Id to blank so we know it is a new record

            objUser.Id = "";
            IsUserDoctor = true;
            userRoleId = await UserService.GetRoleIdByName("User"); // default, admin user is already created
            User = objUser;

            EditRecord = false; // insert mode

            // Open the Popup
            EditUserDialogVisibility = true;

        }

        protected async Task SaveUser()
        {
            try
            {
                // Is this an existing user? Yes ==> update user
                if (User.Id != "")
                {
                    await UpdateUser();
                }
                else
                {
                    await CreateUser();
                }

                if (ErrorMsgsList.Any() || ErrorMsgsList.Count > 0)
                {
                    ErrorVisibility = true;
                }
                else
                {
                    // Close the Popup
                    EditUserDialogVisibility = false;
                    // Refresh Users
                    GetUsers();
                }
            }
            catch (Exception ex)
            {
                strError = ex.GetBaseException().Message;
                ErrorMsgsList.Add(strError);
                ErrorVisibility = true;
            }
        }

        protected async Task EditUser(IdentityUser _IdentityUser)
        {
            // Set the selected user
            // as the current user
            User = _IdentityUser;
            // Get the user
            var user = await UserManager.FindByIdAsync(User.Id);
            if (user != null)
            {
                // Is user in administrator role?
                var UserResult = await UserManager.IsInRoleAsync(user, ADMINISTRATION_ROLE);

                if (UserResult)
                {
                    CurrentUserRole = ADMINISTRATION_ROLE;
                }
                else
                {
                    CurrentUserRole = "User";
                }
            }
            // Open the Popup
            EditUserDialogVisibility = true;
        }


        protected async Task DeleteUser()
        {
            // Get the user
            var user = await UserManager.FindByIdAsync(User.Id);
            if (user != null)
            {
                // Delete the user
                await UserManager.DeleteAsync(user);
            }
        }

        private async Task UpdateUser()
        {
            // Get the user
            var user = await UserManager.FindByIdAsync(User.Id);

            // Update Fields
            user.Email = User.Email;
            user.UserName = User.UserName;
            user.PasswordHash = User.PasswordHash;

            // Update the user
            await UserManager.UpdateAsync(user);

            // Only update password if the current value
            // is not the default value
            if (User.PasswordHash != "*****")
            {
                var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
                var passworduser = await UserManager.ResetPasswordAsync(
                        user,
                        resetToken,
                        User.PasswordHash);

                if (!passworduser.Succeeded)
                {
                    if (passworduser.Errors.FirstOrDefault() != null)
                    {
                        foreach (var error in passworduser.Errors)
                        {
                            ErrorMsgsList.Add(error.Description);
                        }
                        //strError = passworduser.Errors.FirstOrDefault().Description;
                    }
                    else
                    {
                        ErrorMsgsList.Add("Erro na password");
                        strError = "Erro na password";
                    }
                    // Keep the popup opened
                    return;
                }
            }

            // Handle Roles

            // Is user in administrator role?
            var UserResult = await UserManager.IsInRoleAsync(user, ADMINISTRATION_ROLE);

            // Is Administrator role selected
            // but user is not an Administrator?
            if ((CurrentUserRole == ADMINISTRATION_ROLE) & (!UserResult))
            {
                // Put admin in Administrator role
                await UserManager.AddToRoleAsync(user, ADMINISTRATION_ROLE);
            }
            else if ((CurrentUserRole == "User") & (!UserResult))
            {
                var RoleResult = await RoleManager.FindByNameAsync("User");
                if (RoleResult == null)
                {
                    // Create Users Role
                    await RoleManager.CreateAsync(new IdentityRole("User"));
                    await UserManager.AddToRoleAsync(user, "User");
                }

            }
            else  // Admin
            {
                // Is Administrator role not selected
                // but user is an Administrator?
                if ((CurrentUserRole != ADMINISTRATION_ROLE) & (UserResult))
                {
                    // Remove user from Administrator role
                    await UserManager.RemoveFromRoleAsync(user, ADMINISTRATION_ROLE);
                }
            }
        }

        private async Task CreateUser()
        {
            var NewUser =
                new IdentityUser
                {
                    UserName = User.UserName,
                    Email = User.Email,
                    PasswordHash = User.PasswordHash,
                    PhoneNumber = "",
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true
                };

            var CreateResult = await UserManager.CreateAsync(NewUser, User.PasswordHash);

            if (!CreateResult.Succeeded)
            {
                if (CreateResult.Errors.FirstOrDefault() != null)
                {
                    foreach (var error in CreateResult.Errors)
                    {
                        ErrorMsgsList.Add(error.Description);
                    }
                    strError = CreateResult.Errors.FirstOrDefault().Description;
                }
                else
                {
                    ErrorMsgsList.Add(LocalizerApp["ErroGeral"]);
                    strError = LocalizerApp["ErroGeral"];
                }
                return;
            }
            else // CreateResult.Succeeded
            {
                // Handle Roles
                if (CurrentUserRole == ADMINISTRATION_ROLE)
                {
                    // Put admin in Administrator role
                    await UserManager.AddToRoleAsync(NewUser, ADMINISTRATION_ROLE);
                }
                else
                {
                    var RoleResult = await RoleManager.FindByNameAsync("User");
                    if (RoleResult == null)
                    {
                        await RoleManager.CreateAsync(new IdentityRole("User"));
                    }

                    await UserManager.AddToRoleAsync(NewUser, "User");
                }
            }
        }
    }
}

