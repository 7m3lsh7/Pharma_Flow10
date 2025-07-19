namespace Pharmaflow7.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string IssueType { get; set; }
        public string Details { get; set; }
        public string DistributorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
