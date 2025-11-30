using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Website_Cosmetics.Attributes;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ApplicationDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Customers/
        public async Task<IActionResult> Index(
            string? search = null,
            string? status = null,
            string? role = null,
            int page = 1,
            int pageSize = 10)
        {
            // Lấy tất cả users có role User (Customer)
            var customerQuery = _context.UserRoles
                .Where(ur => ur.Role.RoleName == "User")
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Select(ur => ur.User);

            // Filter theo search (tên, username, email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                customerQuery = customerQuery.Where(u =>
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
                    customerQuery = customerQuery.Where(u => u.IsActive == true);
                }
                else if (status.ToLower() == "inactive")
                {
                    customerQuery = customerQuery.Where(u => u.IsActive == false);
                }
            }

            // Filter theo role (nếu có nhiều role trong tương lai)
            if (!string.IsNullOrWhiteSpace(role))
            {
                customerQuery = customerQuery.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == role));
            }

            // Lấy tổng số để phân trang
            var totalCount = await customerQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Validate page number
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Phân trang
            var customers = await customerQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load roles cho mỗi customer
            foreach (var user in customers)
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

            return View(customers);
        }

        // GET: /Admin/Customers/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserAddresses)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem user có role User (Customer) không
            var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "User");
            if (!hasCustomerRole)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /Admin/Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model)
        {
            try
            {
                // Remove fields not posted by the form to avoid ModelState invalidation
                ModelState.Remove(nameof(model.PasswordHash));
                ModelState.Remove(nameof(model.UserId));
                ModelState.Remove(nameof(model.CreatedAt));
                ModelState.Remove(nameof(model.UpdatedAt));
                ModelState.Remove(nameof(model.UserRoles));
                ModelState.Remove(nameof(model.PasswordResetTokens));
                ModelState.Remove(nameof(model.EmailConfirmationTokens));
                ModelState.Remove(nameof(model.UserPermissions));
                ModelState.Remove(nameof(model.GrantedPermissions));
                ModelState.Remove(nameof(model.ShoppingCarts));
                ModelState.Remove(nameof(model.Orders));
                ModelState.Remove(nameof(model.ProductReviews));
                ModelState.Remove(nameof(model.ProductLikes));
                ModelState.Remove(nameof(model.UserAddresses));

                if (ModelState.IsValid)
                {
                    // Check if username already exists
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username already exists. Please choose a different username.");
                        return View(model);
                    }

                    // Check if email already exists
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Email already exists. Please use a different email.");
                        return View(model);
                    }

                    // Create new user
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"), // Default password
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        IsEmailConfirmed = model.IsEmailConfirmed, // Use value from form
                        IsActive = model.IsActive, // Use value from form
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Assign User role (Customer)
                    var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                    if (userRole != null)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = userRole.RoleId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("User role not found in database. Please ensure the User role exists.");
                        TempData["ErrorMessage"] = "Error: User role not found in database. Please check the database configuration.";
                        return View(model);
                    }

                    TempData["SuccessMessage"] = $"Customer '{user.FullName}' has been created successfully. Default password: Customer123!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log ModelState errors for debugging
                    foreach (var error in ModelState)
                    {
                        foreach (var errorMessage in error.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Error - {Key}: {Message}", error.Key, errorMessage.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Admin/Customers/Edit/{id}
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

            // Kiểm tra xem user có role User (Customer) không
            var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "User");
            if (!hasCustomerRole)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Admin/Customers/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model)
        {
            if (id == 0)
            {
                id = model.UserId;
            }
            if (id != model.UserId)
            {
                return NotFound();
            }

            try
            {
                // Remove fields not posted by the form to avoid ModelState invalidation
                ModelState.Remove(nameof(model.PasswordHash));
                ModelState.Remove(nameof(model.CreatedAt));
                ModelState.Remove(nameof(model.UpdatedAt));
                ModelState.Remove(nameof(model.UserRoles));
                ModelState.Remove(nameof(model.PasswordResetTokens));
                ModelState.Remove(nameof(model.EmailConfirmationTokens));
                ModelState.Remove(nameof(model.UserPermissions));
                ModelState.Remove(nameof(model.GrantedPermissions));
                ModelState.Remove(nameof(model.ShoppingCarts));
                ModelState.Remove(nameof(model.Orders));
                ModelState.Remove(nameof(model.ProductReviews));
                ModelState.Remove(nameof(model.ProductLikes));
                ModelState.Remove(nameof(model.UserAddresses));

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

                    // Kiểm tra xem user có role User (Customer) không
                    var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "User");
                    if (!hasCustomerRole)
                    {
                        return NotFound();
                    }

                    // Check if username already exists (khác user hiện tại)
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != id))
                    {
                        ModelState.AddModelError(nameof(model.Username), "Username already exists. Please choose a different username.");
                        return View(model);
                    }

                    // Check if email already exists (khác user hiện tại)
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
                    {
                        ModelState.AddModelError(nameof(model.Email), "Email already exists. Please use a different email.");
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

                    TempData["SuccessMessage"] = $"Customer '{user.FullName}' has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Log ModelState errors for diagnostics
                    foreach (var kv in ModelState)
                    {
                        foreach (var err in kv.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Error (Edit) - {Key}: {Message}", kv.Key, err.ErrorMessage);
                        }
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CustomerExists(id))
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
                _logger.LogError(ex, "Error editing customer {CustomerId}", id);
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
            }

            return View(model);
        }

        // GET: /Admin/Customers/Delete/{id}
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

            // Kiểm tra xem user có role User (Customer) không
            var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "User");
            if (!hasCustomerRole)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Admin/Customers/Delete/{id}
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

                // Kiểm tra xem user có role User (Customer) không
                var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "User");
                if (!hasCustomerRole)
                {
                    return NotFound();
                }

                var userName = user.FullName;

                // Xóa user (cascade sẽ xóa UserRoles, UserAddresses, Orders, etc.)
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer '{userName}' has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                TempData["ErrorMessage"] = $"Error deleting customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Customers/Activate
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
                    // Kiểm tra xem user có role Customer không
                    var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Customer");
                    if (!hasCustomerRole)
                    {
                        return NotFound();
                    }

                    user.IsActive = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Customer '{user.FullName}' has been activated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating customer {CustomerId}", id);
                TempData["ErrorMessage"] = $"Error activating customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Customers/Deactivate
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
                    // Kiểm tra xem user có role Customer không
                    var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Customer");
                    if (!hasCustomerRole)
                    {
                        return NotFound();
                    }

                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Customer '{user.FullName}' has been deactivated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating customer {CustomerId}", id);
                TempData["ErrorMessage"] = $"Error deactivating customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method
        private async Task<bool> CustomerExists(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AnyAsync(u => u.UserId == id && u.UserRoles.Any(ur => ur.Role.RoleName == "User"));
        }
    }
}

