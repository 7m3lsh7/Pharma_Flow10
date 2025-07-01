using System;
using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class Driver
    {
        [Key]
        public int Id { get; set; } // Matches ApplicationUser.Id
        [Required]
        public string ApplicationUserId { get; set; } 
        [Required]

        public string LicenseNumber { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string FullName { get; set; } // Added FullName
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string DistributorId { get; set; }
        public virtual ApplicationUser Distributor { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public DateTime DateHired { get; set; }
    }
}