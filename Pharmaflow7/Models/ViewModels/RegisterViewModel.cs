using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "consumer";

        public string FullName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}