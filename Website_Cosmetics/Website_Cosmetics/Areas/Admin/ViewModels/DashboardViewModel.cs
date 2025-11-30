using Website_Cosmetics.Models;

namespace Website_Cosmetics.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Cards
        public decimal RevenueToday { get; set; }
        public decimal RevenueYesterday { get; set; }
        public decimal RevenueChangePercent { get; set; }
        
        public int TotalOrders { get; set; }
        public int OrdersYesterday { get; set; }
        public decimal OrdersChangePercent { get; set; }
        
        public int NewUsersToday { get; set; }
        public int NewUsersYesterday { get; set; }
        public decimal NewUsersChangePercent { get; set; }
        
        public int LowStockCount { get; set; }
        
        // Recent Orders
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        
        // Revenue Chart Data (Monthly)
        public List<DailyRevenue> MonthlyRevenue { get; set; } = new List<DailyRevenue>();
    }
    
    public class DailyRevenue
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}

