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
        public string? DistributorId { get; set; }
        public int? StoreId { get; set; }
        public string? Status { get; set; }
        public int? DriverId { get; set; }
        public List<Product>? Products { get; set; }
        public IList<ApplicationUser>? Distributors { get; set; }
        public List<Store>? Stores { get; set; }
        public string? ProductName { get; set; }
        public string? StoreAddress { get; set; }
        public string? CurrentLocation { get; set; }
        public string? DistributorName { get; set; }
        public bool? IsAcceptedByDistributor { get; set; }
        public double? Latitude { get; set; } // Changed to double?
        public double? Longitude { get; set; } // Changed to double?
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public string? DriverFullName { get; set; }
        public string? DriverName { get; set; }
    }
}