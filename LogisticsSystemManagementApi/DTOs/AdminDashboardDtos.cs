namespace LogisticsSystemManagementApi.DTOs
{
    public class AdminDashboardDto
    {
        public AdminFinancialDto Financial { get; set; }
        public AdminOrdersDto Orders { get; set; }
        public AdminUsersDto Users { get; set; }
        public AdminOperationsDto Operations { get; set; }
    }


    public class AdminFinancialDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<decimal> WeeklyRevenue { get; set; }
    }


    public class AdminOrderStatusDto
    {
        public int Pending { get; set; }
        public int InTransit { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
    }


    public class AdminOrdersDto
    {
        public int TotalOrders { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersThisWeek { get; set; }
        public AdminOrderStatusDto AdminOrdersByStatus { get; set; }
        public List<int> OrderGrowth { get; set; }
    }


    public class AdminUsersDto
    {
        public int TotalCustomers { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalDispatchers { get; set; }
    }


    public class AdminOperationsDto
    {
        public int TotalShipments { get; set; }
        public int TotalTrips { get; set; }
        public int ActiveTrips { get; set; }
        public int CompletedTrips { get; set; }
    }
}



