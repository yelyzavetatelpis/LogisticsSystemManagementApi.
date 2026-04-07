using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;


namespace LogisticsSystemManagementApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContext _context;


        public OrderRepository(DbContext context)
        {
            _context = context;
        }


        // insert a new order and return generated id
        public async Task<int> CreateOrderAsync(Order order)
        {
            var sql = @"INSERT INTO Orders
                        (CustomerId, PickupStreet, PickupCity, PickupPostalCode,
                         DeliveryStreet, DeliveryCity, DeliveryPostalCode,
                         PackageWeight, OrderDescription, PickupDate,
                         OrderStatusId, CreatedAt, Price)
                        VALUES
                        (@CustomerId, @PickupStreet, @PickupCity, @PickupPostalCode,
                         @DeliveryStreet, @DeliveryCity, @DeliveryPostalCode,
                         @PackageWeight, @OrderDescription, @PickupDate,
                         @OrderStatusId, @CreatedAt, @Price);
                        SELECT CAST(SCOPE_IDENTITY() AS INT)";


            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, order);


        }


        // get all orders placed by a specific customer with their current status
        public async Task<IEnumerable<MyOrdersDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var sql = @"SELECT
                            o.OrderId, o.PickupStreet, o.PickupCity, o.PickupPostalCode,
                            o.DeliveryStreet, o.DeliveryCity, o.DeliveryPostalCode,
                            o.PackageWeight, o.OrderDescription, o.PickupDate,
                            s.StatusName, o.CreatedAt, o.AdditionalNotes
                        FROM Orders o
                        INNER JOIN OrderStatus s ON o.OrderStatusId = s.OrderStatusId
                        WHERE o.CustomerId = @CustomerId
                        ORDER BY o.CreatedAt DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<MyOrdersDto>(sql, new { CustomerId = customerId });
        }
    }
}



