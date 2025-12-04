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
            
            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = configuration["AzureStorage:ContainerName"] ?? "documents";
            
            // Asegurar que el contenedor existe
            InitializeContainerAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeContainerAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        /// <summary>
        /// Sube un archivo a Azure Blob Storage
        /// </summary>
        /// <param name="fileStream">Stream del archivo a subir</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="contentType">Tipo de contenido del archivo</param>
        /// <returns>URL del blob subido</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions);
            
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Sube un archivo desde un IBrowserFile de Blazor
        /// </summary>
        /// <param name="file">Archivo de Blazor</param>
        /// <param name="customFileName">Nombre personalizado (opcional). Si es null, usa el nombre original</param>
        /// <returns>URL del blob subido</returns>
        public async Task<string> UploadBrowserFileAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, string? customFileName = null)
        {
            var fileName = customFileName ?? file.Name;
            
            // Agregar timestamp para evitar conflictos de nombres
            var fileNameWithTimestamp = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{fileName}";
            
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB máximo
            return await UploadFileAsync(stream, fileNameWithTimestamp, file.ContentType);
        }

        /// <summary>
        /// Elimina un archivo de Azure Blob Storage
        /// </summary>
        /// <param name="fileName">Nombre del archivo a eliminar</param>
        public async Task DeleteFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Obtiene la URL de un blob
        /// </summary>
        /// <param name="fileName">Nombre del archivo</param>
        /// <returns>URL del blob</returns>
        public string GetBlobUrl(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Descarga un archivo de Azure Blob Storage
        /// </summary>
        /// <param name="fileName">Nombre del archivo a descargar</param>
        /// <returns>Stream del archivo</returns>
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}

