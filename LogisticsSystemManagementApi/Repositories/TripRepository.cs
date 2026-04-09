using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;
using System.Data;


namespace LogisticsSystemManagementApi.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly DbContext _context;


        public TripRepository(DbContext context)
        {
            _context = context;
        }


        // get all trips with their current status
        public async Task<IEnumerable<TripDto>> GetTripsAsync()
        {
            var query = @"
                SELECT
                    t.TripId, t.DriverId, t.VehicleId,
                    s.StatusName, t.PlannedDeparture
                FROM Trips t
                INNER JOIN TripStatus s ON t.TripStatusId = s.TripStatusId
                ORDER BY t.CreatedAt DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TripDto>(query);
        }


        // get trips assigned to a specific driver by userId
        public async Task<IEnumerable<TripDto>> GetTripsByDriverAsync(int userId)
        {
            var query = @"
                SELECT
                    t.TripId, t.DriverId, t.VehicleId,
                    s.StatusName, t.PlannedDeparture
                FROM Trips t
                INNER JOIN TripStatus s ON t.TripStatusId = s.TripStatusId
                INNER JOIN Drivers d ON t.DriverId = d.DriverId
                WHERE d.UserId = @UserId
                ORDER BY t.CreatedAt DESC";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TripDto>(query, new { UserId = userId });
        }


        // get all shipments assigned to a specific trip
        public async Task<IEnumerable<TripShipmentDto>> GetTripShipments(int tripId)
        {
            var query = @"
                SELECT
                    s.ShipmentId, s.OrderId, o.PickupCity, o.DeliveryCity,
                    o.PackageWeight, ss.StatusName, t.CreatedAt
                FROM TripShipments ts
                INNER JOIN Shipments s ON ts.ShipmentId = s.ShipmentId
                INNER JOIN Orders o ON s.OrderId = o.OrderId
                INNER JOIN ShipmentStatus ss ON s.ShipmentStatusId = ss.ShipmentStatusId
                INNER JOIN Trips t ON ts.TripId = t.TripId
                WHERE ts.TripId = @TripId";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<TripShipmentDto>(query, new { TripId = tripId });
        }


        // set a planned trip to in progress
        public async Task<TripDto?> StartTripAsync(int tripId)
        {
            const int PlannedStatusId = 1;
            const int InProgressStatusId = 2;


            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                @"UPDATE Trips SET TripStatusId = @InProgressStatusId
                  WHERE TripId = @TripId AND TripStatusId = @PlannedStatusId",
                new { TripId = tripId, InProgressStatusId, PlannedStatusId });


            return await GetTripByIdAsync(connection, tripId);
        }


        // get all currently available drivers
        public async Task<IEnumerable<Driver>> GetAvailableDrivers()
        {
            var query = @"
                SELECT d.DriverId, d.LicenseNumber, d.DriverAvailabilityStatusId, u.FirstName
                FROM Drivers d
                INNER JOIN Users u ON d.UserId = u.UserId
                WHERE d.DriverAvailabilityStatusId = 1";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Driver>(query);
        }


        // get all currently available vehicles
        public async Task<IEnumerable<Vehicle>> GetAvailableVehicles()
        {
            var query = @"
                SELECT VehicleId, RegistrationNumber, Capacity,
                       VehicleAvailabilityStatusId, VehicleModel
                FROM Vehicles
                WHERE VehicleAvailabilityStatusId = 1";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Vehicle>(query);
        }


        // get drivers available on a specific trip date based on their weekly schedule
        public async Task<IEnumerable<Driver>> GetAvailableDriversByDate(DateTime tripDate)
        {
            var query = @"
                SELECT d.DriverId, u.FirstName, u.LastName, d.LicenseNumber,
                       da.AvailableDays, DATENAME(WEEKDAY, @TripDate) AS RequestedDay
                FROM DriverAvailability da
                INNER JOIN Drivers d ON da.DriverId = d.DriverId
                INNER JOIN Users u ON u.UserId = d.UserId
                WHERE da.DriverAvailabilityStatusId = 1
                  AND ',' + da.AvailableDays + ',' LIKE '%,' + DATENAME(WEEKDAY, @TripDate) + ',%'";


            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Driver>(query, new { TripDate = tripDate });
        }


        // create a trip, link shipments, and mark driver and vehicle as unavailable
        public async Task<int> CreateTripAsync(CreateTripDto dto)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();


            var tripId = await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Trips (DriverId, VehicleId, TripStatusId, PlannedDeparture, CreatedAt)
                  VALUES (@DriverId, @VehicleId, 1, GETDATE(), GETDATE());
                  SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new { dto.DriverId, dto.VehicleId }, transaction);


            foreach (var shipmentId in dto.ShipmentIds)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO TripShipments (TripId, ShipmentId) VALUES (@TripId, @ShipmentId)",
                    new { TripId = tripId, ShipmentId = shipmentId }, transaction);


                await connection.ExecuteAsync(
                    "UPDATE Shipments SET ShipmentStatusId = 7 WHERE ShipmentId = @ShipmentId",
                    new { ShipmentId = shipmentId }, transaction);
            }


            await connection.ExecuteAsync(
                "UPDATE Drivers SET DriverAvailabilityStatusId = 2 WHERE DriverId = @DriverId",
                new { dto.DriverId }, transaction);


            await connection.ExecuteAsync(
                "UPDATE Vehicles SET VehicleAvailabilityStatusId = 2 WHERE VehicleId = @VehicleId",
                new { dto.VehicleId }, transaction);


            transaction.Commit();
            return tripId;
        }


        // update shipment status and sync the linked order status
        public async Task<UpdateShipmentStatusResult?> UpdateShipmentStatusAsync(int shipmentId, string statusName)
        {
            var statusId = GetShipmentStatusId(statusName);
            if (statusId == null) return null;


            using var connection = _context.CreateConnection();


            // failed shipments map to Cancelled (12) on the order
            if (statusId == 4)
            {
                await connection.ExecuteAsync(@"
                    UPDATE Shipments SET ShipmentStatusId = @StatusId WHERE ShipmentId = @ShipmentId;
                    UPDATE Orders SET OrderStatusId = 12
                    WHERE OrderId = (SELECT OrderId FROM Shipments WHERE ShipmentId = @ShipmentId);",
                    new { ShipmentId = shipmentId, StatusId = statusId.Value });
            }
            else
            {
                await connection.ExecuteAsync(@"
                    UPDATE Shipments SET ShipmentStatusId = @StatusId WHERE ShipmentId = @ShipmentId;
                    UPDATE Orders
                    SET OrderStatusId = (
                        SELECT os.OrderStatusId FROM OrderStatus os
                        INNER JOIN ShipmentStatus ss ON ss.StatusName = os.StatusName
                        WHERE ss.ShipmentStatusId = @StatusId
                    )
                    WHERE OrderId = (SELECT OrderId FROM Shipments WHERE ShipmentId = @ShipmentId)
                    AND EXISTS (
                        SELECT 1 FROM OrderStatus os
                        INNER JOIN ShipmentStatus ss ON ss.StatusName = os.StatusName
                        WHERE ss.ShipmentStatusId = @StatusId
                    );",
                    new { ShipmentId = shipmentId, StatusId = statusId.Value });
            }


            // check if trip should be closed when any shipment finishes
            if (statusId == 3 || statusId == 4)
                await TryCloseTripAsync(connection, shipmentId);


            return await GetShipmentStatusResultAsync(connection, shipmentId);
        }


        // map status name to shipment status id
        private static int? GetShipmentStatusId(string statusName)
        {
            return statusName.ToLowerInvariant() switch
            {
                "pending" => 5,
                "assigned" => 7,
                "picked up" => 6,
                "delivered" => 3,
                "failed" => 4,
                _ => null
            };
        }


        // close trip as completed or cancelled depending on whether any shipment failed
        private async Task TryCloseTripAsync(IDbConnection connection, int shipmentId)
        {
            const int DeliveredStatusId = 3;
            const int FailedStatusId = 4;
            const int CompletedTripStatusId = 3;
            const int CancelledTripStatusId = 4;


            var tripId = await connection.ExecuteScalarAsync<int?>(
                "SELECT TOP 1 TripId FROM TripShipments WHERE ShipmentId = @ShipmentId",
                new { ShipmentId = shipmentId });


            if (tripId == null) return;


            var totalCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM TripShipments WHERE TripId = @TripId",
                new { TripId = tripId });


            var doneCount = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM TripShipments ts
                  INNER JOIN Shipments s ON ts.ShipmentId = s.ShipmentId
                  WHERE ts.TripId = @TripId
                  AND s.ShipmentStatusId IN (@DeliveredStatusId, @FailedStatusId)",
                new { TripId = tripId, DeliveredStatusId, FailedStatusId });


            // only close trip if all shipments are done
            if (doneCount != totalCount || totalCount == 0) return;


            var failedCount = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM TripShipments ts
                  INNER JOIN Shipments s ON ts.ShipmentId = s.ShipmentId
                  WHERE ts.TripId = @TripId AND s.ShipmentStatusId = @FailedStatusId",
                new { TripId = tripId, FailedStatusId });


            // cancelled if any shipment failed, completed if all delivered
            var newTripStatus = failedCount > 0 ? CancelledTripStatusId : CompletedTripStatusId;


            await connection.ExecuteAsync(
                "UPDATE Trips SET TripStatusId = @NewTripStatus WHERE TripId = @TripId",
                new { TripId = tripId, NewTripStatus = newTripStatus });


            var tripDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT DriverId, VehicleId FROM Trips WHERE TripId = @TripId",
                new { TripId = tripId });


            if (tripDetails == null) return;


            await connection.ExecuteAsync(
                "UPDATE Drivers SET DriverAvailabilityStatusId = 1 WHERE DriverId = @DriverId",
                new { DriverId = tripDetails.DriverId });


            await connection.ExecuteAsync(
                "UPDATE Vehicles SET VehicleAvailabilityStatusId = 1 WHERE VehicleId = @VehicleId",
                new { VehicleId = tripDetails.VehicleId });
        }


        // fetch current status of a shipment and its linked trip
        private async Task<UpdateShipmentStatusResult?> GetShipmentStatusResultAsync(IDbConnection connection, int shipmentId)
        {
            var query = @"
                SELECT
                    s.ShipmentId, s.ShipmentStatusId, ss.StatusName,
                    (SELECT TOP 1 ts.TripId FROM TripShipments ts
                     WHERE ts.ShipmentId = s.ShipmentId) AS TripId,
                    (SELECT TOP 1 ts2.StatusName FROM TripShipments t
                     INNER JOIN Trips tr ON t.TripId = tr.TripId
                     INNER JOIN TripStatus ts2 ON tr.TripStatusId = ts2.TripStatusId
                     WHERE t.ShipmentId = s.ShipmentId) AS TripStatusName
                FROM Shipments s
                INNER JOIN ShipmentStatus ss ON s.ShipmentStatusId = ss.ShipmentStatusId
                WHERE s.ShipmentId = @ShipmentId";


            return await connection.QueryFirstOrDefaultAsync<UpdateShipmentStatusResult>(
                query, new { ShipmentId = shipmentId });
        }


        // get a trip by id using an existing connection
        private async Task<TripDto?> GetTripByIdAsync(IDbConnection connection, int tripId)
        {
            var query = @"
                SELECT
                    t.TripId, t.DriverId, t.VehicleId,
                    s.StatusName, t.PlannedDeparture
                FROM Trips t
                INNER JOIN TripStatus s ON t.TripStatusId = s.TripStatusId
                WHERE t.TripId = @TripId";


            return await connection.QueryFirstOrDefaultAsync<TripDto>(query, new { TripId = tripId });
        }
    }
}



