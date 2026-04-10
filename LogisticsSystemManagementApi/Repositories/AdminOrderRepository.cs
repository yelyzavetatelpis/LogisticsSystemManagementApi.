using Dapper;
using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    public class AdminOrderRepository : IAdminOrderRepository
    {
        private readonly Data.DbContext _context;


        public AdminOrderRepository(Data.DbContext context)
        {
            _context = context;
        }


        // get all orders with customer email and status
        public async Task<IEnumerable<AdminOrderResponseDto>> GetAllOrdersAsync()
        {
            var sql = @"
                SELECT
                    o.OrderId, o.CustomerId, o.PickupCity, o.PickupStreet,
                    o.PickupPostalCode, o.DeliveryStreet, o.DeliveryCity,
                    o.DeliveryPostalCode, o.PackageWeight, o.OrderDescription,
                    o.PickupDate, os.StatusName, u.Email
                FROM Orders o
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.OrderStatusId
                INNER JOIN Customers c ON o.CustomerId = c.CustomerId
                INNER JOIN Users u ON c.UserId = u.UserId
                ORDER BY o.CreatedAt DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<AdminOrderResponseDto>(sql);
        }


        // get orders filtered by email or date 
        public async Task<IEnumerable<AdminOrderResponseDto>> GetFilteredOrdersAsync(AdminOrderFilterDto filter)
        {
            var sql = @"
                SELECT
                    o.OrderId, u.Email, o.PickupCity, o.PickupStreet,
                    o.PickupPostalCode, o.DeliveryStreet, o.DeliveryCity,
                    o.DeliveryPostalCode, o.PackageWeight, o.OrderDescription,
                    o.PickupDate, os.StatusName, o.CreatedAt
                FROM Orders o
                INNER JOIN Customers c ON o.CustomerId = c.CustomerId
                INNER JOIN Users u ON c.UserId = u.UserId
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.OrderStatusId
                WHERE 1=1";


            var parameters = new DynamicParameters();


            if (!string.IsNullOrEmpty(filter.Email))
            {
                sql += " AND u.Email LIKE @Email";
                parameters.Add("Email", $"%{filter.Email}%");
            }


            if (filter.FromDate.HasValue)
            {
                sql += " AND o.CreatedAt >= @FromDate";
                parameters.Add("FromDate", filter.FromDate.Value);
            }


            if (filter.ToDate.HasValue)
            {
                sql += " AND o.CreatedAt <= @ToDate";
                parameters.Add("ToDate", filter.ToDate.Value);
            }


            sql += " ORDER BY o.CreatedAt DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<AdminOrderResponseDto>(sql, parameters);
        }
    }
}



