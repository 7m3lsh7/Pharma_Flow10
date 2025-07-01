using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class AddDriverViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب.")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "رقم الرخصة مطلوب.")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "رقم البطاقة الشخصية مطلوب.")]
        public string NationalId { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير ورقم على الأقل.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}