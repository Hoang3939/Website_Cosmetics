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
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
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
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
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
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
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

    public class RequirePermissionOrAdminAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionName;

        public RequirePermissionOrAdminAttribute(string permissionName)
        {
            _permissionName = permissionName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            
            // Check if user is Admin (Admin has access to everything)
            var isAdmin = await authService.HasRoleAsync(userId, "Admin");
            if (isAdmin)
            {
                return; // Admin has access
            }

            // Check if user has the required permission
            var hasPermission = await authService.HasPermissionAsync(userId, _permissionName);
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireAnyRoleOrPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _rolesOrPermissions;

        public RequireAnyRoleOrPermissionAttribute(params string[] rolesOrPermissions)
        {
            _rolesOrPermissions = rolesOrPermissions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            
            // Check if user has any of the required roles
            var userRoles = await authService.GetUserRolesAsync(userId);
            var hasRole = _rolesOrPermissions.Any(r => userRoles.Contains(r));
            
            if (hasRole)
            {
                return; // User has required role
            }

            // Check if user has any of the required permissions
            foreach (var permission in _rolesOrPermissions)
            {
                var hasPermission = await authService.HasPermissionAsync(userId, permission);
                if (hasPermission)
                {
                    return; // User has required permission
                }
            }

            // User doesn't have required role or permission
            context.Result = new ForbidResult();
        }
    }
}
