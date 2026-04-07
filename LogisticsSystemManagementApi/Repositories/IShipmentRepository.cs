using LogisticsSystemManagementApi.Models;


namespace LogisticsSystemManagementApi.Repositories
{
    // shipment and order acceptance/rejection operations
    public interface IShipmentRepository
    {
        Task AcceptOrderAsync(int orderId);
        Task RejectOrderAsync(int orderId, string reason);
        Task<List<Order>> GetPendingOrdersAsync();
        Task<List<Shipment>> GetShipmentsAsync();
    }
}



