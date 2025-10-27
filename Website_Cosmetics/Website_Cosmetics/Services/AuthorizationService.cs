using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using System.Security.Claims;

namespace Website_Cosmetics.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(Guid userId, string permissionName);
        Task<bool> HasRoleAsync(Guid userId, string roleName);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
        Task<List<string>> GetUserRolesAsync(Guid userId);
        Task<bool> GrantPermissionAsync(Guid userId, string permissionName, Guid grantedBy);
        Task<bool> RevokePermissionAsync(Guid userId, string permissionName);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(ApplicationDbContext context, ILogger<AuthorizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
        {
            try
            {
                // Check role-based permissions
                var hasRolePermission = await _context.UserRoles
                    .Where(ur => ur.UserUID == userId)
                    .Join(_context.RolePermissions, ur => ur.RoleUID, rp => rp.RoleUID, (ur, rp) => rp)
                    .Join(_context.Permissions, rp => rp.PermissionUID, p => p.UID, (rp, p) => p)
                    .AnyAsync(p => p.PermissionName == permissionName && p.IsActive);

                if (hasRolePermission)
                    return true;

                // Check user-specific permissions
                var hasUserPermission = await _context.UserPermissions
                    .Where(up => up.UserUID == userId && up.IsActive)
                    .Join(_context.Permissions, up => up.PermissionUID, p => p.UID, (up, p) => new { up, p })
                    .AnyAsync(x => x.p.PermissionName == permissionName && 
                                   x.p.IsActive && 
                                   (x.up.ExpiresAt == null || x.up.ExpiresAt > DateTime.UtcNow));

                return hasUserPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {PermissionName} for user {UserId}", permissionName, userId);
                return false;
            }
        }

        public async Task<bool> HasRoleAsync(Guid userId, string roleName)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserUID == userId)
                    .Join(_context.Roles, ur => ur.RoleUID, r => r.UID, (ur, r) => r)
                    .AnyAsync(r => r.RoleName == roleName && r.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role {RoleName} for user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                var rolePermissions = await _context.UserRoles
                    .Where(ur => ur.UserUID == userId)
                    .Join(_context.RolePermissions, ur => ur.RoleUID, rp => rp.RoleUID, (ur, rp) => rp)
                    .Join(_context.Permissions, rp => rp.PermissionUID, p => p.UID, (rp, p) => p)
                    .Where(p => p.IsActive)
                    .Select(p => p.PermissionName)
                    .ToListAsync();

                var userPermissions = await _context.UserPermissions
                    .Where(up => up.UserUID == userId && up.IsActive)
                    .Join(_context.Permissions, up => up.PermissionUID, p => p.UID, (up, p) => new { up, p })
                    .Where(x => x.p.IsActive && (x.up.ExpiresAt == null || x.up.ExpiresAt > DateTime.UtcNow))
                    .Select(x => x.p.PermissionName)
                    .ToListAsync();

                return rolePermissions.Union(userPermissions).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserUID == userId)
                    .Join(_context.Roles, ur => ur.RoleUID, r => r.UID, (ur, r) => r)
                    .Where(r => r.IsActive)
                    .Select(r => r.RoleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> GrantPermissionAsync(Guid userId, string permissionName, Guid grantedBy)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionName == permissionName && p.IsActive);

                if (permission == null)
                    return false;

                var existingPermission = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserUID == userId && up.PermissionUID == permission.UID);

                if (existingPermission != null)
                {
                    existingPermission.IsActive = true;
                    existingPermission.GrantedBy = grantedBy;
                    existingPermission.GrantedAt = DateTime.UtcNow;
                    existingPermission.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserPermissions.Add(new UserPermission
                    {
                        UserUID = userId,
                        PermissionUID = permission.UID,
                        GrantedBy = grantedBy,
                        GrantedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission {PermissionName} to user {UserId}", permissionName, userId);
                return false;
            }
        }

        public async Task<bool> RevokePermissionAsync(Guid userId, string permissionName)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionName == permissionName);

                if (permission == null)
                    return false;

                var userPermission = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserUID == userId && up.PermissionUID == permission.UID);

                if (userPermission != null)
                {
                    userPermission.IsActive = false;
                    userPermission.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission {PermissionName} from user {UserId}", permissionName, userId);
                return false;
            }
        }
    }
}
