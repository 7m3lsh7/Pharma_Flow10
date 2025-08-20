using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using Pharmaflow7.Models;
using System.Security.Cryptography;

namespace Pharmaflow7.Services
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;

        public OtpService(AppDbContext context, IEmailService emailService, ILogger<OtpService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> GenerateOtpAsync(string email, string purpose = "EmailVerification")
        {
            // Clean up any existing OTPs for this email and purpose
            var existingOtps = await _context.EmailOtps
                .Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            _context.EmailOtps.RemoveRange(existingOtps);

            // Generate a 6-digit OTP
            var otpCode = GenerateSecureOtp();
            var expiryTime = DateTime.UtcNow.AddMinutes(10); // 10 minutes expiry

            var emailOtp = new EmailOtp
            {
                Email = email,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiryTime,
                Purpose = purpose
            };

            _context.EmailOtps.Add(emailOtp);
            await _context.SaveChangesAsync();

            // Send OTP via email
            try
            {
                await _emailService.SendOtpEmailAsync(email, otpCode);
                _logger.LogInformation("OTP generated and sent successfully to {Email} for {Purpose}", email, purpose);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email} for {Purpose}. Error: {Error}", email, purpose, ex.Message);
                // Remove the OTP from database since email failed
                _context.EmailOtps.Remove(emailOtp);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException($"Failed to send OTP email: {ex.Message}", ex);
            }

            return otpCode;
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose = "EmailVerification")
        {
            var emailOtp = await _context.EmailOtps
                .Where(o => o.Email == email && 
                           o.OtpCode == otpCode && 
                           o.Purpose == purpose && 
                           !o.IsUsed && 
                           o.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (emailOtp == null)
            {
                _logger.LogWarning("Invalid or expired OTP attempt for {Email}", email);
                return false;
            }

            // Mark OTP as used
            emailOtp.IsUsed = true;
            emailOtp.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("OTP validated successfully for {Email}", email);
            return true;
        }

        public async Task<bool> ResendOtpAsync(string email, string purpose = "EmailVerification")
        {
            // Check if there's a recent OTP (within last 2 minutes) to prevent spam
            var recentOtp = await _context.EmailOtps
                .Where(o => o.Email == email && 
                           o.Purpose == purpose && 
                           o.CreatedAt > DateTime.UtcNow.AddMinutes(-2))
                .FirstOrDefaultAsync();

            if (recentOtp != null)
            {
                _logger.LogWarning("OTP resend attempted too soon for {Email}", email);
                return false; // Too soon to resend
            }

            await GenerateOtpAsync(email, purpose);
            return true;
        }

        public async Task CleanupExpiredOtpsAsync()
        {
            var expiredOtps = await _context.EmailOtps
                .Where(o => o.ExpiresAt < DateTime.UtcNow || o.CreatedAt < DateTime.UtcNow.AddDays(-1))
                .ToListAsync();

            if (expiredOtps.Any())
            {
                _context.EmailOtps.RemoveRange(expiredOtps);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired OTPs", expiredOtps.Count);
            }
        }

        private string GenerateSecureOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
                return (randomNumber % 1000000).ToString("D6");
            }
        }
    }
}