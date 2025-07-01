namespace Pharmaflow7.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public ApplicationUser User { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
    }
}
