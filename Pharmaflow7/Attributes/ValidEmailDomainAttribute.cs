using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Pharmaflow7.Attributes
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        public ValidEmailDomainAttribute()
        {
            ErrorMessage = "Email must be a valid format ending with a proper domain (e.g., .com, .org, .net)";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            string email = value.ToString()!;
            
            // Check if email matches proper format with valid domain extension
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            
            if (!Regex.IsMatch(email, emailPattern))
                return false;

            // Additional check to ensure it doesn't end with incomplete domains like @gmail
            var incompleteDomainPattern = @"@[a-zA-Z0-9.-]+(?<!\.[a-zA-Z]{2,})$";
            if (Regex.IsMatch(email, incompleteDomainPattern))
                return false;

            return true;
        }
    }
}