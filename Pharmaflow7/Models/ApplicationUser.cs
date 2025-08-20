using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class ApplicationUser : IdentityUser
    {
       

        public string RoleType { get; set; } = string.Empty;  
        public string? FullName { get; set; }  
        public string? Address { get; set; }  
        public string? CompanyName { get; set; }   
        public string? LicenseNumber { get; set; }  
        public string? ContactNumber { get; set; } 
        public string? DistributorName { get; set; }  
        public string? WarehouseAddress { get; set; }  

    }
}
