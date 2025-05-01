using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string DistributorId { get; set; }
        public string StoreName { get; set; }
        [Required, StringLength(200)]
        public string StoreAddress { get; set; }
        public ApplicationUser Distributor { get; set; }
    }
}
