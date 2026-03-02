using LogisticsSystemManagementApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

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
        public DbSet<Order> Orders { get; set; }
    }
}