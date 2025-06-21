using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pharmaflow7.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; } // Changed to int, auto-incremented primary key

        public string ApplicationUserId { get; set; } // Remains string, foreign key to ApplicationUser.Id
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [Display(Name = "رقم الرخصة")]
        public string LicenseNumber { get; set; }

        [Required]
        [Display(Name = "رقم البطاقة")]
        public string NationalId { get; set; }

        [Display(Name = "الشركة الموزعة")]
        public string? DistributorId { get; set; } // Remains string?, foreign key to ApplicationUser.Id
        public virtual ApplicationUser? Distributor { get; set; }

        [Display(Name = "تاريخ التعيين")]
        public DateTime DateHired { get; set; } = DateTime.Now;
    }
}