using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Remove fields that shouldn't be validated from form
            ModelState.Remove("UserId");
            ModelState.Remove("Username");
            ModelState.Remove("Email");
            ModelState.Remove("PasswordHash");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("IsEmailConfirmed");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only allowed fields
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                    TempData["ErrorMessage"] = "An error occurred while updating your profile. Please try again.";
                }
            }

            return View(user);
        }

        // GET: /Account/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.User = user;
            return View();
        }

        // POST: /Account/Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                return Json(new { success = false, message = "Please login" });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Validate passwords
            if (string.IsNullOrWhiteSpace(currentPassword) || 
                string.IsNullOrWhiteSpace(newPassword) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                return Json(new { success = false, message = "All password fields are required" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "New password and confirm password do not match" });
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters long" });
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            try
            {
                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", userId);

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }
    }
}

