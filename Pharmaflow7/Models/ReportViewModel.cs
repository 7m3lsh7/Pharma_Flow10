using System.ComponentModel.DataAnnotations;

namespace Pharmaflow7.Models
{
    public class ReportViewModel
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string IssueType { get; set; }
        [Required]
        public string Details { get; set; }
    }
}
