using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    // dashboard data operations for the customer 
    public interface ICustomerDashboardRepository
    {
        Task<int> GetCustomerIdByUserIdAsync(int userId);
        Task<CustomerDashboardDto> GetCustomerDashboardDataAsync(int customerId);
    }
}



