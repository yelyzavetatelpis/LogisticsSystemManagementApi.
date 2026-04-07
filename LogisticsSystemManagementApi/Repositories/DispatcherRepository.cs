using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    public class DispatcherRepository : IDispatcherRepository
    {
        private readonly DbContext _context;


        public DispatcherRepository(DbContext context)
        {
            _context = context;
        }


        // statistics for the dispatcher dashboard
        public async Task<DispatcherDashboardDto> GetDashboardData()
        {
            using var connection = _context.CreateConnection();


            var dashboard = new DispatcherDashboardDto();


            dashboard.TotalOrders = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Orders");


            dashboard.PendingOrders = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM Orders o
                  JOIN OrderStatus s ON o.OrderStatusId = s.OrderStatusId
                  WHERE s.StatusName = 'Pending'");


            dashboard.AvailableDriver = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM Drivers d
                  JOIN DriverAvailabilityStatus ds ON d.DriverAvailabilityStatusId = ds.DriverAvailabilityStatusId
                  WHERE ds.StatusName = 'Available'");


            dashboard.AvailableVehicle = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) FROM Vehicles v
                  JOIN VehicleAvailabilityStatus vs ON v.VehicleAvailabilityStatusId = vs.VehicleAvailabilityStatusId
                  WHERE vs.StatusName = 'Available'");


            return dashboard;
        }


        // get trips planned within a given date range
        public async Task<IEnumerable<TripDto>> GetTripsByDate(DateTime fromDate, DateTime toDate)
        {
            var query = @"
                SELECT
                    t.TripId, t.DriverId, t.VehicleId,
                    s.StatusName, t.PlannedDeparture
                FROM Trips t
                INNER JOIN TripStatus s ON t.TripStatusId = s.TripStatusId
                WHERE CAST(t.PlannedDeparture AS DATE) BETWEEN @FromDate AND @ToDate
                ORDER BY t.PlannedDeparture DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TripDto>(query, new
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            });
        }
    }
}



