using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    public class CustomerDashboardRepository : ICustomerDashboardRepository
    {
        private readonly DbContext _context;


        private const int StatusPending = 7;
        private const int StatusInTransit = 10;
        private const int StatusDelivered = 11;


        public CustomerDashboardRepository(DbContext context)
        {
            _context = context;
        }


        // get the customer id linked to user
        public async Task<int> GetCustomerIdByUserIdAsync(int userId)
        {
            var sql = "SELECT CustomerId FROM Customers WHERE UserId = @UserId";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }


        // return order counts for thre customer dashboard
        public async Task<CustomerDashboardDto> GetCustomerDashboardDataAsync(int customerId)
        {
            using var connection = _context.CreateConnection();


            var dashboard = new CustomerDashboardDto();


            dashboard.TotalOrders = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId",
                new { CustomerId = customerId });


            dashboard.PendingOrders = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId AND OrderStatusId = @Status",
                new { CustomerId = customerId, Status = StatusPending });


            dashboard.InTransitOrders = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId AND OrderStatusId = @Status",
                new { CustomerId = customerId, Status = StatusInTransit });


            dashboard.DeliveredOrders = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId AND OrderStatusId = @Status",
                new { CustomerId = customerId, Status = StatusDelivered });


            // 5 most recent orders
            var recentSql = @"
                SELECT TOP 5
                    o.OrderId, o.DeliveryCity, o.PackageWeight,
                    o.PickupCity, o.PickupDate, s.StatusName
                FROM Orders o
                INNER JOIN OrderStatus s ON o.OrderStatusId = s.OrderStatusId
                WHERE o.CustomerId = @CustomerId
                ORDER BY o.CreatedAt DESC";


            dashboard.RecentOrders = (await connection.QueryAsync<RecentOrderDto>(
                recentSql, new { CustomerId = customerId })).ToList();


            // 5 most recently delivered orders
            var deliveredSql = @"
                SELECT TOP 5
                    OrderId, DeliveryCity, PackageWeight, PickupDate
                FROM Orders
                WHERE CustomerId = @CustomerId AND OrderStatusId = @Status
                ORDER BY CreatedAt DESC";


            dashboard.RecentDeliveredOrders = (await connection.QueryAsync<RecentOrderDto>(
                deliveredSql, new { CustomerId = customerId, Status = StatusDelivered })).ToList();


            return dashboard;
        }
    }
}



