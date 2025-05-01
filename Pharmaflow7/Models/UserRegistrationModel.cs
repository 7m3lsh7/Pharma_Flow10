using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class UserRegistrationModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "User type is required.")]
        public string RoleType { get; set; }

        public string FullName { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string LicenseNumber { get; set; }
        public string CompanyContactNumber { get; set; } // للشركة
        public string DistributorContactNumber { get; set; } // للموزع
        public string DistributorName { get; set; }
        public string WarehouseAddress { get; set; }
    }
}
