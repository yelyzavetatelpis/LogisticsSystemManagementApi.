using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContext _context;

        // Order status id numbers from the database
        private const int StatusPending = 7;
        private const int StatusAccepted = 8;
        private const int StatusInTransit = 10;
        private const int StatusDelivered = 11;
        private const int StatusRejected = 12;

        // Shipment status id number from the database
        private const int ShipmentStatusReady = 1;

        public OrderRepository(DbContext context)
        {
            _context = context;
        }

        // Get the customer id linked to a user id
        public async Task<int> GetCustomerIdByUserIdAsync(int userId)
        {
            var sql = "SELECT CustomerId FROM Customers WHERE UserId = @UserId";
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
            }
        }

        // Insert a new order in the database
        public async Task CreateOrderAsync(Order order)
        {
            var sql = @"INSERT INTO Orders
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
                await connection.ExecuteAsync(sql, order);
            }
        }

        // Return order summary counts and recent orders for customer dashboard
        public async Task<CustomerDashboardDto> GetCustomerDashboardDataAsync(int customerId)
        {
            using (var connection = _context.CreateConnection())
            {
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

                // 5 most recent orders for the dashboard 
                var recentSql = @"SELECT TOP 5
                                    o.OrderId, o.DeliveryCity, o.PackageWeight,
                                    o.PickupCity, o.PickupDate, s.StatusName
                                  FROM Orders o
                                  INNER JOIN OrderStatus s ON o.OrderStatusId = s.OrderStatusId
                                  WHERE o.CustomerId = @CustomerId
                                  ORDER BY o.CreatedAt DESC";

                dashboard.RecentOrders = (await connection.QueryAsync<RecentOrderDto>(
                    recentSql, new { CustomerId = customerId })).ToList();

                // 5 recently delivered orders
                var deliveredSql = @"SELECT TOP 5
                                       OrderId, DeliveryCity, PackageWeight, PickupDate
                                     FROM Orders
                                     WHERE CustomerId = @CustomerId AND OrderStatusId = @Status
                                     ORDER BY CreatedAt DESC";

                dashboard.RecentDeliveredOrders = (await connection.QueryAsync<RecentOrderDto>(
                    deliveredSql, new { CustomerId = customerId, Status = StatusDelivered })).ToList();

                return dashboard;
            }
        }

        // Return all orders the customer placed and their current status
        public async Task<IEnumerable<MyOrdersDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var sql = @"SELECT
                            o.OrderId, o.PickupStreet, o.PickupCity, o.PickupPostalCode,
                            o.DeliveryStreet, o.DeliveryCity, o.DeliveryPostalCode,
                            o.PackageWeight, o.OrderDescription, o.PickupDate, s.StatusName
                        FROM Orders o
                        INNER JOIN OrderStatus s ON o.OrderStatusId = s.OrderStatusId
                        WHERE o.CustomerId = @CustomerId
                        ORDER BY o.CreatedAt DESC";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<MyOrdersDto>(sql, new { CustomerId = customerId });
            }
        }
        // --- Dispatcher-Related operations ---

        // Accept a pending order and create a shipment for it
        public async Task AcceptOrderAsync(int orderId)
        {
            using (var connection = _context.CreateConnection())
            {
                // Update the order status to Accepted
                var updateSql = "UPDATE Orders SET OrderStatusId = @Status WHERE OrderId = @OrderId";
                await connection.ExecuteAsync(updateSql, new { Status = StatusAccepted, OrderId = orderId });

                // Only create a shipment if one doesn't already exist
                var existsSql = "SELECT COUNT(*) FROM Shipments WHERE OrderId = @OrderId";
                var shipmentExists = await connection.ExecuteScalarAsync<int>(existsSql, new { OrderId = orderId });

                if (shipmentExists == 0)
                {
                    var shipmentSql = @"INSERT INTO Shipments (OrderId, ShipmentStatusId, CreatedAt)
                                        VALUES (@OrderId, @ShipmentStatusId, @CreatedAt)";
                    await connection.ExecuteAsync(shipmentSql, new
                    {
                        OrderId = orderId,
                        ShipmentStatusId = ShipmentStatusReady,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // all orders that are waiting to be reviewed by a dispatcher
        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            var sql = "SELECT * FROM Orders WHERE OrderStatusId = @Status";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Order>(sql, new { Status = StatusPending });
                return result.ToList();
            }
        }

        // Reject an order and provide the reason
        public async Task RejectOrderAsync(int orderId, string reason)
        {
            var sql = @"UPDATE Orders
                        SET OrderStatusId = @Status, RejectReason = @Reason
                        WHERE OrderId = @OrderId";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { Status = StatusRejected, Reason = reason, OrderId = orderId });
            }
        }

        //  all shipments ready to be assigned to a trip
        public async Task<List<Shipment>> GetShipmentsAsync()
        {
            var sql = "SELECT * FROM Shipments WHERE ShipmentStatusId = @Status";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Shipment>(sql, new { Status = ShipmentStatusReady });
                return result.ToList();
            }
        }
    }
}