namespace FileManagementSystem.Model
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    public class databaseconn
    {
        private readonly string connectionString;

        public databaseconn(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        public query CreateQuery(string sql)
        {
            return new query(sql, CreateConnection());
        }
    }
}