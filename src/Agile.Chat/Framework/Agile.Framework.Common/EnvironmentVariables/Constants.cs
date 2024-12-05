namespace Agile.Framework.Common.EnvironmentVariables;

public static class Constants
{
    #region CosmosDbConstants
    public const string CosmosDatabaseName = "AgileChat";
    
    public const string CosmosAssistantsContainerName = "Assistants";
    public const string CosmosAssistantsPartitionKeyPath = "/id";
    
    public const string CosmosAuditsContainerName = "Audits";
    public const string CosmosAuditsPartitionKeyPath = "/type";
    
    public const string CosmosChatsContainerName = "Chats";
    public const string CosmosChatsPartitionKeyPath = "/type";
    
    public const string CosmosToolsContainerName = "Tools";
    public const string CosmosToolsPartitionKeyPath = "/id";
    
    public const string CosmosFilesContainerName = "Files";
    public const string CosmosFilesPartitionKeyPath = "/id";
    
    public const string CosmosIndexesContainerName = "Indexes";
    public const string CosmosIndexesPartitionKeyPath = "/id";
    #endregion
    
    #region BlobStorageConstants
    public const string BlobContainerName = "index-content";
    #endregion
    
    #region PromptPaths
    public static string ChatCompletionsPromptsPath => Path.Combine(@"ChatCompletions", "Prompts");
    public static class Prompts
    {
        public const string ChatWithRag = "chatWithRag.prompt.yaml";
    }

    #endregion
}