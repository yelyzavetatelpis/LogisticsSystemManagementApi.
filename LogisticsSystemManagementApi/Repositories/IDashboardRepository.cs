using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardDto> GetDashboardMetricsAsync();
    }
}



