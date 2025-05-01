using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public DateTime ProductionDate { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required, StringLength(100, MinimumLength = 2)]
        public string Description { get; set; }
    }
}
