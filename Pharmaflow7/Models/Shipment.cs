using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public string Destination { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Required]
        public string CompanyId { get; set; }
        public string DistributorId { get; set; }
        public int? StoreId { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual ApplicationUser Distributor { get; set; }
        public bool? IsAcceptedByDistributor { get; set; }
        public Store Store { get; set; }
    }
}
