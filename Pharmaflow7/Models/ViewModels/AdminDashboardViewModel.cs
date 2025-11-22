using System.Collections.Generic;

namespace PharmaFlow.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int TotalDistributors { get; set; }
        public int TotalDrugs { get; set; }
        public int TotalConsumers { get; set; }
        public List<CompanyListItemViewModel> RecentCompanies { get; set; } = new();
    }

    public class CompanyListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
