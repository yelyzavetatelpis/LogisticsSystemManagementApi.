using Dapper;
using LogisticsSystemManagementApi.Models;

namespace LogisticsSystemManagementApi.Repositories
{
    public interface IAuthRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<int> RegisterUserAsync(User user);
        Task<Role> GetRoleByIdAsync(int roleId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> UpdateUserStatusAsync(int id, bool isActive);
    }
    public class AuthRepository : IAuthRepository
    {
        private readonly Data.DbContext _context;

        public AuthRepository(Data.DbContext context)
        {
            _context = context;
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Username = @Username",
                    new { Username = username }
                );

                
                return user ?? new User
                {
                    Username = username
                    
                };
            }
        }


        public async Task<int> RegisterUserAsync(User user)
        {
            var sql = @"Insert into Users (Username, PasswordHash, RoleId, FullName, Email, IsActive) 
                            Values (@Username, @PasswordHash, @RoleId, @FullName, @Email, @IsActive);
                            Select cast(scope_identity() as int);";
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleAsync<int>(sql, user);

            }
        }
        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Role>("SELECT * FROM Roles WHERE Id = @Id", new { Id = roleId });
            }
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<User>("Select * from Users");
            }
        }
        public async Task<bool> UpdateUserStatusAsync(int id, bool isActive)
        {
            var sql = "UPDATE Users SET IsActive = @IsActive WHERE Id = @Id";
            using (var connection = _context.CreateConnection())
                return await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive ? 1 : 0 }) > 0;
        }
    }


} 
