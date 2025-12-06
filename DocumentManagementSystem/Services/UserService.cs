using Npgsql;
using Microsoft.Extensions.Configuration;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;

        // 🔹 Stores the logged-in user's Username and Role
        public string? CurrentUsername { get; private set; }
        public string? CurrentRole { get; private set; }

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        // 🔹 Validate username + password
        public string? ValidateUser(string username, string password)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

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

            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                CurrentUsername = username;  // <-- FIXED
                CurrentRole = result.ToString();
            }

            return CurrentRole;
        }

        // ---------------------------
        // USER LIST METHODS
        // ---------------------------

        public List<UserModel> GetUsers()
        {
            var list = new List<UserModel>();
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"SELECT ""UserId"", ""Username"", ""PasswordHash"", ""Role"" FROM ""DocMS"".""Users"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new UserModel
                {
                    UserId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }

            return list;
        }

        public void AddUser(string username, string password, string role)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"
                INSERT INTO ""DocMS"".""Users"" (""Username"", ""PasswordHash"", ""Role"")
                VALUES (@u, @p, @r)
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            cmd.Parameters.AddWithValue("@r", role);

            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(int id)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"DELETE FROM ""DocMS"".""Users"" WHERE ""UserId"" = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        public void UpdateRole(int id, string role)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"UPDATE ""DocMS"".""Users"" SET ""Role"" = @r WHERE ""UserId"" = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        public int CountAllUsers()
        {
            string connStr = _config.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM ""DocMS"".""Users"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

    }
}