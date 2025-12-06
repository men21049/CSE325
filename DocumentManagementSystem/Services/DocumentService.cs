using Npgsql;
using Microsoft.Extensions.Configuration;
using DocumentManagementSystem.Model;

namespace DocumentManagementSystem.Services
{
    public class DocumentService
    {
        private readonly IConfiguration _config;

        public DocumentService(IConfiguration config)
        {
            _config = config;
        }

        // Example existing in-memory list
        private List<DocumentModel> Documents = new();

        // -----------------------
        // Dashboard Count: Documents
        // -----------------------
        public int CountAllDocuments()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM ""DocMS"".""Documents"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // -----------------------
        // Dashboard Count: Offices
        // -----------------------
        public int CountOffices()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM ""DocMS"".""Offices"";";

            using var cmd = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // -----------------------
        // Dashboard Count: Today Uploads
        // -----------------------
        public int CountDocumentsUploadedToday()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"
                SELECT COUNT(*)
                FROM ""DocMS"".""Documents""
                WHERE ""UploadDate""::date = CURRENT_DATE;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}