using Dapper;
using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly Data.DbContext _context;

        public VehicleRepository(Data.DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleResponseDto>> GetVehiclesAsync()
        {
            var sql = @"
            SELECT 
                v.VehicleId,
                v.RegistrationNumber,
                v.Capacity,
                v.VehicleModel,
                v.VehicleAvailabilityStatusId,
                vas.StatusName
            FROM Vehicles v
            INNER JOIN VehicleAvailabilityStatus vas 
                ON v.VehicleAvailabilityStatusId = vas.VehicleAvailabilityStatusId";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<VehicleResponseDto>(sql);
            }
        }

        public async Task<bool> IsRegistrationExists(string registrationNumber)
        {
            var sql = "SELECT COUNT(1) FROM Vehicles WHERE RegistrationNumber = @RegistrationNumber";

            using (var connection = _context.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(sql, new { RegistrationNumber = registrationNumber });
                return count > 0;
            }
        }

        public async Task<int> CreateVehicleAsync(CreateVehicleDto dto)
        {
            var sql = @"
            INSERT INTO Vehicles (RegistrationNumber, Capacity, VehicleModel, VehicleAvailabilityStatusId)
            VALUES (@RegistrationNumber, @Capacity, @VehicleModel, @VehicleAvailabilityStatusId);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleAsync<int>(sql, dto);
            }
        }
        public async Task<bool> UpdateVehicleStatusAsync(int vehicleId, int statusId)
        {
            var sql = @"
        UPDATE Vehicles
        SET VehicleAvailabilityStatusId = @StatusId
        WHERE VehicleId = @VehicleId";

            using (var connection = _context.CreateConnection())
            {
                var rows = await connection.ExecuteAsync(sql, new
                {
                    VehicleId = vehicleId,
                    StatusId = statusId
                });

                return rows > 0;
            }
        }
    }
}


