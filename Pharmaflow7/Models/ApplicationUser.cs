using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(200)]
        public string? CompanyName { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public string? CreatedByAdminId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedDate { get; set; }

        [StringLength(50)]
        public string? UserType { get; set; } // Admin, Company, Distributor, Consumer
        public string RoleType { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? LicenseNumber { get; set; }
        public string? ContactNumber { get; set; }
        public string? DistributorName { get; set; }
        public string? WarehouseAddress { get; set; }
    }
}
