using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using System.Security.Claims;

namespace Website_Cosmetics.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(int userId, string permissionName);
        Task<bool> HasRoleAsync(int userId, string roleName);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task<bool> GrantPermissionAsync(int userId, string permissionName, int grantedBy);
        Task<bool> RevokePermissionAsync(int userId, string permissionName);
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

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            try
            {
                // Check role-based permissions
                var hasRolePermission = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp)
                    .Join(_context.Permissions, rp => rp.PermissionId, p => p.PermissionId, (rp, p) => p)
                    .AnyAsync(p => p.PermissionName == permissionName && p.IsActive);

                if (hasRolePermission)
                    return true;

                // Check user-specific permissions
                var hasUserPermission = await _context.UserPermissions
                    .Where(up => up.UserId == userId && up.IsActive)
                    .Join(_context.Permissions, up => up.PermissionId, p => p.PermissionId, (up, p) => new { up, p })
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

        public async Task<bool> HasRoleAsync(int userId, string roleName)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r)
                    .AnyAsync(r => r.RoleName == roleName && r.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role {RoleName} for user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            try
            {
                var rolePermissions = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp)
                    .Join(_context.Permissions, rp => rp.PermissionId, p => p.PermissionId, (rp, p) => p)
                    .Where(p => p.IsActive)
                    .Select(p => p.PermissionName)
                    .ToListAsync();

                var userPermissions = await _context.UserPermissions
                    .Where(up => up.UserId == userId && up.IsActive)
                    .Join(_context.Permissions, up => up.PermissionId, p => p.PermissionId, (up, p) => new { up, p })
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

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => r)
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

        public async Task<bool> GrantPermissionAsync(int userId, string permissionName, int grantedBy)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionName == permissionName && p.IsActive);

                if (permission == null)
                    return false;

                var existingPermission = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.PermissionId);

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
                        UserId = userId,
                        PermissionId = permission.PermissionId,
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

        public async Task<bool> RevokePermissionAsync(int userId, string permissionName)
        {
            try
            {
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionName == permissionName);

                if (permission == null)
                    return false;

                var userPermission = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permission.PermissionId);

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
