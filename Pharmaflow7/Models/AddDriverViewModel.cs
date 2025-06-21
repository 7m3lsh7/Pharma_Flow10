using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class AddDriverViewModel
    {
        public int Id { get; set; } // Should be string to match Driver.Id, but lowercase 'id' is unconventional
        [Required]
        public string LicenseNumber { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Phone]
        public string ContactNumber { get; set; }
    }
}
