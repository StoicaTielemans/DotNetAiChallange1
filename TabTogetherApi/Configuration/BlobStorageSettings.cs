namespace TabTogetherApi.Configuration
{
        /// <summary>
    /// Configuration model for Azure Blob Storage settings.
    /// Maps to a section in appsettings.json, e.g., "BlobStorage".
    /// </summary>
    public class BlobStorageSettings
    {
        // This key must match the section name in appsettings.json
        public const string ConfigurationSection = "BlobStorage";

        /// <summary>
        /// The connection string for the Azure Storage Account.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// The name of the blob container where images are stored.
        /// </summary>
        public string ContainerName { get; set; } = string.Empty;
    }
}
