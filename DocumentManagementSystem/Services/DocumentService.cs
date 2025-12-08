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
        private List<DocumentManagementSystem.Model.DocumentModel> _documents = new();

        private string BlobUrl = "";

        private List<OfficeService.OfficeInfo> OfficeList = new();

        private BlobStorageService _blobStorageService => new BlobStorageService(_configuration);

        public DocumentService(DatabaseConnection dbConnection, IConfiguration configuration)
        {
            _dbConnection = dbConnection;
            _configuration = configuration;
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
            BlobUrl = await _blobStorageService.UploadBrowserFileAsync(SelectedFile);

            if (string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(filetype))
                return;

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
    }
}