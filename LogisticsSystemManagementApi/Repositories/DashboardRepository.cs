using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using System.Data;


namespace LogisticsSystemManagementApi.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DbContext _context;


        public DashboardRepository(DbContext context)
        {
            _context = context;
        }


        public async Task<AdminDashboardDto> GetDashboardMetricsAsync()
        {
            using var connection = _context.CreateConnection();


            var dashboard = new AdminDashboardDto();


            // --- Financial metrics ---
            var financial = await connection.QueryFirstAsync<AdminFinancialDto>(@"
                SELECT 
                    ISNULL(SUM(Price),0) AS TotalRevenue,
                    ISNULL(SUM(CASE 
                        WHEN MONTH(CreatedAt) = MONTH(GETDATE()) 
                        AND YEAR(CreatedAt) = YEAR(GETDATE()) 
                        THEN Price END),0) AS RevenueThisMonth,
                    ISNULL(SUM(CASE 
                        WHEN CreatedAt >= DATEADD(DAY, -7, GETDATE()) 
                        THEN Price END),0) AS RevenueThisWeek,
                    ISNULL(AVG(Price),0) AS AverageOrderValue
                FROM Orders
            ");


            var weeklyRevenue = (await connection.QueryAsync<decimal>(@"
                SELECT TOP 4 ISNULL(SUM(Price),0) AS Revenue
                FROM Orders
                WHERE CreatedAt >= DATEADD(WEEK, -4, GETDATE())
                GROUP BY DATEPART(WEEK, CreatedAt)
                ORDER BY DATEPART(WEEK, CreatedAt)
            ")).ToList();


            while (weeklyRevenue.Count < 4)
                weeklyRevenue.Insert(0, 0);


            financial.WeeklyRevenue = weeklyRevenue;


            // --- Order counts ---
            var orders = await connection.QueryFirstAsync<AdminOrdersDto>(@"
                SELECT 
                    COUNT(*) AS TotalOrders,
                    COUNT(CASE 
                        WHEN MONTH(CreatedAt) = MONTH(GETDATE()) 
                        AND YEAR(CreatedAt) = YEAR(GETDATE()) 
                        THEN 1 END) AS OrdersThisMonth,
                    COUNT(CASE 
                        WHEN CreatedAt >= DATEADD(DAY, -7, GETDATE()) 
                        THEN 1 END) AS OrdersThisWeek
                FROM Orders
            ");


            // --- Order status  ---
            var orderStatus = await connection.QueryFirstAsync<AdminOrderStatusDto>(@"
    SELECT 
        COUNT(CASE WHEN OrderStatusId = 7 THEN 1 END) AS Pending,
        COUNT(CASE WHEN OrderStatusId IN (8,9,10) THEN 1 END) AS InTransit,
        COUNT(CASE WHEN OrderStatusId = 11 THEN 1 END) AS Delivered,
        COUNT(CASE WHEN OrderStatusId IN (12,13) THEN 1 END) AS Cancelled
    FROM Orders
");


            orders.AdminOrdersByStatus = orderStatus;


            // --- Order growth ---
            var orderGrowth = (await connection.QueryAsync<int>(@"
                SELECT TOP 4 COUNT(*) AS Count
                FROM Orders
                WHERE CreatedAt >= DATEADD(WEEK, -4, GETDATE())
                GROUP BY DATEPART(WEEK, CreatedAt)
                ORDER BY DATEPART(WEEK, CreatedAt)
            ")).ToList();


            while (orderGrowth.Count < 4)
                orderGrowth.Insert(0, 0);


            orders.OrderGrowth = orderGrowth;


            // --- User counts ---
            var users = await connection.QueryFirstAsync<AdminUsersDto>(@"
                SELECT 
                    (SELECT COUNT(*) FROM Users) AS TotalCustomers,
                    (SELECT COUNT(*) FROM Drivers) AS TotalDrivers,
                    (SELECT COUNT(*) FROM Dispatchers) AS TotalDispatchers
            ");


            // --- Operations summary ---
            var operations = await connection.QueryFirstAsync<AdminOperationsDto>(@"
                SELECT 
                    (SELECT COUNT(*) FROM Shipments) AS TotalShipments,
                    (SELECT COUNT(*) FROM Trips) AS TotalTrips,
                    (SELECT COUNT(*) FROM Trips WHERE TripStatusId = 1) AS ActiveTrips,
                    (SELECT COUNT(*) FROM Trips WHERE TripStatusId = 2) AS CompletedTrips
            ");


            dashboard.Financial = financial;
            dashboard.Orders = orders;
            dashboard.Users = users;
            dashboard.Operations = operations;


            return dashboard;
        }
    }
}



