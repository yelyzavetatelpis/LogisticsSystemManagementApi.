using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;


namespace LogisticsSystemManagementApi.Repositories
{
    // trip, driver, vehicle and shipment status operations
    public interface ITripRepository
    {
        Task<IEnumerable<TripDto>> GetTripsAsync();
        Task<IEnumerable<TripShipmentDto>> GetTripShipments(int tripId);
        Task<TripDto?> StartTripAsync(int tripId);
        Task<IEnumerable<Driver>> GetAvailableDrivers();
        Task<IEnumerable<Vehicle>> GetAvailableVehicles();
        Task<IEnumerable<Driver>> GetAvailableDriversByDate(DateTime tripDate);
        Task<int> CreateTripAsync(CreateTripDto dto);
        Task<UpdateShipmentStatusResult?> UpdateShipmentStatusAsync(int shipmentId, string statusName);
        Task<IEnumerable<TripDto>> GetTripsByDriverAsync(int userId);
    }
}



