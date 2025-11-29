namespace FileManagementSystem.Model
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    public class DatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        public IDbCommand CreateQuery(string sql)
        {
            var conn = CreateConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }
    }
}
