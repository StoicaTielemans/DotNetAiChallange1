using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TabTogetherApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TabTogetherApi.Configuration;

namespace TabTogetherApi.Services
{
    /// <summary>
    /// Concrete implementation of the IStorageAccountBlobRepository using Azure Blob Storage.
    /// </summary>
    public class StorageAccountBlobRepository : IStorageAccountBlobRepository
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<StorageAccountBlobRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageRepository"/> class.
        /// </summary>
        /// <param name="settings">The configured blob storage settings.</param>
        /// <param name="logger">Logger for tracing operations and errors.</param>
        public StorageAccountBlobRepository(
            IOptions<BlobStorageSettings> settings,
            ILogger<StorageAccountBlobRepository> logger)
        {
            _logger = logger;
            var config = settings.Value;

            try
            {
                // Create the service client using the connection string
                var blobServiceClient = new BlobServiceClient(config.ConnectionString);

                // Get the container client instance
                _containerClient = blobServiceClient.GetBlobContainerClient(config.ContainerName);

                // Ensure the container exists (creates it if it doesn't)
                _containerClient.CreateIfNotExists(PublicAccessType.Blob);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize StorageAccountBlobRepository. Check connection string and container name.");
                // In a real application, you might throw a more specific exception or handle this gracefully.
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            try
            {
                _logger.LogInformation("Starting upload of {FileName} with content type {ContentType}.", fileName, contentType);

                // Options to set the content type and overwrite if the file already exists
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                };

                // Upload the stream to the blob
                await blobClient.UploadAsync(imageStream, uploadOptions, CancellationToken.None);

                _logger.LogInformation("Successfully uploaded {FileName}.", fileName);

                // Return the URI of the uploaded blob
                return blobClient.Uri.AbsoluteUri;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "ContainerNotFound")
            {
                _logger.LogError("Upload failed: Container '{ContainerName}' not found.", _containerClient.Name);
                throw new System.InvalidOperationException($"Storage container '{_containerClient.Name}' not found.", ex);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred during blob upload for {FileName}.", fileName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<(Stream Stream, string ContentType)?> GetImageAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            try
            {
                // Check if the blob exists before attempting to download
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob '{FileName}' not found.", fileName);
                    return null;
                }

                _logger.LogInformation("Starting download of {FileName}.", fileName);
                
                // Download the blob content and properties
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();

                // Create a MemoryStream to hold the downloaded data
                // We convert BinaryData to Stream to make it easier for ASP.NET Core controllers to return it.
                var stream = new MemoryStream(downloadResult.Content.ToArray());

                _logger.LogInformation("Successfully downloaded {FileName}. ContentType: {ContentType}", 
                    fileName, downloadResult.Details.ContentType);

                // Return the stream and content type
                return (stream, downloadResult.Details.ContentType);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
            {
                _logger.LogWarning("Get failed: Blob '{FileName}' not found.", fileName);
                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred during blob download for {FileName}.", fileName);
                throw;
            }
        }
    }
}
