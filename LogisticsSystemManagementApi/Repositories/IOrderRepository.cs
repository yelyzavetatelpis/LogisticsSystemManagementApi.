using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    // Order-related database operations
    public interface IOrderRepository
    {
        Task<int> GetCustomerIdByUserIdAsync(int userId);
        Task CreateOrderAsync(Order order);
        Task<CustomerDashboardDto> GetCustomerDashboardDataAsync(int customerId);
        Task<IEnumerable<MyOrdersDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task AcceptOrderAsync(int orderId);
        Task RejectOrderAsync(int orderId, string reason);
        Task<List<Order>> GetPendingOrdersAsync();
        Task<List<Shipment>> GetShipmentsAsync();
        Task<IEnumerable<Vehicle>> GetAvailableVehicles();
        Task<IEnumerable<Driver>> GetAvailableDrivers();
        Task<int> CreateTripAsync (CreateTripDto dto);
    }
}