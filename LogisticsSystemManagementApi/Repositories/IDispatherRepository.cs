using LogisticsSystemManagementApi.DTOs;


namespace LogisticsSystemManagementApi.Repositories
{
    // dispatcher dashboard and trip operations
    public interface IDispatcherRepository
    {
        Task<DispatcherDashboardDto> GetDashboardData();
        Task<IEnumerable<TripDto>> GetTripsByDate(DateTime fromDate, DateTime toDate);
    }
}



