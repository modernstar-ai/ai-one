namespace agile_chat_api.Configurations
{
    public static class AppConfigs
    {
        public static string AppInsightsInstrumentationKey =
            Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ??
            throw new InvalidOperationException("APPINSIGHTS_INSTRUMENTATIONKEY not set");
        
        public static string CosmosEndpoint => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_URI")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_URI not set.");

        public static string CosmosKey => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_KEY")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_KEY not set.");

        public static string CosmosDBName => Environment.GetEnvironmentVariable("AZURE_COSMOSDB_DB_NAME")
            ?? throw new InvalidOperationException("AZURE_COSMOSDB_DB_NAME not set.");

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