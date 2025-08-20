using System.ComponentModel.DataAnnotations;
using Pharmaflow7.Attributes;

namespace Pharmaflow7.Models
{
    public class LoginViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [ValidEmailDomain]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
