using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    public class DispatcherRepository
    {
        private readonly LogisticsSystemManagementApi.Data.DbContext _context;

        public DispatcherRepository(LogisticsSystemManagementApi.Data.DbContext context)
        {
            _context = context;
        }
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
                        join DriverAvailabilityStatus ds on d.DriverAvailabilityStatusId = ds.DriverAvailabilityStatusId
                        where StatusName = 'Available'");

            dashboard.AvailableVehicle = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*) FROM Vehicles v 
                        join VehicleAvailabilityStatus vs on v.VehicleAvailabilityStatusId = vs.VehicleAvailabilityStatusId
                        where StatusName = 'Available'");

            return dashboard;
        }
    }
}