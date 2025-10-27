using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Website_Cosmetics.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string username, string confirmationToken);
        Task SendPasswordResetAsync(string email, string username, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string username, string confirmationToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cosmetic Store", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress(username, email));
                message.Subject = "Account Confirmation - Cosmetic Store";

                var confirmationUrl = $"{_configuration["Email:BaseUrl"]}/Auth/ConfirmEmail?token={confirmationToken}&email={email}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Welcome to Cosmetic Store!</h2>
                        <p>Hello <strong>{username}</strong>,</p>
                        <p>Thank you for registering at Cosmetic Store. To complete your registration, please confirm your email by clicking the button below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationUrl}' 
                               style='background-color: #e91e63; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Confirm Email
                            </a>
                        </div>
                        
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #666;'>{confirmationUrl}</p>
                        
                        <p><strong>Note:</strong> This link will expire after 24 hours.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #666; font-size: 12px;'>
                            If you did not register this account, please ignore this email.<br>
                            Cosmetic Store - Where beauty meets nature
                        </p>
                    </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email confirmation sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email confirmation to {email}");
                throw;
            }
        }

        public async Task SendPasswordResetAsync(string email, string username, string resetToken)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cosmetic Store", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress(username, email));
                message.Subject = "Password Reset - Cosmetic Store";

                var resetUrl = $"{_configuration["Email:BaseUrl"]}/Auth/ResetPassword?token={resetToken}&email={email}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>Password Reset</h2>
                        <p>Hello <strong>{username}</strong>,</p>
                        <p>We received a request to reset your password. To reset your password, please click the button below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' 
                               style='background-color: #e91e63; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #666;'>{resetUrl}</p>
                        
                        <p><strong>Note:</strong> This link will expire after 1 hour.</p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #666; font-size: 12px;'>
                            If you did not request a password reset, please ignore this email.<br>
                            Cosmetic Store - Where beauty meets nature
                        </p>
                    </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Password reset email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                throw;
            }
        }
    }
}
