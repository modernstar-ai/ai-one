namespace agile_chat_api.Configurations
{
    public static class Constants
    {
        /// <summary>
        /// The file container partition key path
        /// </summary>
        public const string FileContainerPartitionKeyPath = "/id";

        /// <summary>
        /// The assistant container partition key path
        /// </summary>
        public const string AssistantContainerPartitionKeyPath = "/CreatedBy";

        /// <summary>
        /// The BLOB container name
        /// </summary>
        public const string BlobContainerName = "fileuploadsv1";
    }
}