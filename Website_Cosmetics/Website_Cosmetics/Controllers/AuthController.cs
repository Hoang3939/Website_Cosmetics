using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Website_Cosmetics.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Website_Cosmetics.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ApplicationDbContext context, IEmailService emailService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.LoginAsync(model.UsernameOrEmail, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (!user.IsEmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add roles
            var roles = await _authService.GetUserRolesAsync(user.UID);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect based on user role
            var userRoles = await _authService.GetUserRolesAsync(user.UID);
            if (userRoles.Any(r => r.RoleName == "Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (userRoles.Any(r => r.RoleName == "Staff"))
            {
                return RedirectToAction("Index", "StaffDashboard", new { area = "Admin" });
            }
            else
            {
                // Regular users go to products page
                return RedirectToAction("Index", "Products");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.RegisterAsync(model);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username or email already exists.");
                return View(model);
            }

            // Generate email confirmation token
            var token = await _authService.GenerateEmailConfirmationTokenAsync(user.UID);
            
            // Send confirmation email
            try
            {
                await _emailService.SendEmailConfirmationAsync(user.Email, user.Username, token);
                TempData["SuccessMessage"] = "Registration successful! Please check your email to confirm your account.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                TempData["SuccessMessage"] = "Registration successful! However, there was an error sending confirmation email. Please contact admin.";
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirmed()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Invalid token or email.";
                return RedirectToAction("Login");
            }

            var result = await _authService.ConfirmEmailAsync(token);
            if (result)
            {
                TempData["SuccessMessage"] = "Email confirmed successfully! You can now log in.";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired token. Please register again.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ForgotPasswordAsync(model.Email);
            if (result)
            {
                // Get user to send reset email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var resetToken = await _authService.GeneratePasswordResetTokenAsync(user.UID);
                    try
                    {
                        await _emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken);
                        TempData["SuccessMessage"] = "Password reset link has been sent to your email.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                        TempData["ErrorMessage"] = "Error sending email. Please try again later.";
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "No account found with this email.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            return View(new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.ResetPasswordAsync(model.Token, model.Email, model.Password);
            if (result)
            {
                TempData["SuccessMessage"] = "Password reset successfully! You can now log in with your new password.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired password reset link.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult GenerateHash(string password = "Admin123!")
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
            return Content($"Password: {password}\nHash: {hash}", "text/plain");
        }
    }
}
