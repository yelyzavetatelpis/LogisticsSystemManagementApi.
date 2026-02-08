using Dapper;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
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

    public class AuthRepository : IAuthRepository
    {
        private readonly Data.DbContext _context;

        public AuthRepository(Data.DbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = "SELECT * FROM Users WHERE Email = @Email";
                return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
            }
        }

        public async Task<int> RegisterUserAsync(User user)
        {
            var sql = @"INSERT INTO Users (FirstName, LastName, Email, MobileNumber, RoleId) 
                        VALUES (@FirstName, @LastName, @Email, @MobileNumber, @RoleId);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleAsync<int>(sql, user);
            }
        }

        public async Task CreateUserCredentialsAsync(int userId, string passwordHash)
        {
            var sql = @"INSERT INTO UserCredentials (UserId, PasswordHash) 
                        VALUES (@UserId, @PasswordHash);";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash });
            }
        }

        public async Task CreateCustomerAsync(int userId)
        {
            var sql = @"INSERT INTO Customers (UserId) 
                        VALUES (@UserId);";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { UserId = userId });
            }
        }

        public async Task<string> GetPasswordHashAsync(int userId)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = "SELECT PasswordHash FROM UserCredentials WHERE UserId = @UserId";
                return await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
            }
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Role>(
                    "SELECT * FROM Roles WHERE RoleId = @RoleId",
                    new { RoleId = roleId }
                );
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<User>("SELECT * FROM Users");
            }
        }
    }
}