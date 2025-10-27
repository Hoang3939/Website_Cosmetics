using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Attributes
{
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionName;

        public RequirePermissionAttribute(string permissionName)
        {
            _permissionName = permissionName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var hasPermission = await authService.HasPermissionAsync(userId, _permissionName);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _roleName;

        public RequireRoleAttribute(string roleName)
        {
            _roleName = roleName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var hasRole = await authService.HasRoleAsync(userId, _roleName);

            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireAnyRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roleNames;

        public RequireAnyRoleAttribute(params string[] roleNames)
        {
            _roleNames = roleNames;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            var userRoles = await authService.GetUserRolesAsync(userId);

            if (!_roleNames.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
