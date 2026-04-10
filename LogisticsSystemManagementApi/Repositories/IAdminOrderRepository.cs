using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    // admin order  
    public interface IAdminOrderRepository
    {
        Task<IEnumerable<AdminOrderResponseDto>> GetAllOrdersAsync();
        Task<IEnumerable<AdminOrderResponseDto>> GetFilteredOrdersAsync(AdminOrderFilterDto filter);
    }
}



