using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class Product
    {
        [Key]  
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime ProductionDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required]
        public string CompanyId { get; set; }
        public ApplicationUser Company { get; set; }
        public string Signature { get; set; } = string.Empty;
    }
}
