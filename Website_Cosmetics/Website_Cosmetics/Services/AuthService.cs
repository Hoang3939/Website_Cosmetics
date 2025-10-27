using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using BCrypt.Net;

namespace Website_Cosmetics.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string usernameOrEmail, string password);
        Task<User?> RegisterAsync(RegisterViewModel model);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string email, string newPassword);
        Task<bool> ConfirmEmailAsync(string token);
        Task<string> GeneratePasswordResetTokenAsync(Guid userId);
        Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<Role>> GetUserRolesAsync(Guid userId);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

                if (user == null || !user.IsActive)
                    return null;

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return null;

                // Update last login time
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {UsernameOrEmail}", usernameOrEmail);
                return null;
            }
        }

        public async Task<User?> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                    return null;

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    IsEmailConfirmed = false,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign default role (User)
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (userRole != null)
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserUID = user.UID,
                        RoleUID = userRole.UID
                    });
                    await _context.SaveChangesAsync();
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                return null;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null || !user.IsActive)
                    return false;

                var token = await GeneratePasswordResetTokenAsync(user.UID);
                
                // TODO: Send email with reset link
                // For now, just log the token (in production, send email)
                _logger.LogInformation("Password reset token for {Email}: {Token}", email, token);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string email, string newPassword)
        {
            try
            {
                var resetToken = await _context.PasswordResetTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token && rt.User.Email == email);

                if (resetToken == null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
                    return false;

                resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                resetToken.User.UpdatedAt = DateTime.UtcNow;
                resetToken.IsUsed = true;
                resetToken.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for {Email}", email);
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            try
            {
                var confirmationToken = await _context.EmailConfirmationTokens
                    .Include(ct => ct.User)
                    .FirstOrDefaultAsync(ct => ct.Token == token);

                if (confirmationToken == null || confirmationToken.IsUsed || confirmationToken.ExpiresAt < DateTime.UtcNow)
                    return false;

                confirmationToken.User.IsEmailConfirmed = true;
                confirmationToken.User.UpdatedAt = DateTime.UtcNow;
                confirmationToken.IsUsed = true;
                confirmationToken.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation");
                return false;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                UserUID = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // Token expires in 24 hours
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            var token = Guid.NewGuid().ToString();
            var confirmationToken = new EmailConfirmationToken
            {
                UserUID = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Token expires in 7 days
            };

            _context.EmailConfirmationTokens.Add(confirmationToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<Role>> GetUserRolesAsync(Guid userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserUID == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
        }
    }
}
