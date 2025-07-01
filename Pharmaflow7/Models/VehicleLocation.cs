using System;
using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class VehicleLocation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ShipmentId { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public virtual Shipment Shipment { get; set; }
    }
}