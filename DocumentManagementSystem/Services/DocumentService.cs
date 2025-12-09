using DocumentManagementSystem.Model;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DocumentManagementSystem.Services
{
    public class DocumentService
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly IConfiguration _configuration;
        private readonly BlobStorageService _blobStorageService;
        private List<DocumentManagementSystem.Model.DocumentModel> _documents = new();

        private string BlobUrl = "";

        private List<OfficeService.OfficeInfo> OfficeList = new();

        public DocumentService(DatabaseConnection dbConnection, IConfiguration configuration, BlobStorageService blobStorageService)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
            _blobStorageService = blobStorageService;
        }

        public async Task LoadDocumentsAsync()
        {
            try
            {
                var sql = "SELECT \"DocumentId\", \"FileName\", \"FilePath\", \"FileType\", \"UploadDate\", o.\"OfficeId\" FROM \"DocMS\".\"Documents\" d	left join \"DocMS\".\"Offices\" o on d.\"OfficeId\" = o.\"OfficeId\"";
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>());

                if (results.Count > 0)
                {
                    _documents = results
                        .Select(row => new DocumentManagementSystem.Model.DocumentModel
                        {
                            DocumentID = row["DocumentId"] != null ? Convert.ToInt32(row["DocumentId"]) : 0,
                            FileName = row["FileName"]?.ToString() ?? string.Empty,
                            FilePath = row["FilePath"]?.ToString() ?? string.Empty,
                            FileType = row["FileType"]?.ToString() ?? string.Empty,
                            UploadDate = row["UploadDate"] != null ? Convert.ToDateTime(row["UploadDate"]) : DateTime.MinValue,
                            OfficeID = row["OfficeId"] != null ? Convert.ToInt32(row["OfficeId"]) : 0,
                            OfficeName = string.Empty // Se puede obtener del JOIN si es necesario
                        })
                        .ToList();
                }
            }
            catch
            {
                _documents = new List<DocumentManagementSystem.Model.DocumentModel>();
                throw;
            }
        }

        public IEnumerable<DocumentManagementSystem.Model.DocumentModel> GetDocuments() => _documents;

        public async Task RefreshDocumentsAsync()
        {
            await LoadDocumentsAsync();
        }

        public async Task AddDocumentsAsync(string filename, IBrowserFile SelectedFile, string filetype, int officeID)
        {
            // Upload file to blob storage first
            BlobUrl = await _blobStorageService.UploadBrowserFileAsync(SelectedFile);

            // Validate that blob upload was successful
            if (string.IsNullOrWhiteSpace(BlobUrl))
            {
                throw new Exception("Failed to upload file to storage. The file path is empty.");
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(filetype))
            {
                // If validation fails, try to delete the uploaded blob to avoid orphaned files
                try
                {
                    string fileName = BlobUrl.Substring(BlobUrl.LastIndexOf('/') + 1);
                    await _blobStorageService.DeleteFileAsync(fileName);
                }
                catch
                {
                    // Log error but don't throw - the main error is more important
                }
                throw new Exception("Filename and file type are required.");
            }

            try
            {
                var sql = "INSERT INTO \"DocMS\".\"Documents\" (\"FileName\", \"FilePath\", \"FileType\", \"UploadDate\", \"OfficeId\") VALUES (@FileName, @FilePath, @FileType, @UploadDate, @OfficeId)";
                var parameters = new[]
                {
                    new NpgsqlParameter("@FileName", filename),
                    new NpgsqlParameter("@FilePath", BlobUrl),
                    new NpgsqlParameter("@FileType", filetype),
                    new NpgsqlParameter("@UploadDate", DateTime.UtcNow),
                    new NpgsqlParameter("@OfficeId", officeID)
                };
                await _dbConnection.ExecuteNonQueryAsync(sql, parameters);
            }
            catch
            {
                // If database insert fails, try to delete the uploaded blob to avoid orphaned files
                try
                {
                    string fileName = BlobUrl.Substring(BlobUrl.LastIndexOf('/') + 1);
                    await _blobStorageService.DeleteFileAsync(fileName);
                }
                catch
                {
                    // Log error but don't throw - the main error is more important
                }
                throw;
            }
        }

        public async Task DeleteDocumentAsync(int documentID)
        {
            try
            {
                var sqlPath = "SELECT \"FilePath\" FROM \"DocMS\".\"Documents\" WHERE \"DocumentId\" = @DocumentId";
                var pathParameters = new[] { new NpgsqlParameter("@DocumentId", documentID) };
                var results = await _dbConnection.ExecuteQueryAsync(sqlPath, new Dictionary<string, object>(), pathParameters);
                if (results.Count > 0)
                {
                    var filePath = results[0]["FilePath"]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        string fileName = filePath.Substring(filePath.LastIndexOf('/') + 1);
                        try
                        {
                            await _blobStorageService.DeleteFileAsync(fileName);
                        }
                        catch
                        {
                            throw new Exception("Error deleting file from bob storage.");
                        }
                    }
                }
                var sql = "DELETE FROM \"DocMS\".\"Documents\" WHERE \"DocumentId\" = @DocumentId";
                var deleteParameters = new[] { new NpgsqlParameter("@DocumentId", documentID) };
                await _dbConnection.ExecuteNonQueryAsync(sql, deleteParameters);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<DocumentManagementSystem.Model.DocumentModel>> SearchDocumentsAsync(string searchTerm)
        {
            try
            {
                var sql = "SELECT \"DocumentId\", \"FileName\", \"FilePath\", \"FileType\", \"UploadDate\", o.\"OfficeId\" FROM \"DocMS\".\"Documents\" d	left join \"DocMS\".\"Offices\" o on d.\"OfficeId\" = o.\"OfficeId\" WHERE \"FileName\" ILIKE @SearchTerm OR \"FileType\" ILIKE @SearchTerm OR o.\"OfficeName\" ILIKE @SearchTerm";
                var parameters = new[] { new NpgsqlParameter("@SearchTerm", $"%{searchTerm}%") };
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>(), parameters);

                if (results.Count > 0)
                {
                    _documents = results
                        .Select(row => new DocumentManagementSystem.Model.DocumentModel
                        {
                            DocumentID = row["DocumentId"] != null ? Convert.ToInt32(row["DocumentId"]) : 0,
                            FileName = row["FileName"]?.ToString() ?? string.Empty,
                            FilePath = row["FilePath"]?.ToString() ?? string.Empty,
                            FileType = row["FileType"]?.ToString() ?? string.Empty,
                            UploadDate = row["UploadDate"] != null ? Convert.ToDateTime(row["UploadDate"]) : DateTime.MinValue,
                            OfficeID = row["OfficeId"] != null ? Convert.ToInt32(row["OfficeId"]) : 0,
                            OfficeName = string.Empty // Se puede obtener del JOIN si es necesario
                        })
                        .ToList();
                }
                else
                {
                    _documents = new List<DocumentManagementSystem.Model.DocumentModel>();
                }

                return _documents;
            }
            catch
            {
                _documents = new List<DocumentManagementSystem.Model.DocumentModel>();
                throw;
            }

        }

        public async Task<(Stream stream, string fileName, string contentType)> GetDocumentStreamAsync(int documentID)
        {
            try
            {
                var sql = "SELECT \"FilePath\", \"FileName\", \"FileType\" FROM \"DocMS\".\"Documents\" WHERE \"DocumentId\" = @DocumentId";
                var parameters = new[] { new NpgsqlParameter("@DocumentId", documentID) };
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>(), parameters);

                if (results.Count == 0)
                {
                    throw new Exception("Document not found.");
                }

                var filePath = results[0]["FilePath"]?.ToString() ?? string.Empty;
                var fileName = results[0]["FileName"]?.ToString() ?? "document";
                var fileType = results[0]["FileType"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new Exception("Document file path is empty.");
                }

                // Extract blob name from full URL
                string blobName = filePath.Substring(filePath.LastIndexOf('/') + 1);

                // Get file stream from blob storage
                var sourceStream = await _blobStorageService.DownloadFileAsync(blobName);

                // Copy stream to MemoryStream to ensure it can be read multiple times
                var memoryStream = new MemoryStream();
                await sourceStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Determine content type based on file type
                string contentType = fileType.ToUpper() == "PDF" ? "application/pdf" : "text/csv";

                return (memoryStream, fileName, contentType);
            }
            catch
            {
                throw;
            }
        }
    }
}