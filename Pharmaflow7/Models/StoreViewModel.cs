using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class StoreViewModel
    {
        [Required, StringLength(100)]
        public string StoreName { get; set; }

        [Required, StringLength(200)]
        public string StoreAddress { get; set; }

    }
}
