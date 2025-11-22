using System.ComponentModel.DataAnnotations;

namespace PharmaFlow.Models.ViewModels
{
    public class EditCompanyViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Account Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Account Verified")]
        public bool IsVerified { get; set; }
    }
}
