namespace TabTogetherApi.Services
{

    /// <summary>
    /// Defines the contract for interacting with the image storage service (Azure Blob Storage).
    /// </summary>
    public interface IStorageAccountBlobRepository
    {
        /// <summary>
        /// Uploads an image stream to the storage container.
        /// </summary>
        /// <param name="imageStream">The stream containing the image data.</param>
        /// <param name="fileName">The desired unique name for the blob (e.g., a GUID or unique identifier).</param>
        /// <param name="contentType">The MIME type of the content (e.g., "image/jpeg").</param>
        /// <returns>The URI of the newly uploaded blob.</returns>
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType);

        /// <summary>
        /// Retrieves an image stream from the storage container.
        /// </summary>
        /// <param name="fileName">The name of the blob to retrieve.</param>
        /// <returns>A tuple containing the Stream of the image and its ContentType, or null if the blob is not found.</returns>
        Task<(Stream Stream, string ContentType)?> GetImageAsync(string fileName);
    }
}
