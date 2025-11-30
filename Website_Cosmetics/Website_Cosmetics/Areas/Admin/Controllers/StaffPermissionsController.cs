using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermission("Admin.Permission.Manage")]
    public class StaffPermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authService;
        private readonly ILogger<StaffPermissionsController> _logger;

        public StaffPermissionsController(
            ApplicationDbContext context,
            IAuthorizationService authService,
            ILogger<StaffPermissionsController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // GET: /Admin/StaffPermissions/
        public async Task<IActionResult> Index(
            string? search = null,
            string? category = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả staff users
            var staffQuery = _context.UserRoles
                .Where(ur => ur.Role.RoleName == "Staff")
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Select(ur => ur.User)
                .Distinct();

            // Filter theo search (tên, username, email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                staffQuery = staffQuery.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.FirstName != null && u.FirstName.Contains(search)) ||
                    (u.LastName != null && u.LastName.Contains(search)) ||
                    (u.FirstName + " " + u.LastName).Contains(search)
                );
            }

            // Lấy tổng số để phân trang
            var totalCount = await staffQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var staff = await staffQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load permissions cho mỗi staff
            var staffWithPermissions = new List<StaffPermissionViewModel>();
            foreach (var user in staff)
            {
                var permissions = await _authService.GetUserPermissionsAsync(user.UserId);
                staffWithPermissions.Add(new StaffPermissionViewModel
                {
                    User = user,
                    Permissions = permissions
                });
            }

            // Truyền filter parameters vào ViewBag
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            // Lấy danh sách categories
            ViewBag.Categories = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .Where(c => c != null)
                .ToListAsync();

            return View(staffWithPermissions);
        }

        // GET: /Admin/StaffPermissions/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem user có role Staff không
            var hasStaffRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Staff");
            if (!hasStaffRole)
            {
                return NotFound();
            }

            // Lấy permissions của user
            var userPermissions = await _authService.GetUserPermissionsAsync(user.UserId);

            // Lấy tất cả permissions có thể cấp (Category = "Staff")
            var allPermissions = await _context.Permissions
                .Where(p => p.Category == "Staff" && p.IsActive)
                .OrderBy(p => p.PermissionName)
                .ToListAsync();

            // Lấy UserPermissions với thông tin chi tiết
            var userPermissionDetails = await _context.UserPermissions
                .Where(up => up.UserId == id && up.IsActive)
                .Include(up => up.Permission)
                .Include(up => up.GrantedByUser)
                .OrderBy(up => up.Permission.PermissionName)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserPermissions = userPermissions;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.UserPermissionDetails = userPermissionDetails;

            return View();
        }

        // GET: /Admin/StaffPermissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/StaffPermissions/Create
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
                        ModelState.AddModelError(nameof(permission.PermissionName), "Tên quyền đã tồn tại. Vui lòng chọn tên khác.");
                        return View(permission);
                    }

                    // Set default values
                    permission.IsActive = true;
                    permission.CreatedAt = DateTime.UtcNow;
                    permission.UpdatedAt = DateTime.UtcNow;

                    _context.Permissions.Add(permission);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Quyền '{permission.PermissionName}' đã được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                ModelState.AddModelError("", $"Lỗi khi tạo quyền: {ex.Message}");
            }

            return View(permission);
        }

        // GET: /Admin/StaffPermissions/Edit/{id}
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

        // POST: /Admin/StaffPermissions/Edit/{id}
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
                    // Check if permission name already exists (khác permission hiện tại)
                    if (await _context.Permissions.AnyAsync(p => p.PermissionName == permission.PermissionName && p.PermissionId != id))
                    {
                        ModelState.AddModelError(nameof(permission.PermissionName), "Tên quyền đã tồn tại. Vui lòng chọn tên khác.");
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

                    TempData["SuccessMessage"] = $"Quyền '{existingPermission.PermissionName}' đã được cập nhật thành công.";
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
                ModelState.AddModelError("", $"Lỗi khi cập nhật quyền: {ex.Message}");
            }

            return View(permission);
        }

        // GET: /Admin/StaffPermissions/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions
                .Include(p => p.UserPermissions)
                .Include(p => p.RolePermissions)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có user hoặc role nào đang sử dụng permission này không
            var userPermissionCount = permission.UserPermissions?.Count(up => up.IsActive) ?? 0;
            var rolePermissionCount = permission.RolePermissions?.Count ?? 0;
            ViewBag.UserPermissionCount = userPermissionCount;
            ViewBag.RolePermissionCount = rolePermissionCount;

            return View(permission);
        }

        // POST: /Admin/StaffPermissions/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var permission = await _context.Permissions
                    .Include(p => p.UserPermissions)
                    .Include(p => p.RolePermissions)
                    .FirstOrDefaultAsync(p => p.PermissionId == id);

                if (permission == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem có user hoặc role nào đang sử dụng permission này không
                var userPermissionCount = permission.UserPermissions?.Count(up => up.IsActive) ?? 0;
                var rolePermissionCount = permission.RolePermissions?.Count ?? 0;

                if (userPermissionCount > 0 || rolePermissionCount > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa quyền '{permission.PermissionName}' vì có {userPermissionCount} user và {rolePermissionCount} role đang sử dụng quyền này.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                var permissionName = permission.PermissionName;

                // Xóa permission (cascade sẽ xóa UserPermissions và RolePermissions)
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Quyền '{permissionName}' đã được xóa thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi xóa quyền: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/StaffPermissions/GrantPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermission(int userId, string permissionName)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var success = await _authService.GrantPermissionAsync(userId, permissionName, currentUserId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Quyền đã được cấp thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cấp quyền. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission {PermissionName} to user {UserId}", permissionName, userId);
                TempData["ErrorMessage"] = $"Lỗi khi cấp quyền: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = userId });
        }

        // POST: /Admin/StaffPermissions/RevokePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokePermission(int userId, string permissionName)
        {
            try
            {
                var success = await _authService.RevokePermissionAsync(userId, permissionName);

                if (success)
                {
                    TempData["SuccessMessage"] = "Quyền đã được thu hồi thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể thu hồi quyền. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission {PermissionName} from user {UserId}", permissionName, userId);
                TempData["ErrorMessage"] = $"Lỗi khi thu hồi quyền: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = userId });
        }

        // POST: /Admin/StaffPermissions/AssignDefaultPermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDefaultPermissions(int userId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Default permissions cho Staff: Order.Manage và Customer.Manage
                var defaultPermissions = new[] { "Admin.Order.Manage", "Admin.Customer.Manage" };

                foreach (var permissionName in defaultPermissions)
                {
                    await _authService.GrantPermissionAsync(userId, permissionName, currentUserId);
                }

                TempData["SuccessMessage"] = "Đã cấp quyền mặc định cho nhân viên thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning default permissions to user {UserId}", userId);
                TempData["ErrorMessage"] = $"Lỗi khi cấp quyền mặc định: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = userId });
        }

        // Helper method
        private async Task<bool> PermissionExists(int id)
        {
            return await _context.Permissions.AnyAsync(e => e.PermissionId == id);
        }
    }

    // ViewModel for Staff with Permissions
    public class StaffPermissionViewModel
    {
        public User User { get; set; } = null!;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}

