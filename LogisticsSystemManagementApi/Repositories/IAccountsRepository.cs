using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Repositories
{
    // user account management 
    public interface IAccountsRepository
    {
        Task<IEnumerable<AccountsDto>> GetUsers();
        Task<AccountsDto?> GetUserById(int id);
        Task<int> CreateUser(AccountsDto dto);
        Task<bool> UpdateUser(int id, AccountsDto dto);
        Task<bool> DeleteUser(int userId);
        Task<IEnumerable<DriverOverviewDto>> GetDriversAsync();
    }
}


