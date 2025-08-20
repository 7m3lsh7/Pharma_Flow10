namespace Pharmaflow7.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task SendOtpEmailAsync(string email, string otpCode);
    }
}