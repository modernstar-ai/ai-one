namespace agile_chat_api.Configurations
{
    public static class AppConfigs
    {
        public static string CosmosEndpoint => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_URI")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_URI not set.");

        public static string CosmosKey => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_KEY")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_KEY not set.");

        public static string CosmosDBName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_DB_NAME")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_DB_NAME not set.");

        public static string FileContainerName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_FILES_CONTAINER_NAME")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_FILES_CONTAINER_NAME not set.");

        public static string IndexContainerName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_INDEX_CONTAINER_NAME")
             ?? throw new InvalidOperationException("AZURE_COSMOSDB_INDEX_CONTAINER_NAME not set.");

        public static string BlobStorageConnectionString => Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION")
            ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_CONNECTION not set.");
        
        public static string BlobStorageContainerName => Environment.GetEnvironmentVariable("AZURE_STORAGE_FOLDERS_CONTAINER_NAME")
        ?? throw new InvalidOperationException("AZURE_STORAGE_FOLDERS_CONTAINER_NAME not set.");
    }
}