using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.Models;
using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Repositories
{
    public class DispatcherRepository
    {
        private readonly DbContext _context;

        public DispatcherRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<int> GetCustomerIdByUserId(int userId)
        {
            var query = "SELECT CustomerId FROM Customers WHERE UserId = @UserId";

            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<int>(query, new { UserId = userId });
            }
        }

        public async Task CreateOrderAsync(Order order)
        {
            var query = @"
                INSERT INTO Orders
                (CustomerId, PickupStreet, PickupCity, PickupPostalCode,
                 DeliveryStreet, DeliveryCity, DeliveryPostalCode,
                 PackageWeight, OrderDescription, PickupDate,
                 OrderStatusId, CreatedAt)
                VALUES
                (@CustomerId, @PickupStreet, @PickupCity, @PickupPostalCode,
                 @DeliveryStreet, @DeliveryCity, @DeliveryPostalCode,
                 @PackageWeight, @OrderDescription, @PickupDate,
                 @OrderStatusId, @CreatedAt)";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, order);
            }
        }
        public async Task<CustomerDashboardDto> GetCustomerDashboardData(int customerId)
        {
            using (var connection = _context.CreateConnection())
            {
                var dashboard = new CustomerDashboardDto();

                // Total Orders
                dashboard.TotalOrders = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Orders WHERE CustomerId = @CustomerId",
                    new { CustomerId = customerId });

                // Pending Orders
                dashboard.PendingOrders = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM Orders 
                      WHERE CustomerId = @CustomerId AND OrderStatusId = 7",
                    new { CustomerId = customerId });

                // In Transit
                dashboard.InTransitOrders = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM Orders 
                      WHERE CustomerId = @CustomerId AND OrderStatusId = 10",
                    new { CustomerId = customerId });

                // Delivered
                dashboard.DeliveredOrders = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM Orders 
                      WHERE CustomerId = @CustomerId AND OrderStatusId = 11",
                    new { CustomerId = customerId });

                // Recent Orders
                var recentOrdersQuery = @"
                    SELECT TOP 5
                        o.OrderId,
                        o.DeliveryCity,
                        o.PackageWeight,
                        o.PickupCity,
                        o.PickupDate,
                        s.StatusName
                    FROM Orders o
                    INNER JOIN OrderStatus s 
                        ON o.OrderStatusId = s.OrderStatusId
                    WHERE o.CustomerId = @CustomerId
                    ORDER BY o.CreatedAt DESC";

                dashboard.RecentOrders = (await connection.QueryAsync<RecentOrderDto>(
                    recentOrdersQuery,
                    new { CustomerId = customerId })).ToList();

                var deliveredQuery = @"
                    SELECT TOP 5
                    OrderId,
                    DeliveryCity,
                    PackageWeight,
                    PickupDate
                    FROM Orders
                    WHERE CustomerId = @CustomerId
                    AND OrderStatusId = 11
                    ORDER BY CreatedAt DESC";

                dashboard.RecentDeliveredOrders = (await connection.QueryAsync<RecentOrderDto>(
                    deliveredQuery,
                    new { CustomerId = customerId }
                )).ToList();

                return dashboard;
            }

        }
        public async Task<IEnumerable<MyOrdersDto>> GetOrdersByCustomerId(int customerId)
        {
            var query = @"
        SELECT 
            o.OrderId,
            o.PickupStreet,
            o.PickupCity,
            o.PickupPostalCode,
            o.DeliveryStreet,
            o.DeliveryCity,
            o.DeliveryPostalCode,
            o.PackageWeight,
            o.OrderDescription,
            o.PickupDate,
            s.StatusName
        FROM Orders o
        INNER JOIN OrderStatus s
            ON o.OrderStatusId = s.OrderStatusId
        WHERE o.CustomerId = @CustomerId
        ORDER BY o.CreatedAt DESC";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<MyOrdersDto>(query, new { CustomerId = customerId });
            }
        }

    }
}