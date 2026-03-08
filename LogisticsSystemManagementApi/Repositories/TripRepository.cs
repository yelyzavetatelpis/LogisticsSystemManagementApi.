using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;

public class TripRepository
{
    private readonly DbContext _context;

    public TripRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TripDto>> GetTripsAsync()
    {
        var query = @"
        SELECT 
            t.TripId,
            t.DriverId,
            t.VehicleId,
            s.StatusName,
            t.PlannedDeparture
        FROM Trips t
        INNER JOIN TripStatus s
            ON t.TripStatusId = s.TripStatusId
        ORDER BY t.CreatedAt DESC";

        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryAsync<TripDto>(query);
        }
    }

    public async Task<IEnumerable<TripShipmentDto>> GetTripShipments(int tripId)
    {
        var query = @"
        SELECT 
            s.ShipmentId,
            s.OrderId,
            o.PickupCity,
            o.DeliveryCity,
            o.PackageWeight,
            ss.StatusName,
            t.CreatedAt
            
        FROM TripShipments ts
        INNER JOIN Shipments s
            ON ts.ShipmentId = s.ShipmentId
        INNER JOIN Orders o
            ON s.OrderId = o.OrderId
        INNER join ShipmentStatus ss
            on s.ShipmentStatusId = ss.ShipmentStatusId
        Inner join Trips t
			on ts.TripId = t.TripId
        WHERE ts.TripId = @TripId";

        using (var connection = _context.CreateConnection())
        {
            return await connection.QueryAsync<TripShipmentDto>(
                query,
                new { TripId = tripId }
            );
        }
    }
}