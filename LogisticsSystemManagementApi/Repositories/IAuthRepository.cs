using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    //authentication-related database operations

    public interface IAuthRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<int> RegisterUserAsync(User user);
        Task CreateUserCredentialsAsync(int userId, string passwordHash);
        Task CreateCustomerAsync(int userId);
        Task<string> GetPasswordHashAsync(int userId);
        Task<Role> GetRoleByIdAsync(int roleId);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}