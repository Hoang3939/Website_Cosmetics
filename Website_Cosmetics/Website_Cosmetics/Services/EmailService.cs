using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Website_Cosmetics.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string username, string confirmationToken);
        Task SendPasswordResetAsync(string email, string username, string resetToken);
        Task SendOrderStatusUpdateAsync(string email, string username, string orderNumber, string status, decimal total, string? shippingAddress = null);
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

        public async Task SendOrderStatusUpdateAsync(string email, string username, string orderNumber, string status, decimal total, string? shippingAddress = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Cosmetic Store", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress(username, email));

                string subject = "";
                string statusMessage = "";
                string statusDescription = "";

                switch (status.ToLower())
                {
                    case "processing":
                        subject = "Order Confirmation - Your Order is Being Processed";
                        statusMessage = "Order Confirmed and Processing";
                        statusDescription = "Your order has been successfully placed and is currently being processed.";
                        break;
                    case "cancelled":
                        subject = "Order Cancellation Notice";
                        statusMessage = "Order Cancelled";
                        statusDescription = "Your order was not successful and has been cancelled.";
                        break;
                    case "shipped":
                        subject = "Your Order Has Been Shipped";
                        statusMessage = "Order Shipped";
                        statusDescription = "Your order has been handed over to the shipping carrier and is on its way to you.";
                        break;
                    case "delivered":
                        subject = "Order Delivered Successfully";
                        statusMessage = "Order Delivered";
                        statusDescription = "Your order has been delivered successfully. Thank you for your purchase!";
                        break;
                    default:
                        subject = "Order Status Update";
                        statusMessage = $"Order Status: {status}";
                        statusDescription = $"Your order status has been updated to: {status}";
                        break;
                }

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                            <h2 style='color: #333; margin-top: 0;'>{statusMessage}</h2>
                            <p style='color: #666; font-size: 16px;'>{statusDescription}</p>
                        </div>
                        
                        <div style='background-color: #ffffff; border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>
                            <h3 style='color: #333; margin-top: 0;'>Order Details</h3>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Order Number:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>{orderNumber}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Status:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'><strong>{status}</strong></td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Total Amount:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'><strong>${total:F2}</strong></td>
                                </tr>
                                {(string.IsNullOrWhiteSpace(shippingAddress) ? "" : $@"
                                <tr>
                                    <td style='padding: 8px 0; color: #666;'><strong>Shipping Address:</strong></td>
                                    <td style='padding: 8px 0; color: #333;'>{shippingAddress}</td>
                                </tr>")}
                            </table>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{_configuration["Email:BaseUrl"]}/Orders/Details?orderNumber={orderNumber}' 
                               style='background-color: #e91e63; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                View Order Details
                            </a>
                        </div>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                        <p style='color: #666; font-size: 12px;'>
                            If you have any questions about your order, please contact our customer service.<br>
                            Cosmetic Store - Where beauty meets nature
                        </p>
                    </div>";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], int.Parse(_configuration["Email:SmtpPort"] ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Order status update email sent to {email} for order {orderNumber} with status {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send order status update email to {email} for order {orderNumber}");
                throw;
            }
        }
    }
}
