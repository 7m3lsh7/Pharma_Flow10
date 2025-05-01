namespace Pharmaflow7.Models
{
    public class CompanyDashboardViewModel
    {

        public int TotalProducts { get; set; }
        public int ActiveShipments { get; set; }
        public int PerformanceScore { get; set; }
        public int DeliveredShipments { get; set; }
        public List<object> Shipments { get; set; }
        public List<object> SalesData { get; set; }
        public List<object> DistributionData { get; set; }

    }
}
