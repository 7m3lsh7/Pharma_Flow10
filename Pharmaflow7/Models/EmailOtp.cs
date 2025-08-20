using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class EmailOtp
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? UsedAt { get; set; }
        
        public string Purpose { get; set; } = "EmailVerification"; // EmailVerification, PasswordReset, etc.
    }
}