﻿namespace agile_chat_api.Configurations
{
    public static class Constants
    {
        /// <summary>
        /// The file container partition key path
        /// </summary>
        public const string FileContainerPartitionKeyPath = "/id";
        public const string FileUploadContainerName = "fileUploads";

        /// <summary>
        /// The assistant container partition key path
        /// </summary>
        public const string AssistantContainerPartitionKeyPath = "/CreatedBy";
        public const string AssistantContainerName = "assistants";

        /// <summary>
        /// The index container partition key path
        /// </summary>
        public const string IndexContainerPartitionKeyPath = "/id";
        public const string IndexContainerName = "indexes";

        public const string BlobStorageContainerName = "index-content";
    }
}