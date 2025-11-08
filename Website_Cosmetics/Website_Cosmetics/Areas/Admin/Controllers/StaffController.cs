using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [RequirePermission("Admin.Staff.Manage")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(ApplicationDbContext context, IAuthorizationService authService, ILogger<StaffController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // GET: /Admin/Staff/
        public async Task<IActionResult> Index()
        {
            var staff = await _context.UserRoles
                .Where(ur => ur.Role.RoleName == "Staff")
                .Include(ur => ur.User)
                .Select(ur => ur.User)
                .ToListAsync();

            return View(staff);
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
            if (ModelState.IsValid)
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Username or email already exists.");
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
                    IsActive = true
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
                        RoleId = staffRole.RoleId
                    });
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Staff created successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
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

        // POST: /Admin/Staff/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Staff deactivated successfully!";
            }

            return RedirectToAction("Index");
        }

        // POST: /Admin/Staff/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Staff activated successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}
