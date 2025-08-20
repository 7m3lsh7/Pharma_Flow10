using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Pharmaflow7.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm your email address - PharmaFlow";
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                        <h1>PharmaFlow</h1>
                    </div>
                    <div style='padding: 20px; background-color: #f8f9fa;'>
                        <h2>Confirm Your Email Address</h2>
                        <p>Thank you for registering with PharmaFlow. To complete your registration, please confirm your email address by clicking the button below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Confirm Email Address
                            </a>
                        </div>
                        
                        <p>If the button doesn't work, you can copy and paste the following link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{confirmationLink}</p>
                        
                        <p><strong>Important:</strong> You will not be able to log in to your account until your email address is confirmed.</p>
                        
                        <hr style='margin: 30px 0;'>
                        <p style='color: #666; font-size: 12px;'>
                            If you didn't create an account with PharmaFlow, please ignore this email.
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(email, subject, htmlContent);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Reset your password - PharmaFlow";
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                        <h1>PharmaFlow</h1>
                    </div>
                    <div style='padding: 20px; background-color: #f8f9fa;'>
                        <h2>Reset Your Password</h2>
                        <p>You requested to reset your password. Click the button below to create a new password:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #dc3545; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <p>If the button doesn't work, you can copy and paste the following link into your browser:</p>
                        <p style='word-break: break-all; color: #007bff;'>{resetLink}</p>
                        
                        <p><strong>Note:</strong> This link will expire in 24 hours for security reasons.</p>
                        
                        <hr style='margin: 30px 0;'>
                        <p style='color: #666; font-size: 12px;'>
                            If you didn't request a password reset, please ignore this email.
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(email, subject, htmlContent);
        }
    }
}