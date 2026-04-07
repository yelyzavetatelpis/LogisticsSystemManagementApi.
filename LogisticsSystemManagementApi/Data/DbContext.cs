using Microsoft.Data.SqlClient;
using System.Data;


namespace LogisticsSystemManagementApi.Data
{
    public class DbContext
    {
        private readonly string _connectionString;


        public DbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public virtual IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }



    }
}



