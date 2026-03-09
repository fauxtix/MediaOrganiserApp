using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Web.Blazor.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<IdentityUser> _manager;
        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, UserManager<IdentityUser> manager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _manager = manager;
        }

        // this code shoudn't be executed here (!!!)
        public async Task<IActionResult> OnGet()
        {
            var user = await GetCurrentUser();
            string userEmail = user.Email;
            _logger.LogInformation($"User {userEmail} logged out.");

            await _signInManager.SignOutAsync();
            return LocalRedirect("/");
        }


        // didn't get called (??)
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            _logger.LogInformation("User logged out.");

            await _signInManager.SignOutAsync();
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect the user to application root
                return LocalRedirect("/");
                //return LocalRedirect("~/Identity/Account/Login");
            }
        }

        private async Task<IdentityUser> GetCurrentUser()
        {
            return await _manager.GetUserAsync(HttpContext.User);
        }
    }
}


