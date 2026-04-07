using LogisticsSystemManagementApi.DTOs;
using LogisticsSystemManagementApi.Models;


namespace LogisticsSystemManagementApi.Repositories
{
    // order creation and retrieval operations
    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(Order order);
        Task<IEnumerable<MyOrdersDto>> GetOrdersByCustomerIdAsync(int customerId);
    }
}



