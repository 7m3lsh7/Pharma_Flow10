using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class ShipmentViewModel
    {
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required, StringLength(100, MinimumLength = 2)]
        public string Destination { get; set; }
        public string? DistributorId { get; set; } // اختياري (nullable)
        public int? StoreId { get; set; } // اختياري (nullable بالفعل)
        public string? Status { get; set; } // اختياري (nullable)
        public List<Product>? Products { get; set; } // اختياري (nullable)
        public IList<ApplicationUser>? Distributors { get; set; } // اختياري (nullable)
        public List<Store>? Stores { get; set; } // اختياري (nullable)
        public string? ProductName { get; set; } // اختياري (nullable)
        public string? StoreAddress { get; set; } // اختياري (nullable)
        public string? CurrentLocation { get; set; } // اختياري (nullable)
        public string? DistributorName { get; set; } // اختياري (nullable)
        public bool? IsAcceptedByDistributor { get; set; } // nullable بالفعل
    }
}