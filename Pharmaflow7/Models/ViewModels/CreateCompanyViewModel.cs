using System.ComponentModel.DataAnnotations;

namespace PharmaFlow.Models.ViewModels
{
    public class CreateCompanyViewModel
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Company Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person name is required")]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Verify Account Immediately")]
        public bool VerifyImmediately { get; set; } = true;
    }
}
