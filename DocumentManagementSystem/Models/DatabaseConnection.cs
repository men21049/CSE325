namespace DocumentManagementSystem.Model
{
    using System;
    using System.Data;
    using System.Data.Common;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    public class DatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureSqlConnection") 
                ?? throw new InvalidOperationException("Azure SQL connection string not found in configuration");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            var results = new List<Dictionary<string, object>>();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }

        public async Task<int> ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task<int>> transactionAction)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var dbTransaction = await connection.BeginTransactionAsync();
            using var transaction = (SqlTransaction)dbTransaction;
            
            try
            {
                var rowsAffected = await transactionAction(connection, transaction);
                await transaction.CommitAsync();
                return rowsAffected;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
