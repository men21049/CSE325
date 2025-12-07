using Npgsql;
using Microsoft.Extensions.Configuration;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Services
{
    public class OfficeService
    {
        private readonly IConfiguration _config;

        public OfficeService(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("DefaultConnection");

        // ------------------------
        // Get All Offices
        // ------------------------
        public List<OfficeModel> GetOffices()
        {
            var list = new List<OfficeModel>();

            using var conn = new NpgsqlConnection(Conn);
            conn.Open();

            string sql = @"SELECT ""OfficeId"", ""OfficeName"", ""Description"" 
                           FROM ""DocMS"".""Offices"" 
                           ORDER BY ""OfficeId"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new OfficeModel
                {
                    OfficeId = reader.GetInt32(0),
                    OfficeName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }

            return list;
        }

        // ------------------------
        // Add Office
        // ------------------------
        public void AddOffice(string name, string? description)
        {
            using var conn = new NpgsqlConnection(Conn);
            conn.Open();

            string sql = @"INSERT INTO ""DocMS"".""Offices"" (""OfficeName"", ""Description"")
                           VALUES (@n, @d);";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", (object?)description ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        // ------------------------
        // Update Office
        // ------------------------
        public void UpdateOffice(int id, string name, string? description)
        {
            using var conn = new NpgsqlConnection(Conn);
            conn.Open();

            string sql = @"UPDATE ""DocMS"".""Offices"" 
                           SET ""OfficeName"" = @n, ""Description"" = @d
                           WHERE ""OfficeId"" = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", (object?)description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ------------------------
        // Delete Office
        // ------------------------
        public void DeleteOffice(int id)
        {
            using var conn = new NpgsqlConnection(Conn);
            conn.Open();

            string sql = @"DELETE FROM ""DocMS"".""Offices"" WHERE ""OfficeId"" = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ------------------------
        // 🔹 Count Offices (Used in Dashboard)
        // ------------------------
        public int CountOffices()
        {
            using var conn = new NpgsqlConnection(Conn);
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM ""DocMS"".""Offices"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}