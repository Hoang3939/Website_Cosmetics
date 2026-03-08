using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(ApplicationDbContext context, ILogger<PermissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Permissions/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            string? category = null,
            int page = 1,
            int pageSize = 10)
        {
            // Get all permissions
            var permissionsQuery = _context.Permissions.AsQueryable();

            // Filter by search (PermissionName, Description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                permissionsQuery = permissionsQuery.Where(p =>
                    p.PermissionName.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search))
                );
            }

            // Filter by status (Active/Inactive)
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                {
                    permissionsQuery = permissionsQuery.Where(p => p.IsActive == true);
                }
                else if (status.ToLower() == "inactive")
                {
                    permissionsQuery = permissionsQuery.Where(p => p.IsActive == false);
                }
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                permissionsQuery = permissionsQuery.Where(p => p.Category == category);
            }

            // Get total count for pagination
            var totalCount = await permissionsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Pagination
            var permissions = await permissionsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get distinct categories for filter dropdown
            var categories = await _context.Permissions
                .Where(p => p.Category != null)
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Pass filter parameters to ViewBag
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Category = category;
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(permissions);
        }

        // GET: /Admin/Permissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Permissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Permission permission)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if permission name already exists
                    if (await _context.Permissions.AnyAsync(p => p.PermissionName == permission.PermissionName))
                    {
                        ModelState.AddModelError(nameof(permission.PermissionName), "Permission name already exists. Please choose a different name.");
                        return View(permission);
                    }

                    // Set default values
                    permission.IsActive = permission.IsActive;
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.UpdatedAt = DateTime.UtcNow;

                    _context.Permissions.Add(permission);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Permission '{permission.PermissionName}' has been created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                ModelState.AddModelError("", $"Error creating permission: {ex.Message}");
            }

            return View(permission);
        }

        // GET: /Admin/Permissions/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            return View(permission);
        }

        // POST: /Admin/Permissions/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Permission permission)
        {
            if (id != permission.PermissionId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Check if permission name already exists (different from current permission)
                    if (await _context.Permissions.AnyAsync(p => p.PermissionName == permission.PermissionName && p.PermissionId != id))
                    {
                        ModelState.AddModelError(nameof(permission.PermissionName), "Permission name already exists. Please choose a different name.");
                        return View(permission);
                    }

                    var existingPermission = await _context.Permissions.FindAsync(id);
                    if (existingPermission == null)
                    {
                        return NotFound();
                    }

                    // Update permission properties
                    existingPermission.PermissionName = permission.PermissionName;
                    existingPermission.Description = permission.Description;
                    existingPermission.Category = permission.Category;
                    existingPermission.IsActive = permission.IsActive;
                    existingPermission.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Permission '{existingPermission.PermissionName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PermissionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing permission {PermissionId}", id);
                ModelState.AddModelError("", $"Error updating permission: {ex.Message}");
            }

            return View(permission);
        }

        // GET: /Admin/Permissions/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .Include(p => p.UserPermissions)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null)
            {
                return NotFound();
            }

            // Check if permission is being used
            var rolePermissionCount = permission.RolePermissions?.Count ?? 0;
            var userPermissionCount = permission.UserPermissions?.Count ?? 0;
            ViewBag.RolePermissionCount = rolePermissionCount;
            ViewBag.UserPermissionCount = userPermissionCount;

            return View(permission);
        }

        // POST: /Admin/Permissions/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.RolePermissions)
                    .Include(p => p.UserPermissions)
                    .FirstOrDefaultAsync(p => p.PermissionId == id);

                if (permission == null)
                {
                    return NotFound();
                }

                // Check if permission is being used
                var rolePermissionCount = permission.RolePermissions?.Count ?? 0;
                var userPermissionCount = permission.UserPermissions?.Count ?? 0;
                
                if (rolePermissionCount > 0 || userPermissionCount > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete permission '{permission.PermissionName}' because it is being used by {rolePermissionCount} role(s) and {userPermissionCount} user(s). Please remove those assignments first.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                var permissionName = permission.PermissionName;

                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Permission '{permissionName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
                TempData["ErrorMessage"] = $"Error deleting permission: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method
        private async Task<bool> PermissionExists(int id)
        {
            return await _context.Permissions.AnyAsync(e => e.PermissionId == id);
        }
    }
}

