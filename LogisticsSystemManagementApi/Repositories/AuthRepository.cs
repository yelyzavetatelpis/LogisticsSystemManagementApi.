using Dapper;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly Data.DbContext _context;

        public AuthRepository(Data.DbContext context)
        {
            _context = context;
        }

        // Finding a user by email address
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
            }
        }

        // Insert a new user and return generated UserId
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

        // Store hashed password for a user
        public async Task CreateUserCredentialsAsync(int userId, string passwordHash)
        {
            var sql = @"INSERT INTO UserCredentials (UserId, PasswordHash)
                        VALUES (@UserId, @PasswordHash);";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash });
            }
        }

        // Create a customer record 
        public async Task CreateCustomerAsync(int userId)
        {
            var sql = "INSERT INTO Customers (UserId) VALUES (@UserId);";
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { UserId = userId });
            }
        }

        // Retrieve the stored password hash for login verification
        public async Task<string> GetPasswordHashAsync(int userId)
        {
            var sql = "SELECT PasswordHash FROM UserCredentials WHERE UserId = @UserId";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
            }
        }

        // Get a role by  id
        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            var sql = "SELECT * FROM Roles WHERE RoleId = @RoleId";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { RoleId = roleId });
            }
        }

        // Return a list of all users in the system
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<User>("SELECT * FROM Users");
            }
        }
    }
}