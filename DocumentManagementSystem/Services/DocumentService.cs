using Npgsql;
using Microsoft.Extensions.Configuration;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Services
{
    public class DocumentService
    {
        private readonly IConfiguration _config;

        public DocumentService(IConfiguration config)
        {
            _config = config;
        }

        // -----------------------------------------
        // GET ALL DOCUMENTS (joins Office table)
        // -----------------------------------------
        public List<DocumentModel> GetAllDocuments()
        {
            var list = new List<DocumentModel>();
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"
                SELECT d.""DocumentId"",
                       d.""FileName"",
                       d.""OfficeId"",
                       o.""OfficeName"",
                       d.""UploadDate"",
                       d.""FilePath""
                FROM ""DocMS"".""Documents"" d
                JOIN ""DocMS"".""Offices"" o ON d.""OfficeId"" = o.""OfficeId""
                ORDER BY d.""UploadDate"" DESC;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new DocumentModel
                {
                    DocumentId = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    OfficeId = reader.GetInt32(2),
                    OfficeName = reader.GetString(3),
                    DateUploaded = reader.GetDateTime(4),
                    FilePath = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }

            return list;
        }

        // -----------------------------------------
        // ADD DOCUMENT USING MODEL
        // -----------------------------------------
        public void AddDocument(DocumentModel doc)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"
                INSERT INTO ""DocMS"".""Documents""
                (""FileName"", ""OfficeId"", ""UploadDate"", ""FilePath"")
                VALUES (@f, @o, @d, @p);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", doc.FileName);
            cmd.Parameters.AddWithValue("@o", doc.OfficeId);
            cmd.Parameters.AddWithValue("@d", doc.DateUploaded);
            cmd.Parameters.AddWithValue("@p", doc.FilePath ?? "");

            cmd.ExecuteNonQuery();
        }

        // -----------------------------------------
        // ADD DOCUMENT (USED BY UPLOAD.RAZOR)
        // -----------------------------------------
        public void AddDocumentToDatabase(string fileName, int officeId, string filePath)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string fileType = Path.GetExtension(fileName)?
                .Replace(".", "").ToUpper() ?? "UNKNOWN";

            string sql = @"
                INSERT INTO ""DocMS"".""Documents""
                (""FileName"", ""OfficeId"", ""FilePath"", ""FileType"", ""UploadDate"")
                VALUES (@f, @o, @p, @t, CURRENT_TIMESTAMP);
            ";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", fileName);
            cmd.Parameters.AddWithValue("@o", officeId);
            cmd.Parameters.AddWithValue("@p", filePath);
            cmd.Parameters.AddWithValue("@t", fileType);

            cmd.ExecuteNonQuery();
        }

        // -----------------------------------------
        // DELETE DOCUMENT
        // -----------------------------------------
        public void DeleteDocument(int documentId)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"DELETE FROM ""DocMS"".""Documents"" WHERE ""DocumentId"" = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", documentId);
            cmd.ExecuteNonQuery();
        }

        // -----------------------------------------
        // DASHBOARD COUNTS
        // -----------------------------------------
        public int CountAllDocuments()
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"SELECT COUNT(*) FROM ""DocMS"".""Documents"";";
            using var cmd = new NpgsqlCommand(sql, conn);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

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

        // -----------------------------------------
        // UPDATE DOCUMENT
        // -----------------------------------------
        public void UpdateDocument(DocumentModel doc)
        {
            using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            string sql = @"
                UPDATE ""DocMS"".""Documents""
                SET ""FileName"" = @f,
                    ""OfficeId"" = @o
                WHERE ""DocumentId"" = @id;
            ";

            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@f", doc.FileName);
            cmd.Parameters.AddWithValue("@o", doc.OfficeId);
            cmd.Parameters.AddWithValue("@id", doc.DocumentId);

            cmd.ExecuteNonQuery();
        }

    }
}