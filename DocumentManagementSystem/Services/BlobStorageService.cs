using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace DocumentManagementSystem.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorageConnection")
                ?? throw new InvalidOperationException("Azure Storage connection string not found in configuration");
            
            // Log connection string info (without sensitive data)
            var accountName = ExtractAccountName(connectionString);
            Console.WriteLine($"[BLOB STORAGE] Initializing with account: {accountName}");
            Console.WriteLine($"[BLOB STORAGE] Connection string exists: {!string.IsNullOrEmpty(connectionString)}");
            
            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _containerName = configuration["AzureStorage:ContainerName"] ?? "documents";
                Console.WriteLine($"[BLOB STORAGE] Container name: {_containerName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BLOB STORAGE] Error creating BlobServiceClient: {ex.Message}");
                throw;
            }
        }

        private string ExtractAccountName(string connectionString)
        {
            try
            {
                var parts = connectionString.Split(';');
                var accountPart = parts.FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase));
                return accountPart?.Split('=')[1] ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private async Task EnsureContainerExistsAsync()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                Console.WriteLine($"[BLOB STORAGE] Starting upload for file: {fileName}");
                await EnsureContainerExistsAsync();
                
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                Console.WriteLine($"[BLOB STORAGE] Blob client created. Target URI: {blobClient.Uri}");

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };

                await blobClient.UploadAsync(fileStream, uploadOptions);
                Console.WriteLine($"[BLOB STORAGE] Upload successful. File URL: {blobClient.Uri}");
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BLOB STORAGE] Upload error: {ex.Message}");
                Console.WriteLine($"[BLOB STORAGE] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[BLOB STORAGE] Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<string> UploadBrowserFileAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, string? customFileName = null)
        {
            var fileName = customFileName ?? file.Name;
            

            var fileNameWithTimestamp = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}";
            
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max
            return await UploadFileAsync(stream, fileNameWithTimestamp, file.ContentType);
        }


        public async Task DeleteFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public string GetBlobUrl(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}

