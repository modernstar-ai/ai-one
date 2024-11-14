namespace agile_chat_api.Configurations
{
    public static class AppConfigs
    {
        /// <summary>
        /// Gets the cosmos endpoint.
        /// </summary>
        /// <value>
        /// The cosmos endpoint.
        /// </value>
        /// <exception cref="System.InvalidOperationException">AZURE_COSMOSDB_URI not set.</exception>
        public static string CosmosEndpoint => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_URI")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_URI not set.");
        /// <summary>
        /// Gets the cosmos key.
        /// </summary>
        /// <value>
        /// The cosmos key.
        /// </value>
        /// <exception cref="System.InvalidOperationException">AZURE_COSMOSDB_KEY not set.</exception>
        public static string CosmosKey => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_KEY")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_KEY not set.");
        /// <summary>
        /// Gets the name of the cosmos database.
        /// </summary>
        /// <value>
        /// The name of the cosmos database.
        /// </value>
        /// <exception cref="System.InvalidOperationException">AZURE_COSMOSDB_DB_NAME not set.</exception>
        public static string CosmosDBName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_DB_NAME")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_DB_NAME not set.");
        /// <summary>
        /// Gets the name of the file container.
        /// </summary>
        /// <value>
        /// The name of the file container.
        /// </value>
        /// <exception cref="System.InvalidOperationException">AZURE_COSMOSDB_FILES_CONTAINER_NAME not set.</exception>
        public static string FileContainerName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_FILES_CONTAINER_NAME")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_FILES_CONTAINER_NAME not set.");

        /// <summary>
        /// Gets the BLOB storage connection string.
        /// </summary>
        /// <value>
        /// The BLOB storage connection string.
        /// </value>
        /// <exception cref="System.InvalidOperationException">AZURE_STORAGE_ACCOUNT_CONNECTION not set.</exception>
        public static string BlobStorageConnectionString => Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION")
            ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_CONNECTION not set.");
        
        public static string AzureOpenAiEndpoint => Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
                                                            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set.");

        public static string AzureOpenAiEmbeddingsDeploymentName =
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME")
            ?? throw new InvalidOperationException("AZURE_OPENAI_API_EMBEDDINGS_DEPLOYMENT_NAME not set");
        
        public static string AzureOpenAiApiKey =
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
            ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY not set");
        
        public static string AzureAiServicesKey =
            Environment.GetEnvironmentVariable("AZURE_AI_SERVICES_KEY")
            ?? throw new InvalidOperationException("AZURE_AI_SERVICES_KEY not set");

        public static string AzureOpenAiEmbeddingsModelName =
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_EMBEDDINGS_MODEL_NAME") ??
            throw new InvalidOperationException("AZURE_OPENAI_API_EMBEDDINGS_MODEL_NAME not set");
    }
}
