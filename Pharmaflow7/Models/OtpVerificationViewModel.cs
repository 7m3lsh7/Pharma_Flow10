using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class OtpVerificationViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;

        public bool CanResend { get; set; } = true;
        public int ResendCountdown { get; set; } = 0;
    }
}