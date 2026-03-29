using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Repositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<VehicleResponseDto>> GetVehiclesAsync();
        Task<int> CreateVehicleAsync(CreateVehicleDto dto);
        Task<bool> IsRegistrationExists(string registrationNumber);
        Task<bool> UpdateVehicleStatusAsync(int vehicleId, int statusId);
    }

}


