namespace Pharmaflow7.Models
{
    public class ReportsViewModel
    {
        public List<SalesData> SalesPerformance { get; set; }
        public List<IssueData> Issues { get; set; }
        public List<DistributionData> DistributionPerformance { get; set; } // تغيير الاسم
        public List<ProductSalesData> TopProducts { get; set; }

        public class SalesData
        {
            public string Month { get; set; }
            public int Total { get; set; }
        }

        public class IssueData
        {
            public int Id { get; set; }
            public string ProductName { get; set; }
            public string IssueType { get; set; }
            public string ReportedBy { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }

        public class DistributionData
        {
            public string Destination { get; set; }
            public int Count { get; set; }
        }

        public class ProductSalesData
        {
            public string ProductName { get; set; }
            public int SalesCount { get; set; }
        }
    }
}