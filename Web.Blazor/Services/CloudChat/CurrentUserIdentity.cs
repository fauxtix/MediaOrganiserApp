using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Web.Blazor.Services.CloudChat
{
    /// <summary>
    /// Resolves the authenticated user for per-request X-UserId / X-IsAdmin headers
    /// sent to the WebApi (data isolation between users).
    /// </summary>
    public sealed class CurrentUserIdentity
    {
        private const string AdministratorRole = "Administrator";

        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public CurrentUserIdentity(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<(string? UserId, bool IsAdmin)> GetAsync(CancellationToken ct = default)
        {
            AuthenticationState state;
            try
            {
                state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            }
            catch
            {
                return (null, false);
            }

            var user = state.User;
            if (user.Identity is null || !user.Identity.IsAuthenticated)
                return (null, false);

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = user.FindFirstValue(ClaimTypes.Role);
            var isAdmin = string.Equals(role, AdministratorRole, StringComparison.OrdinalIgnoreCase)
                || role?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true;

            return (string.IsNullOrWhiteSpace(userId) ? null : userId, isAdmin);
        }
    }
}