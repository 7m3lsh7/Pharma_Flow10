namespace Pharmaflow7.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, string purpose = "EmailVerification");
        Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose = "EmailVerification");
        Task<bool> ResendOtpAsync(string email, string purpose = "EmailVerification");
        Task CleanupExpiredOtpsAsync();
    }
}