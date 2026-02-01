using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace LogisticsSystemManagementApi.Data
{
    public class DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ILogger<DbContext> _logger;

        public DbContext(IConfiguration configuration, ILogger<DbContext> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");

           
            _logger.LogInformation("Connection String: {ConnectionString}", _connectionString);
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}