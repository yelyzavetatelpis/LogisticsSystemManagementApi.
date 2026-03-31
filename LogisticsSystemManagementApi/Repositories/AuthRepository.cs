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

        // find a user by their email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        // insert a new user and return id
        public async Task<int> RegisterUserAsync(User user)
        {
            var sql = @"INSERT INTO Users (FirstName, LastName, Email, MobileNumber, RoleId)
                        VALUES (@FirstName, @LastName, @Email, @MobileNumber, @RoleId);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, user);
        }

        // save the hashed password for a user
        public async Task CreateUserCredentialsAsync(int userId, string passwordHash)
        {
            var sql = "INSERT INTO UserCredentials (UserId, PasswordHash) VALUES (@UserId, @PasswordHash);";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash });
        }

        // create a customer record linked to the user
        public async Task CreateCustomerAsync(int userId)
        {
            var sql = "INSERT INTO Customers (UserId) VALUES (@UserId);";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        // create a driver record linked to the user
        public async Task CreateDriverAsync(int userId, string licenseNumber, int driverAvailabilityStatusId)
        {
            var sql = @"INSERT INTO Drivers (UserId, LicenseNumber, DriverAvailabilityStatusId)
                        VALUES (@UserId, @LicenseNumber, @DriverAvailabilityStatusId);";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId, LicenseNumber = licenseNumber, DriverAvailabilityStatusId = driverAvailabilityStatusId });
        }

        // create a dispatcher record linked to the user
        public async Task CreateDispatcherAsync(int userId)
        {
            var sql = "INSERT INTO Dispatchers (UserId) VALUES (@UserId);";
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { UserId = userId });
        }

        // get password hash for login verification
        public async Task<string> GetPasswordHashAsync(int userId)
        {
            var sql = "SELECT PasswordHash FROM UserCredentials WHERE UserId = @UserId";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
        }

        // get role by id
        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            var sql = "SELECT * FROM Roles WHERE RoleId = @RoleId";
            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { RoleId = roleId });
        }

        // return all users in the system
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<User>("SELECT * FROM Users");
        }

        // check if a license number is already registered
        public async Task<bool> IsLicenseExists(string licenseNumber)
        {
            var sql = "SELECT COUNT(1) FROM Drivers WHERE LicenseNumber = @LicenseNumber";
            using var connection = _context.CreateConnection();
            var count = await connection.QuerySingleOrDefaultAsync<int>(sql, new { LicenseNumber = licenseNumber });
            return count > 0;
        }
    }
}


