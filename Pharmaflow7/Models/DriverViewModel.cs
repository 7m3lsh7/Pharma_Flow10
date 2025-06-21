using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class DriverViewModel
    {
        public int Id { get; set; } // Matches Driver.Id, string
        [Required]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string LicenseNumber { get; set; }
        [Required]
        public string NationalId { get; set; }
        public DateTime DateHired { get; set; } = DateTime.Now;
    }
}

