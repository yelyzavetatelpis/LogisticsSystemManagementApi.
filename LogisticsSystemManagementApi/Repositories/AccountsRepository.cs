using Dapper;
using LogisticsSystemManagementApi.Data;
using LogisticsSystemManagementApi.DTOs;

namespace LogisticsSystemManagementApi.Repositories
{
    public class AccountsRepository : IAccountsRepository
    {
        private readonly DbContext _context;

        public AccountsRepository(DbContext context)
        {
            _context = context;
        }

        // get all users with their role names
        public async Task<IEnumerable<AccountsDto>> GetUsers()
        {
            var query = @"
                SELECT
                    u.UserId, u.FirstName, u.LastName,
                    u.Email, u.MobileNumber, u.RoleId, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleId = r.RoleId
                ORDER BY u.UserId DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<AccountsDto>(query);
        }

        // get a single user by their id
        public async Task<AccountsDto?> GetUserById(int id)
        {
            var query = @"
                SELECT
                    u.UserId, u.FirstName, u.LastName,
                    u.Email, u.MobileNumber, u.RoleId, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.UserId = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AccountsDto>(query, new { Id = id });
        }

        // create a new user 
        public async Task<int> CreateUser(AccountsDto dto)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // insert the user row and grab the new id
                var userId = await connection.QuerySingleAsync<int>(@"
                    INSERT INTO Users (FirstName, LastName, Email, MobileNumber, RoleId)
                    VALUES (@FirstName, @LastName, @Email, @MobileNumber, @RoleId);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new
                    {
                        dto.FirstName,
                        dto.LastName,
                        dto.Email,
                        dto.MobileNumber,
                        dto.RoleId
                    }, transaction);

                // store the password hash in UserCredentials
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var hashed = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    await connection.ExecuteAsync(
                        "INSERT INTO UserCredentials (UserId, PasswordHash) VALUES (@UserId, @PasswordHash);",
                        new { UserId = userId, PasswordHash = hashed }, transaction);
                }

                // role specific record

                if (dto.RoleId == 4)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO Customers (UserId) VALUES (@UserId);",
                        new { UserId = userId }, transaction);
                }
                else if (dto.RoleId == 2)
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO Dispatchers (UserId) VALUES (@UserId);",
                        new { UserId = userId }, transaction);
                }

                transaction.Commit();
                return userId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // update user details and optionally the password
        public async Task<bool> UpdateUser(int id, AccountsDto dto)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var rows = await connection.ExecuteAsync(@"
                    UPDATE Users
                    SET FirstName = @FirstName, LastName = @LastName,
                        Email = @Email, MobileNumber = @MobileNumber, RoleId = @RoleId
                    WHERE UserId = @Id",
                    new { Id = id, dto.FirstName, dto.LastName, dto.Email, dto.MobileNumber, dto.RoleId },
                    transaction);

                // only update password if a new one was provided
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    await connection.ExecuteAsync(@"
                        IF EXISTS (SELECT 1 FROM UserCredentials WHERE UserId = @Id)
                            UPDATE UserCredentials SET PasswordHash = @PasswordHash WHERE UserId = @Id
                        ELSE
                            INSERT INTO UserCredentials (UserId, PasswordHash) VALUES (@Id, @PasswordHash)",
                        new { Id = id, PasswordHash = hashedPassword }, transaction);
                }

                transaction.Commit();
                return rows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // delete a user and all their related records
        public async Task<bool> DeleteUser(int userId)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // remove customer record if the user is a customer
                await connection.ExecuteAsync(
                    "DELETE FROM Customers WHERE UserId = @UserId",
                    new { UserId = userId }, transaction);

                // remove dispatcher record if the user is a dispatcher
                await connection.ExecuteAsync(
                    "DELETE FROM Dispatchers WHERE UserId = @UserId",
                    new { UserId = userId }, transaction);

                // clean up everything that references the driver 
                await connection.ExecuteAsync(@"
                    DELETE FROM TripShipments
                    WHERE TripId IN (
                        SELECT TripId FROM Trips
                        WHERE DriverId IN (SELECT DriverId FROM Drivers WHERE UserId = @UserId)
                    )", new { UserId = userId }, transaction);

                await connection.ExecuteAsync(@"
                    DELETE FROM Trips
                    WHERE DriverId IN (SELECT DriverId FROM Drivers WHERE UserId = @UserId)",
                    new { UserId = userId }, transaction);

                await connection.ExecuteAsync(@"
                    DELETE FROM DriverAvailability
                    WHERE DriverId IN (SELECT DriverId FROM Drivers WHERE UserId = @UserId)",
                    new { UserId = userId }, transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM Drivers WHERE UserId = @UserId",
                    new { UserId = userId }, transaction);

                // remove login credentials
                await connection.ExecuteAsync(
                    "DELETE FROM UserCredentials WHERE UserId = @UserId",
                    new { UserId = userId }, transaction);

                // remove the user
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId = @UserId",
                    new { UserId = userId }, transaction);

                transaction.Commit();
                return rows > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Delete failed: " + ex.Message);
            }
        }

        // get all drivers with their availability status
        public async Task<IEnumerable<DriverOverviewDto>> GetDriversAsync()
        {
            var query = @"
                SELECT
                    u.UserId, u.FirstName, u.LastName, u.Email,
                    d.LicenseNumber,
                    ds.StatusName AS AvailabilityStatus
                FROM Drivers d
                INNER JOIN Users u ON d.UserId = u.UserId
                INNER JOIN DriverAvailabilityStatus ds ON d.DriverAvailabilityStatusId = ds.DriverAvailabilityStatusId
                ORDER BY u.FirstName";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<DriverOverviewDto>(query);
        }
    }
}


