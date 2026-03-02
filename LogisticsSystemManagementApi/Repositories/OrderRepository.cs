using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    public class OrderRepository
    {
        private readonly DbContext _context;

        public OrderRepository(DbContext context)
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
    }
}