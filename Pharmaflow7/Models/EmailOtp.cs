using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class EmailOtp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Purpose { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }

}