using Npgsql;
using Microsoft.Extensions.Configuration;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        // Returns:
        // "Admin"  → if username/password match and role is Admin
        // "User"   → if username/password match and role is User
        // null     → if login invalid
        public string? ValidateUser(string username, string password)
        {
            Console.WriteLine("ValidateUser started");

            try
            {
                string connStr = _config.GetConnectionString("DefaultConnection");
                Console.WriteLine("Connection string: " + connStr);

                using var conn = new NpgsqlConnection(connStr);
                conn.Open();
                Console.WriteLine("Connected to PostgreSQL");

                string sql = @"
                    SELECT ""Role""
                    FROM ""DocMS"".""Users""
                    WHERE ""Username"" = @u
                    AND ""PasswordHash"" = @p
                    LIMIT 1;
                ";

                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                Console.WriteLine($"Executing SQL: Username={username}, Password={password}");

                var result = cmd.ExecuteScalar();

                Console.WriteLine("SQL result: " + (result == null ? "NULL" : result.ToString()));

                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public List<UserModel> GetUsers()
        {
            var users = new List<UserModel>();
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"
                SELECT ""UserId"", ""Username"", ""Role""
                FROM ""DocMS"".""Users""
                ORDER BY ""UserId"";
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new UserModel
                {
                    UserId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Role = reader.GetString(2)
                });
            }

            return users;
        }

    }
}