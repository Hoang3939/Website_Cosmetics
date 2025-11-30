using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;
using CustomAuth = Website_Cosmetics.Services;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomAuth.IAuthorizationService _authService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(ApplicationDbContext context, CustomAuth.IAuthorizationService authService, ILogger<StaffController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // GET: /Admin/Staff/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            string? role = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả users có role Staff
            var staffQuery = _context.UserRoles
                .Where(ur => ur.Role.RoleName == "Staff")
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Select(ur => ur.User);

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

            // Filter theo trạng thái (Active/Inactive)
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                {
                    staffQuery = staffQuery.Where(u => u.IsActive == true);
                }
                else if (status.ToLower() == "inactive")
                {
                    staffQuery = staffQuery.Where(u => u.IsActive == false);
                }
            }

            // Filter theo role (nếu có nhiều role trong tương lai)
            if (!string.IsNullOrWhiteSpace(role))
            {
                staffQuery = staffQuery.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == role));
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

            // Load roles cho mỗi staff
            foreach (var user in staff)
            {
                await _context.Entry(user)
                    .Collection(u => u.UserRoles)
                    .Query()
                    .Include(ur => ur.Role)
                    .LoadAsync();
            }

            // Truyền filter parameters vào ViewBag
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Role = role;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(staff);
        }

        // GET: /Admin/Staff/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Include(u => u.UserAddresses)
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

            // Load permissions
            var permissions = await _authService.GetUserPermissionsAsync(user.UserId);
            ViewBag.Permissions = permissions;

            return View(user);
        }

        // GET: /Admin/Staff/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if username already exists
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username đã tồn tại. Vui lòng chọn username khác.");
                        return View(model);
                    }

                    // Check if email already exists
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại. Vui lòng sử dụng email khác.");
                        return View(model);
                    }

                    // Create new user
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"), // Default password
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        IsEmailConfirmed = true, // Admin creates, so auto-confirm
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Assign Staff role
                    var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Staff");
                    if (staffRole != null)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = staffRole.RoleId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = $"Nhân viên '{user.FullName}' đã được tạo thành công. Mật khẩu mặc định: Staff123!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating staff");
                ModelState.AddModelError("", $"Lỗi khi tạo nhân viên: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Admin/Staff/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
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

            return View(user);
        }

        // POST: /Admin/Staff/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
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

                    // Check if username already exists (khác user hiện tại)
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != id))
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username đã tồn tại. Vui lòng chọn username khác.");
                        return View(model);
                    }

                    // Check if email already exists (khác user hiện tại)
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại. Vui lòng sử dụng email khác.");
                        return View(model);
                    }

                    // Update user properties
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsActive = model.IsActive;
                    user.IsEmailConfirmed = model.IsEmailConfirmed;
                    user.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Thông tin nhân viên '{user.FullName}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StaffExists(id))
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
                _logger.LogError(ex, "Error editing staff {StaffId}", id);
                ModelState.AddModelError("", $"Lỗi khi cập nhật nhân viên: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Admin/Staff/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
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

            return View(user);
        }

        // POST: /Admin/Staff/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
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

                var userName = user.FullName;

                // Xóa user (cascade sẽ xóa UserRoles, UserPermissions, etc.)
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Nhân viên '{userName}' đã được xóa thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff {StaffId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi xóa nhân viên: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/Staff/Permissions/{id}
        public async Task<IActionResult> Permissions(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var userPermissions = await _authService.GetUserPermissionsAsync(id);
            var allPermissions = await _context.Permissions
                .Where(p => p.Category == "Staff" && p.IsActive)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserPermissions = userPermissions;
            ViewBag.AllPermissions = allPermissions;

            return View();
        }

        // POST: /Admin/Staff/GrantPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPermission(int userId, string permissionName)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var success = await _authService.GrantPermissionAsync(userId, permissionName, currentUserId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Permission granted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to grant permission!";
            }

            return RedirectToAction("Permissions", new { id = userId });
        }

        // POST: /Admin/Staff/RevokePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokePermission(int userId, string permissionName)
        {
            var success = await _authService.RevokePermissionAsync(userId, permissionName);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Permission revoked successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to revoke permission!";
            }

            return RedirectToAction("Permissions", new { id = userId });
        }

        // POST: /Admin/Staff/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user != null)
                {
                    // Kiểm tra xem user có role Staff không
                    var hasStaffRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Staff");
                    if (!hasStaffRole)
                    {
                        return NotFound();
                    }

                    user.IsActive = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Nhân viên '{user.FullName}' đã được kích hoạt thành công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating staff {StaffId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi kích hoạt nhân viên: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Staff/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user != null)
                {
                    // Kiểm tra xem user có role Staff không
                    var hasStaffRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Staff");
                    if (!hasStaffRole)
                    {
                        return NotFound();
                    }

                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Nhân viên '{user.FullName}' đã được vô hiệu hóa thành công.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating staff {StaffId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi vô hiệu hóa nhân viên: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method
        private async Task<bool> StaffExists(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AnyAsync(u => u.UserId == id && u.UserRoles.Any(ur => ur.Role.RoleName == "Staff"));
        }
    }
}
