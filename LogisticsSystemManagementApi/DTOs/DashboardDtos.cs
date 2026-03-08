namespace LogisticsSystemManagementApi.DTOs
{
    // Data  for the customer dashboard
    public class CustomerDashboardDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int InTransitOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; }
        public List<RecentOrderDto> RecentDeliveredOrders { get; set; }
    }

    // Data for the dispatcher dashboard
    public class DispatcherDashboardDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int AvailableDriver { get; set; }
        public int AvailableVehicle { get; set; }
    }
}